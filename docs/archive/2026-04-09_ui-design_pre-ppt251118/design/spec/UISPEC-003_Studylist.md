# UISPEC-003: 스터디리스트 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 문서 ID | UISPEC-003 |
| 화면 ID | SCR-STUDYLIST-001 |
| 버전 | 1.0.0 |
| 작성일 | 2026-04-07 |
| 상태 | 초안 |
| PPT 참조 | Slide 5–7 (Studylist 1안/2안) |
| 구현 파일 | `src/HnVue.UI/Views/StudylistView.xaml` |
| 구현 준수율 | 63% (핵심 구조 구현, Modality·Status·Priority 컬럼 미구현) |
| 관련 MRD | MR-SL-001, MR-SL-002, MR-SL-003 |

---

## 1. 화면 개요

스터디리스트(Study List) 화면은 PACS 서버에서 검색·조회한 기존 검사(Study) 목록을 표시하는 화면이다. 워크리스트가 RIS 기반 신규 오더 목록을 표시하는 것과 달리, 스터디리스트는 이미 촬영이 완료되어 PACS에 저장된 영상 데이터를 기준으로 조회한다.

**주요 기능:**
- PACS 서버 선택 및 연결 상태 표시
- 날짜 범위 및 기간 단축 필터
- 키워드 검색 (환자ID, 환자명, Accession No)
- Study 목록 DataGrid 표시 (Modality 배지, Status, Priority 포함)
- Study 선택 후 촬영 워크플로우 연계 (MR-SL-003)

**설계 원칙:**
- 다크 네이비 테마 일관성 유지 (워크리스트와 동일 레이아웃 언어)
- IEC 62366 터치 타겟 최소 44px 준수 (행 높이)
- 키보드 탐색 지원 (방향키, Enter 선택)

---

## 2. 레이아웃 구조

### 2.1 전체 레이아웃

```
┌──────────────────────────────────────────────────────────────────┐
│  헤더 (48px): [◀] [▶]  Study List                 PACS: [  ▼]  │
├──────────────────────────────────────────────────────────────────┤
│  필터 바 (40px): [오늘][3일][1주][전체][1개월]    [🔍 검색...]  │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  데이터 그리드 (flex:1)                                          │
│  [Modality][Patient ID][환자명][Acc No][검사일][BodyPart][설명]  │
│  [상태][Priority]                                                │
│  ....                                                            │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│  상태 바 (28px): 에러메시지                      24 studies      │
└──────────────────────────────────────────────────────────────────┘
```

**WPF Grid 행 정의:**

| Row | Height | 내용 |
|-----|--------|------|
| 0 | 48px | 헤더 (Prev/Next + 타이틀 + PACS 드롭다운) |
| 1 | Auto | 필터 바 (기간 버튼 + 검색 입력) |
| 2 | * | DataGrid (flex) |
| 3 | Auto | 상태 바 |

### 2.2 툴바 명세

#### 헤더 (Row 0)

| 요소 | 타입 | 스타일/크기 | 바인딩 |
|------|------|------------|--------|
| 이전 버튼 | Button | `HnVue.OutlineButton`, 32×32px, 아이콘 `&#xE76B;` (Segoe MDL2) | `NavigatePreviousCommand` |
| 다음 버튼 | Button | `HnVue.OutlineButton`, 32×32px, 아이콘 `&#xE76C;` (Segoe MDL2) | `NavigateNextCommand` |
| 섹션 타이틀 | TextBlock | `HnVue.SectionHeader`, 텍스트 "Study List" | — |
| PACS 레이블 | TextBlock | 13px, `HnVue.Semantic.Text.Secondary` | — |
| PACS ComboBox | ComboBox | 120px 너비, 30px 높이 | `PacsServers` / `SelectedPacsServer` |

배경: `HnVue.Semantic.Surface.Panel`
하단 테두리: `HnVue.Semantic.Border.Default`, 두께 1px

#### 필터 바 (Row 1)

| 요소 | 타입 | 크기 | 바인딩 |
|------|------|------|--------|
| 오늘 버튼 | Button | Height=28px, Padding=10,0 | `FilterByPeriodCommand("Today")` |
| 3일 버튼 | Button | Height=28px, Padding=10,0 | `FilterByPeriodCommand("3Days")` |
| 1주일 버튼 | Button | Height=28px, Padding=10,0 | `FilterByPeriodCommand("1Week")` |
| 전체 버튼 | Button | Height=28px, Padding=10,0 | `FilterByPeriodCommand("All")` |
| 1개월 버튼 | Button | Height=28px, Padding=10,0 | `FilterByPeriodCommand("1Month")` |
| 검색 입력 | TextBox | 200px 너비, 28px 높이, 라운드 4px | `SearchQuery` (UpdateSourceTrigger=PropertyChanged) |

- 기간 버튼 스타일: `HnVue.OutlineButton`, FontSize=12px
- 기간 버튼 가로 스크롤 허용 (`ScrollViewer` 래핑)
- 검색 입력 배경: `HnVue.Semantic.Surface.Card`
- 검색 입력 테두리: `HnVue.Semantic.Border.Default`
- 검색 아이콘: `&#xE721;` (Segoe MDL2), 12px, `HnVue.Semantic.Text.Secondary`
- 플레이스홀더: "Search studies..."

**활성 기간 버튼 상태:**
- 기본: `HnVue.OutlineButton` (배경 투명, 테두리 있음)
- 선택됨: 배경 `#1f6fc7`, 텍스트 흰색, 테두리 `#2a88e8`

### 2.3 데이터 테이블 명세

#### DataGrid 속성

| 속성 | 값 |
|------|----|
| RowHeight | 44px (IEC 62366 터치 타겟 준수) |
| ColumnHeaderHeight | 36px |
| GridLinesVisibility | Horizontal |
| HorizontalGridLinesBrush | `HnVue.Semantic.Border.Default` |
| IsReadOnly | True |
| AutoGenerateColumns | False |
| SelectionMode | Single |
| Background | Transparent |
| BorderThickness | 0 |
| FontSize | 13px |

#### 컬럼 정의 (설계 목표)

| # | 헤더 | 바인딩 필드 | 너비 | 정렬 | 비고 |
|---|------|------------|------|------|------|
| 1 | Modality | `Modality` | 80px | 중앙 | 배지 렌더링 (템플릿) |
| 2 | Patient ID | `PatientId` | 120px | 좌 | `#7bc8f5` 강조색 |
| 3 | 환자명 | `PatientName` | 120px | 좌 | — |
| 4 | Acc No | `AccessionNumber` | 130px | 좌 | nullable |
| 5 | 검사일 | `StudyDate` | 120px | 좌 | `yyyy-MM-dd HH:mm` |
| 6 | Body Part | `BodyPart` | 90px | 좌 | nullable |
| 7 | Description | `Description` | * | 좌 | flex |
| 8 | Status | `Status` | 90px | 중앙 | 도트 인디케이터 |
| 9 | Priority | `Priority` | 70px | 중앙 | 긴급 배지 |

#### 컬럼 헤더 스타일

- 배경: `#152645` (dark blue header)
- 글자색: `#b8cce0` (`HnVue.Semantic.Text.Secondary`)
- 폰트: 13px / 600
- 정렬: 좌측 정렬 (숫자형은 우측)
- 호버: 배경 `#1a2a44`

#### 행 상태 스타일

| 상태 | 배경 | 좌측 테두리 |
|------|------|------------|
| 기본 | transparent | 없음 |
| 호버 | `rgba(0,174,239,0.08)` | 없음 |
| 선택됨 | `rgba(31,111,199,0.2)` | 2px `#1f6fc7` |
| 긴급 | `rgba(231,76,60,0.06)` | 3px `#e74c3c` |

---

## 3. 컴포넌트 디자인 명세

### 3.1 Modality 배지

Modality 컬럼은 텍스트 대신 색상 배지로 표시한다.

**배지 기본 스타일:**
- 크기: 36×20px (최소)
- 폰트: 10px / 700
- 텍스트 색상: `#ffffff`
- 테두리 반경: 3px
- 정렬: 중앙

**Modality별 배경색:**

| Modality | 배경색 | 표시 텍스트 |
|----------|--------|-----------|
| CR | `#1f6fc7` (primary blue) | CR |
| DR | `#1565a0` | DR |
| CT | `#c0392b` | CT |
| MR | `#2980b9` | MR |
| XR | `#27ae60` | XR |
| US | `#8e44ad` | US |
| NM | `#d35400` | NM |
| 기타 | `#546e7a` | 코드 |

**WPF 구현 패턴:**
```xml
<DataGridTemplateColumn Header="Modality" Width="80">
  <DataGridTemplateColumn.CellTemplate>
    <DataTemplate>
      <Border CornerRadius="3" Padding="4,2"
              Background="{Binding Modality, Converter={StaticResource ModalityToBrushConverter}}"
              HorizontalAlignment="Center">
        <TextBlock Text="{Binding Modality}" FontSize="10" FontWeight="Bold"
                   Foreground="White" HorizontalAlignment="Center"/>
      </Border>
    </DataTemplate>
  </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

### 3.2 Status 인디케이터

Status 컬럼은 컬러 도트와 레이블 텍스트의 조합으로 표시한다.

| Status 값 | 도트 색상 | 레이블 텍스트 | 의미 |
|-----------|----------|--------------|------|
| Completed | `#2ecc71` | 완료 | 판독 완료 |
| InProgress | `#3498db` | 진행 중 | 판독 중 |
| Pending | `#f39c12` | 대기 | 판독 대기 |
| Cancelled | `#546e7a` | 취소 | 취소됨 |
| Urgent | `#e74c3c` | 긴급 | 긴급 처리 |

- 도트 크기: 8×8px, border-radius: 50%
- 레이블 폰트: 11px / 400, `#b8cce0`
- 도트-레이블 간격: 5px

**WPF 구현 패턴:**
```xml
<DataGridTemplateColumn Header="Status" Width="90">
  <DataGridTemplateColumn.CellTemplate>
    <DataTemplate>
      <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" VerticalAlignment="Center">
        <Ellipse Width="8" Height="8" Margin="0,0,5,0"
                 Fill="{Binding Status, Converter={StaticResource StatusToBrushConverter}}"/>
        <TextBlock Text="{Binding Status, Converter={StaticResource StatusToTextConverter}}"
                   FontSize="11" Foreground="#b8cce0"/>
      </StackPanel>
    </DataTemplate>
  </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

### 3.3 Priority 인디케이터

Priority 컬럼은 우선순위별 아이콘과 색상으로 표시한다.

| Priority 값 | 색상 | 표시 |
|------------|------|------|
| Urgent | `#e74c3c` | ▲ 긴급 |
| High | `#f39c12` | ↑ 높음 |
| Normal | `#b8cce0` | — 보통 |
| Low | `#546e7a` | ↓ 낮음 |

### 3.4 PACS 서버 선택 ComboBox

- 너비: 120px, 높이: 30px
- 배경: `HnVue.Semantic.Surface.Card` (`#1a2540`)
- 테두리: `HnVue.Semantic.Border.Default` (`#2e4070`)
- 포커스 테두리: `HnVue.Semantic.Brand.Accent` (`#00aeef`)
- 텍스트: `#e0e6f0`, 13px
- 드롭다운 배경: `#101a2e`

**연결 상태 표시:**

| 상태 | 표시 방식 |
|------|----------|
| 연결됨 | 텍스트 앞 녹색 도트 (8px, `#2ecc71`) |
| 연결 중 | 텍스트 앞 황색 도트 (8px, `#f39c12`, 펄싱) |
| 연결 실패 | 텍스트 앞 적색 도트 (8px, `#e74c3c`) |

---

## 4. 색상 토큰 매핑

### 화면별 토큰 사용

| UI 영역 | CSS 변수 (HTML 목업) | WPF 토큰 | 실제 색상 |
|---------|---------------------|----------|----------|
| 앱 전체 배경 | `--color-bg-app` | `HnVue.Semantic.Surface.Page` | `#0d1527` |
| 패널 배경 | `--color-bg-surface` | `HnVue.Semantic.Surface.Panel` | `#1a2540` |
| 헤더/필터 배경 | `--color-bg-header-wl` | `HnVue.Semantic.Surface.Panel` | `#152035` |
| 테이블 헤더 배경 | `--color-bg-table-head` | (신규 토큰 필요) | `#152645` |
| 입력 배경 | `--color-bg-search` | `HnVue.Semantic.Surface.Card` | `#152035` |
| 기본 테두리 | `--color-border` | `HnVue.Semantic.Border.Default` | `#2e4070` |
| 행 구분선 | `--color-border-row` | `HnVue.Semantic.Border.Default` | `#1e2e48` |
| 기본 텍스트 | `--color-text-primary` | `HnVue.Semantic.Text.Primary` | `#e0e6f0` |
| 보조 텍스트 | `--color-text-secondary` | `HnVue.Semantic.Text.Secondary` | `#b8cce0` |
| 뮤트 텍스트 | `--color-text-muted` | `HnVue.Semantic.Text.Muted` | `#7090b0` |
| Patient ID 강조 | `--color-accent-blue` | `HnVue.Semantic.Brand.Accent` | `#7bc8f5` |
| 섹션 배지 배경 | `--color-badge-section` | `HnVue.Component.SectionBadge.Bg` | `#2a3a5c` |
| 섹션 배지 텍스트 | `--color-accent-yellow` | `HnVue.Component.SectionBadge.Text` | `#f9e04b` |

### 상태 색상 (IEC 62366 준수)

| 상태 | 색상 | 토큰 | 용도 |
|------|------|------|------|
| 완료/정상 | `#2ecc71` | `HnVue.Semantic.Status.Safe` | 완료된 검사 |
| 진행 중 | `#3498db` | `HnVue.Semantic.Status.Info` | 판독 진행 중 |
| 대기/경고 | `#f39c12` | `HnVue.Semantic.Status.Warning` | 판독 대기 |
| 긴급/에러 | `#e74c3c` | `HnVue.Semantic.Status.Emergency` | 긴급 케이스 |
| 취소/비활성 | `#546e7a` | `HnVue.Semantic.Text.Disabled` | 취소된 검사 |

---

## 5. 상태 디자인

### 5.1 화면 로딩 상태

| 상태 | UI 표현 |
|------|---------|
| 초기 로드 | 스켈레톤 행 3개 표시 (회색 박스 애니메이션) |
| PACS 연결 중 | 상태 바에 "서버 연결 중..." + 스피너 |
| 조회 중 | 테이블 오버레이 반투명 + 스피너 중앙 |
| 데이터 없음 | 중앙에 아이콘 + "검색 결과가 없습니다" 텍스트 |
| PACS 연결 실패 | 상태 바에 적색 에러 메시지 |

### 5.2 검색 상태

| 상태 | UI 표현 |
|------|---------|
| 기간 필터 선택됨 | 해당 버튼 배경 `#1f6fc7`, 텍스트 흰색 |
| 키워드 검색 활성 | 검색 입력 테두리 `#00aeef` (포커스 링) |
| 검색 결과 하이라이트 | 검색어 매칭 텍스트 배경 `rgba(0,174,239,0.2)` |

### 5.3 선택 상태

| 상태 | UI 표현 |
|------|---------|
| 행 호버 | 배경 `rgba(0,174,239,0.08)` |
| 행 선택됨 | 배경 `rgba(31,111,199,0.2)` + 좌측 2px `#1f6fc7` 테두리 |
| 더블클릭 | 검사 상세 모달 열림 |

### 5.4 PACS 서버 상태

| 상태 | 표현 |
|------|------|
| 연결됨 | ComboBox 항목 앞 녹색 도트, 상태 바 "Connected" |
| 연결 실패 | ComboBox 항목 앞 적색 도트, 상태 바 에러 메시지 |
| 미선택 | 빈 목록 표시, "PACS 서버를 선택하세요" 안내 |

---

## 6. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 내용 | Tier | 구현 요소 |
|--------|-------------|------|----------|
| MR-SL-001 | PACS 스터디 목록 표시 | Tier 2 | DataGrid + PACS ComboBox + 조회 API |
| MR-SL-002 | 날짜/Modality 필터링 | Tier 2 | 기간 단축 버튼 + Modality 사이드 필터 (미구현) |
| MR-SL-003 | 검사 선택 후 촬영 워크플로우 연계 | Tier 1 | 행 선택 → Acquisition 화면 전환 |

---

## 7. 구현 갭 분석

현재 `StudylistView.xaml` (구현 준수율: 63%)와 설계 명세 간의 갭.

### 구현 완료 항목 (구현됨)

| 항목 | 구현 상태 |
|------|----------|
| Prev/Next 네비게이션 화살표 | 완료 |
| PACS 서버 ComboBox | 완료 |
| 기간 필터 버튼 (Today/3Days/1Week/All/1Month) | 완료 |
| 검색 입력 박스 (SearchQuery 바인딩) | 완료 |
| DataGrid (PatientId, AccNo, StudyDate, BodyPart, Description) | 완료 |
| 상태 바 (에러 메시지 + 건수) | 완료 |

### 미구현 항목 (갭)

| 항목 | 우선순위 | 영향 MRD | 설명 |
|------|---------|---------|------|
| Modality 배지 컬럼 | 높음 | MR-SL-001 | CT/MR/XR 등 배지 렌더링 없음 |
| Status 컬럼 | 높음 | MR-SL-001 | 검사 상태 인디케이터 없음 |
| Priority 인디케이터 | 중간 | MR-SL-003 | 긴급 케이스 식별 불가 |
| 환자명 컬럼 | 중간 | MR-SL-001 | PatientName 컬럼 없음 |
| 행 긴급 좌측 테두리 색상 | 낮음 | — | 긴급 행 시각적 강조 없음 |
| Modality 사이드 필터 | 낮음 | MR-SL-002 | Modality별 체크박스 필터 없음 |
| 행 더블클릭 상세 모달 | 낮음 | MR-SL-001 | 상세 조회 없음 |
| PACS 연결 상태 표시 | 중간 | MR-SL-001 | 연결/실패 시각 피드백 없음 |

---

## 8. 개선 우선순위

### Phase 1 — 즉시 구현 (MRD Tier 1/2 직결)

1. **Modality 배지 컬럼 추가**
   - `ModalityToBrushConverter` 구현
   - DataGridTemplateColumn 추가
   - 파일: `StudylistView.xaml` + 새 Converter

2. **Status 인디케이터 컬럼 추가**
   - `StatusToBrushConverter`, `StatusToTextConverter` 구현
   - DataGridTemplateColumn 추가

3. **PatientName 컬럼 추가**
   - `StudyRecord` 모델에 `PatientName` 프로퍼티 확인/추가
   - DataGridTextColumn 추가

### Phase 2 — 단기 구현 (UX 향상)

4. **Priority 인디케이터**
   - `PriorityToBrushConverter` 구현
   - 긴급 행 좌측 테두리 VisualState 추가

5. **PACS 연결 상태 표시**
   - ComboBox 항목 커스텀 템플릿 (도트 + 서버명)
   - ViewModel에 `PacsConnectionStatus` 프로퍼티 추가

### Phase 3 — 장기 구현 (기능 확장)

6. **Modality 사이드 필터 패널** (선택사항)
7. **검사 상세 모달** (더블클릭)
8. **썸네일 그리드 뷰** (HTML 목업의 카드 뷰 모드)
