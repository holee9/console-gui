# UISPEC-002: 워크리스트 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| 버전 | v1.0 |
| 상태 | Draft |
| 작성일 | 2026-04-07 |
| PPT 참조 | Slide 4 (디자인 스크린샷), Slide 5 (HTML 코드), Slide 6 (디자인 토큰) |
| HTML 목업 | `docs/ui_mockups/02-worklist.html` |
| 구현 파일 | `src/HnVue.UI/Views/PatientListView.xaml` |
| ViewModel | `src/HnVue.UI/ViewModels/PatientListViewModel.cs` |
| 관련 SPEC | SPEC-UI-002 |
| 현재 준수율 | 약 44% (PPT 명세 대비) |

---

## 1. 화면 개요

워크리스트 화면은 HnVue의 메인 작업 화면이다. 방사선 검사 환자 목록을 관리하고 Study 상세 정보를 확인하며, 촬영 워크플로우를 제어하는 허브 역할을 한다.

**핵심 구성:**
- 앱 헤더 (30px) — 로고 + 네비게이션 탭 + 사용자 정보
- 워크리스트 툴바 (32px) — 섹션 배지 + 액션 버튼들
- 필터 바 (28px) — 날짜/Modality/상태/키워드 필터
- 데이터 테이블 (가변 높이) — 12컬럼 DataGrid
- 우측 Detail Panel (230px) — Study 상세 + 뷰어 미리보기

**전체 레이아웃:** 가로 2분할 — 워크리스트 영역 (flex:1) + Detail Panel (230px 고정)

---

## 2. 레이아웃 구조

### 2.1 전체 레이아웃

```
Window (전체 화면)
└── Body (flex-column, height:100vh, bg:#1a2540)
    ├── AppHeader (height:30px, bg:#0d1b36, flex-shrink:0)
    └── MainLayout (flex:1, flex-row, overflow:hidden)
        ├── WorklistArea (flex:1, flex-column, border-right:2px #2e4070)
        │   ├── WorklistToolbar (height:~32px, bg:#152035, flex-shrink:0)
        │   ├── FilterBar (height:~28px, bg:#1a2a44, flex-shrink:0)
        │   └── TableWrapper (flex:1, overflow:auto)
        │       └── WorklistTable (DataGrid, 12 columns)
        └── DetailPanel (width:230px, bg:#101a2e, flex-column)
            ├── DetailHeader (bg:#0d1527)
            ├── PatientSearch (border-bottom:1px #1e2e48)
            ├── InfoSection × 2
            ├── ToggleRow (Suspend / Hide)
            ├── ViewerPreview (flex:1, bg:#000)
            └── ActionButtons (border-top:1px #1e2e48)
```

**WPF 구현 구조 (PatientListView.xaml):**
```
Grid (RowDefinitions: Auto/Auto/Auto/*/Auto)
├── Row 0: 헤더 (섹션배지 + 타이틀 + 카운트 + 검색박스), Height=44
├── Row 1: 기간 필터 버튼 (전체/오늘/3일/1주일/1개월), Height=36
├── Row 2: 액션 버튼 (새로등록/현재촬영/판독완료/삭제), Height=40
├── Row 3: Grid (ColumnDefinitions: */230)
│   ├── Col 0: DataGrid (워크리스트)
│   └── Col 1: Detail Panel (230px)
└── Row 4: StatusBar (예정)
```

### 2.2 앱 헤더 명세

| 속성 | 값 |
|------|-----|
| Height | 30px |
| Background | `#0d1b36` |
| Border-Bottom | `1px solid #2e4070` |
| Padding | `4px 10px` |
| Layout | flex-row, align-items:center, gap:12px |

**자식 요소:**

| 요소 | 내용 | 스타일 |
|------|------|--------|
| AppLogo | "HnVUE" | 15px/700/`#7bc8f5`, letter-spacing:1px |
| AppMenu | 탭 버튼 그룹 | flex:1, gap:2px |
| HeaderRight | 날짜 + 사용자명 + 로그아웃 | 11px/`#a0b4cc` |

**AppMenu 탭 목록:**

| 탭 | 상태 |
|----|------|
| Worklist | Active (현재 화면) |
| Study | 비활성 |
| Report | 비활성 |
| Teaching File | 비활성 |
| Admin | 비활성 |
| Statistics | 비활성 |

**탭 버튼 스타일:**
- 기본: `background:none, color:#a0b4cc, 11px, padding:3px 8px, radius:3px`
- Active/Hover: `background:#2e4070, color:#ffffff`

**HeaderRight:**
- 날짜 텍스트: `#a0b4cc, 11px`
- 사용자명: `#7bc8f5` (강조 색상)
- 로그아웃 버튼: `#a0b4cc`

### 2.3 툴바 명세

| 속성 | 값 |
|------|-----|
| Height | ~32px |
| Background | `#152035` |
| Border-Bottom | `1px solid #2e4070` |
| Padding | `5px 8px` |
| Layout | flex-row, align-items:center, gap:6px |

**자식 요소 순서:**

| 순서 | 요소 | 내용 | 스타일 |
|------|------|------|--------|
| 1 | SectionBadge | "1. Worklist 목록" | 12px/700/`#f9e04b`, bg:`#2a3a5c`, pad:2px 8px, radius:3px |
| 2 | ToolbarBtn (Primary) | "새로등록" | bg:`#1f6fc7`, border:`#2a88e8`, color:white |
| 3 | ToolbarBtn | "현재촬영" | bg:`#253555`, border:1px `#3a5080`, color:`#c0d0e8` |
| 4 | ToolbarBtn | "판독완료" | 동일 |
| 5 | ToolbarBtn | "삭제" | 동일 |
| 6 | Spacer | — | flex:1 |
| 7 | TotalLabel | "Total : N" | 11px/600/`#7bc8f5` |

**ToolbarBtn 스타일:**
- 기본: `background:#253555, border:1px solid #3a5080, color:#c0d0e8, 11px, padding:2px 8px, radius:3px`
- Hover: `background:#3a5080, color:#ffffff`
- Primary (새로등록): `background:#1f6fc7, border-color:#2a88e8, color:#ffffff`

### 2.4 필터 바 명세

| 속성 | 값 |
|------|-----|
| Height | ~28px |
| Background | `#1a2a44` |
| Border-Bottom | `1px solid #2e4070` |
| Padding | `4px 8px` |
| Layout | flex-row wrap, align-items:center, gap:6px |

**필터 요소 순서:**

| 순서 | 요소 | 타입 | 크기 |
|------|------|------|------|
| 1 | FilterLabel "기간" | Label | 10px/`#7090b0` |
| 2 | DateInput (From) | DatePicker/TextBox | height:20px |
| 3 | Tilde "~" | Label | — |
| 4 | DateInput (To) | DatePicker/TextBox | height:20px |
| 5 | FilterDivider | — | 1px wide, 16px tall, `#3a5080` |
| 6 | FilterLabel "검사유형" | Label | — |
| 7 | ModalitySelect | ComboBox | min-width:80px |
| 8 | FilterDivider | — | — |
| 9 | FilterLabel "상태" | Label | — |
| 10 | StatusSelect | ComboBox | min-width:80px |
| 11 | FilterDivider | — | — |
| 12 | FilterLabel "키워드" | Label | — |
| 13 | KeywordInput | TextBox | — |
| 14 | SearchButton | Button (Primary) | — |

**현재 구현 현황:** 날짜 필터는 기간 버튼(전체/오늘/3일/1주일/1개월) 방식으로 구현됨. PPT의 From/To DatePicker + Modality/Status 드롭다운 방식과 다름 → **갭 항목**

**Input/Select 스타일:**
- background: `#0d1b36`
- border: `1px solid #3a5080`
- color: `#c0d0e8`
- font-size: `10px`
- padding: `2px 5px`
- height: `20px`
- radius: `3px`

### 2.5 데이터 테이블 명세

#### 테이블 컨테이너

| 속성 | 값 |
|------|-----|
| Layout | flex:1, overflow:auto |
| 배경 (짝수 행) | `#0d1527` |
| 배경 (홀수 행) | `#111f3a` |
| 행 호버 | `#1e3558` (transition:0.1s) |
| 행 선택 | `#1f4878` |
| 행 높이 | 24px (PPT 명세) / 현재 36px (WPF 구현) |

#### 테이블 헤더

| 속성 | 값 |
|------|-----|
| Background | `#152645` |
| Foreground | `#90b0d0` |
| Height | 28px |
| Padding | `5px 6px` |
| Font | 10px/600 |
| Border | `1px solid #2a3a58` |
| Position | sticky top:0 |

#### 12 컬럼 정의

| 순번 | 컬럼명 | 타입 | 너비 | 정렬 | 정렬가능 | 특이사항 |
|------|--------|------|------|------|----------|---------|
| 1 | (체크박스) | CheckBox | 36px | Center | No | 전체 선택 |
| 2 | # (순번) | Text | 40px | Center | No | — |
| 3 | 환자ID | Text | 100px | Left | Yes | `#7bc8f5`, Bold |
| 4 | 환자이름 | Text | 100px | Left | Yes | `#e0e8f5` |
| 5 | 성별 | Text | 50px | Center | Yes | — |
| 6 | 생년월일 | Text | 100px | Center | Yes | yyyy-MM-dd |
| 7 | 등록일시 | Text | 130px | Left | Yes | yyyy-MM-dd HH:mm |
| 8 | 검사일시 | Text | 130px | Left | Yes | **미구현** — PPT 명세 |
| 9 | 검사유형 | ModalityBadge | 70px | Center | No | CT/MR/XR/US/NM 배지 — **미구현** |
| 10 | 진단명 | Text | 120px | Left | No | StudyDescription |
| 11 | 담당의 | Text | 90px | Left | No | — |
| 12 | 상태 | StatusIndicator | 80px | Center | Yes | 점(dot) 색상 인디케이터 — **미구현** |

**현재 WPF 구현 컬럼 (PatientListView.xaml):**

| 컬럼명 | 바인딩 | 너비 |
|--------|--------|------|
| (체크박스) | IsSelected | 36 |
| 환자ID | PatientId | 120 |
| 환자이름 | Name | 140 |
| 성별 | Sex | 50 |
| 생년월일 | DateOfBirth | 110 |
| 등록일시 | CreatedAt | 140 |
| 상태 | IsEmergency (Badge) | 80 |
| 담당자 | CreatedBy | * |

#### 행 좌측 테두리 (긴급도 표시)

PPT 명세에서 행의 긴급 상태를 좌측 3px 컬러 테두리로 표시한다.

| 상태 클래스 | 테두리 색상 | 의미 |
|-----------|------------|------|
| stat-urgent | `#e74c3c` (빨간색) | 긴급 |
| stat-normal | `#2ecc71` (초록색) | 정상 |
| stat-waiting | `#f39c12` (주황색) | 대기 |

**현재 구현:** `IsEmergency` 필드 기반 Badge 방식 → 좌측 테두리 방식으로 변경 필요 (**갭**)

### 2.6 우측 Detail Panel 명세

#### 패널 컨테이너

| 속성 | 값 |
|------|-----|
| Width | 230px (고정) |
| flex-shrink | 0 |
| Background | `#101a2e` |
| Layout | flex-column |
| 좌측 테두리 | `2px solid #2e4070` |

#### 패널 구성 요소 (위에서 아래 순서)

**1. DetailHeader**

| 속성 | 값 |
|------|-----|
| Background | `#0d1527` |
| Padding | `5px 8px` |
| Border-Bottom | `1px solid #2e4070` |
| 제목 텍스트 | "Study 상세" |
| 제목 스타일 | 11px/700/`#7bc8f5` |

**2. PatientSearch**

| 속성 | 값 |
|------|-----|
| Padding | `6px 8px` |
| Border-Bottom | `1px solid #1e2e48` |
| Input Background | `#152035` |
| Input Border | `1px solid #3a5080` |
| Input Color | `#c0d0e8` |
| Input Font | 11px |
| Input Padding | `3px 6px` |
| Input Radius | 3px |
| Placeholder | "Input Patient Name..." color:`#5580a0` |

**3. InfoSection × 2 (환자 정보 + Study 정보)**

| 속성 | 값 |
|------|-----|
| Padding | `6px 8px` |
| Border-Bottom | `1px solid #1e2e48` |
| 섹션 타이틀 | 10px/600/`#5580a0`, uppercase, letter-spacing:0.5px |
| Info Row | flex space-between, margin-bottom:3px |
| Key 색상 | `#7090b0` |
| Value 색상 (기본) | `#d0e0f5` weight:500 |
| Value 색상 (강조) | `#f9e04b` (.highlight) |
| Value 색상 (긴급) | `#e74c3c` weight:700 (.urgent) |

**InfoSection 항목 예시 (환자 정보):**

| Key | Value 예시 |
|-----|-----------|
| 환자ID | P240001 |
| 이름 | 홍길동 |
| 성별 | M |
| 생년월일 | 1980-01-15 |
| 나이 | 44 |

**InfoSection 항목 예시 (Study 정보):**

| Key | Value 예시 |
|-----|-----------|
| 검사유형 | XR |
| 검사부위 | CHEST |
| 진단명 | — |
| 검사일시 | 2024-01-15 10:30 |
| 우선순위 | 긴급 (빨간색) |

**4. ToggleRow**

| 속성 | 값 |
|------|-----|
| Padding | `3px 8px` |
| Font | 10px/`#8090a8` |
| 요소 | Suspend 체크박스 + Hide 체크박스 |

**5. ViewerPreview**

| 속성 | 값 |
|------|-----|
| flex | 1 (남은 공간 전체) |
| Background | `#000000` |
| min-height | 100px |
| Position | relative |
| Overlay-Info | absolute, 9px/`#7bc8f5` (좌상/우상/좌하단) |

뷰어 오버레이 정보 위치:
- 좌상단: 환자명, 나이/성별
- 우상단: 검사날짜, 장비명
- 좌하단: Window/Level 값

**6. ActionButtons**

| 속성 | 값 |
|------|-----|
| Padding | `6px 8px` |
| Layout | flex wrap, gap:4px |
| Border-Top | `1px solid #1e2e48` |

| 버튼 | 크기 | 스타일 |
|------|------|--------|
| 뷰어 열기 | flex:1 0 100% | bg:`#1a6fc7`, border:`#2a88e8`, 11px/600/white, pad:4px |
| 담당의 | flex:1 0 calc(50%-4px) | bg:`#1a2e4e`, border:`#2e4a70`, 10px/`#a0b8d5`, pad:3px 4px, radius:3px |
| 전달 검색 | 동일 | 동일 |
| 출력 | 동일 | 동일 |
| 통보 | 동일 | 동일 |
| 삭제 | 동일 | 동일 |

---

## 3. 컴포넌트 디자인 명세

### 3.1 Modality 배지

검사 유형을 색상 코드 배지로 표시한다.

| Modality | 배경 색상 | 표시 텍스트 |
|----------|---------|------------|
| CT | `#c0392b` (빨간색) | CT |
| MR | `#2980b9` (파란색) | MR |
| XR | `#27ae60` (초록색) | XR |
| US | `#8e44ad` (보라색) | US |
| NM | `#d35400` (주황색) | NM |

**배지 공통 스타일:**
- Font: 10px / 700 / `#ffffff`
- Padding: `1px 4px`
- Border-Radius: 2px
- Display: inline-block

**WPF 구현 시 DataTrigger 패턴:**
```xml
<DataGridTemplateColumn Header="검사유형" Width="70">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Border CornerRadius="2" Padding="4,1" HorizontalAlignment="Center">
                <!-- Background: DataTrigger on Modality property -->
                <TextBlock Text="{Binding Modality}" 
                           Foreground="White" FontSize="10" FontWeight="Bold"/>
            </Border>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

### 3.2 Status 인디케이터 (점)

검사 상태를 컬러 점(dot) + 텍스트 조합으로 표시한다.

| 상태 | 점 색상 | 레이블 |
|------|--------|--------|
| Reported (판독완료) | `#2ecc71` (초록) | Reported |
| Pending (대기) | `#f39c12` (주황) | Pending |
| InProgress (진행중) | `#3498db` (파란) | In Progress |
| Urgent (긴급) | `#e74c3c` (빨간) | Urgent |

**점(dot) 스타일:**
- Width/Height: 8x8px
- Border-Radius: 50% (완전한 원)
- Display: inline-block
- Margin-Right: 3px

**WPF 구현 시 Ellipse 사용:**
```xml
<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <Ellipse Width="8" Height="8" Margin="0,0,3,0">
        <!-- Fill: DataTrigger on Status property -->
    </Ellipse>
    <TextBlock Text="{Binding StatusLabel}" FontSize="11"/>
</StackPanel>
```

### 3.3 행 좌측 테두리 (긴급도 표시)

DataGrid 행의 좌측에 3px 색상 테두리를 표시하여 긴급도를 시각화한다.

| 긴급도 | 테두리 색상 | CSS/WPF |
|--------|-----------|---------|
| 긴급 (Urgent) | `#e74c3c` | border-left: 3px solid #e74c3c |
| 정상 (Normal) | `#2ecc71` | border-left: 3px solid #2ecc71 |
| 대기 (Waiting) | `#f39c12` | border-left: 3px solid #f39c12 |

**WPF 구현 방안:** DataGridRow의 BorderBrush, BorderThickness를 DataTrigger로 제어

---

## 4. 색상 토큰 매핑

### 전체 색상 토큰 (slide6.xml SECTION 1 기준)

#### 배경 토큰

| 토큰 이름 | Hex 값 | WPF 키 (매핑) | 사용처 |
|----------|--------|--------------|--------|
| --color-bg-app | `#0d1527` | `HnVue.Core.Color.BgApp` | 전체 앱 배경 |
| --color-bg-surface | `#1a2540` | `HnVue.Semantic.Surface.Panel` | 일반 표면 |
| --color-bg-panel | `#101a2e` | `HnVue.Semantic.Surface.Panel` | Detail Panel |
| --color-bg-header-app | `#0d1b36` | `HnVue.Semantic.Surface.Card` | 앱 헤더 |
| --color-bg-header-wl | `#152035` | `HnVue.Component.Toolbar.Bg` | 워크리스트 툴바 |
| --color-bg-filter | `#1a2a44` | `HnVue.Component.FilterBar.Bg` | 필터 바 |
| --color-bg-table-head | `#152645` | `HnVue.Component.DataGrid.HeaderBg` | 테이블 헤더 |
| --color-bg-search | `#152035` | `HnVue.Component.DataGrid.SearchBg` | 검색 입력 |
| --color-bg-detail-head | `#0d1527` | `HnVue.Component.DetailPanel.HeaderBg` | Detail 헤더 |

#### 텍스트 토큰

| 토큰 이름 | Hex 값 | WPF 키 (매핑) | 사용처 |
|----------|--------|--------------|--------|
| --color-text-primary | `#e0e6f0` | `HnVue.Semantic.Text.Primary` | 주요 텍스트 |
| --color-text-secondary | `#b8cce0` | `HnVue.Semantic.Text.Secondary` | 테이블 셀 |
| --color-text-label | `#90b0d0` | `HnVue.Component.DataGrid.HeaderText` | 테이블 헤더 |
| --color-text-muted | `#7090b0` | `HnVue.Semantic.Text.Muted` | info-key, 필터 레이블 |
| --color-text-placeholder | `#5580a0` | `HnVue.Semantic.Text.Placeholder` | 입력 플레이스홀더 |
| --color-text-nav | `#a0b4cc` | `HnVue.Semantic.Text.Nav` | 네비게이션 버튼 |
| --color-text-panel-id | `#7bc8f5` | `HnVue.Semantic.Brand.Accent` | 환자ID, 패널 제목 |

#### 강조 및 상태 토큰

| 토큰 이름 | Hex 값 | WPF 키 (매핑) | 사용처 |
|----------|--------|--------------|--------|
| --color-accent-blue | `#7bc8f5` | `HnVue.Semantic.Brand.Accent` | 로고, 환자ID, 패널 제목 |
| --color-accent-yellow | `#f9e04b` | `HnVue.Semantic.Brand.SectionBadge` | 섹션 배지 텍스트 |
| --color-badge-section | `#2a3a5c` | `HnVue.Component.SectionBadge.Bg` | 섹션 배지 배경 |
| --color-primary | `#1f6fc7` | `HnVue.Semantic.Brand.Primary` | Primary 버튼 |
| --color-primary-hover | `#2a88e8` | — | Primary 버튼 호버 |
| --color-btn-default | `#253555` | `HnVue.Component.Toolbar.BtnBg` | 기본 버튼 배경 |
| --color-btn-hover | `#3a5080` | `HnVue.Component.Toolbar.BtnHover` | 기본 버튼 호버 |

#### 경계선 토큰

| 토큰 이름 | Hex 값 | WPF 키 (매핑) | 사용처 |
|----------|--------|--------------|--------|
| --color-border | `#2e4070` | `HnVue.Semantic.Border.Default` | 기본 구분선 |
| --color-border-table | `#2a3a58` | `HnVue.Component.DataGrid.GridLine` | 테이블 헤더 |
| --color-border-row | `#1e2e48` | `HnVue.Component.DataGrid.RowLine` | 행 구분선 |
| --color-border-input | `#3a5080` | `HnVue.Semantic.Border.Input` | 입력 필드 테두리 |

#### 상태 색상 토큰

| 토큰 이름 | Hex 값 | WPF 키 (매핑) | 사용처 |
|----------|--------|--------------|--------|
| --color-status-done | `#2ecc71` | `HnVue.Semantic.Status.Done` | 판독완료 dot, 정상 행 테두리 |
| --color-status-pending | `#f39c12` | `HnVue.Semantic.Status.Pending` | 대기 dot, 대기 행 테두리 |
| --color-status-progress | `#3498db` | `HnVue.Semantic.Status.Progress` | 진행중 dot |
| --color-status-urgent | `#e74c3c` | `HnVue.Semantic.Status.Emergency` | 긴급 dot, 긴급 행 테두리 |

#### Modality 배지 색상 토큰

| 토큰 이름 | Hex 값 | WPF 키 | Modality |
|----------|--------|--------|----------|
| --color-badge-ct | `#c0392b` | `HnVue.Badge.CT` | CT |
| --color-badge-mr | `#2980b9` | `HnVue.Badge.MR` | MR |
| --color-badge-xr | `#27ae60` | `HnVue.Badge.XR` | XR |
| --color-badge-us | `#8e44ad` | `HnVue.Badge.US` | US |
| --color-badge-nm | `#d35400` | `HnVue.Badge.NM` | NM |

#### DataGrid 행 상태 색상

| 토큰 이름 | Hex 값 | WPF 키 | 사용처 |
|----------|--------|--------|--------|
| --color-row-hover | `#1e3558` | `HnVue.Component.DataGrid.RowHover` | 마우스 오버 |
| --color-row-selected | `#1f4878` | `HnVue.Component.DataGrid.SelectionBg` | 선택된 행 |

---

## 5. 상태 디자인

### 5.1 DataGrid 행 상태

| 상태 | 배경 색상 | 추가 효과 |
|------|---------|---------|
| 기본 (짝수) | `#0d1527` | — |
| 기본 (홀수) | `#111f3a` | — |
| 마우스 오버 | `#1e3558` | transition: background 0.1s |
| 선택됨 | `#1f4878` | — |
| 긴급 (Urgent) | 기본 + 좌측 3px `#e74c3c` 테두리 | — |
| 정상 (Normal) | 기본 + 좌측 3px `#2ecc71` 테두리 | — |
| 대기 (Waiting) | 기본 + 좌측 3px `#f39c12` 테두리 | — |

### 5.2 버튼 상태

| 버튼 유형 | 기본 | 호버 | 비활성 |
|----------|------|------|--------|
| Primary (새로등록) | bg:`#1f6fc7` | bg:`#2a88e8` | opacity:0.5 |
| Secondary (현재촬영 등) | bg:`#253555` | bg:`#3a5080` | opacity:0.5 |
| Action Btn (패널) | bg:`#1a2e4e` | bg:`#2a4070` | — |
| ActionBtn.open (뷰어 열기) | bg:`#1a6fc7` | bg:`#2a88e8` | — |

### 5.3 입력 필드 상태

| 상태 | 테두리 색상 |
|------|------------|
| 기본 | `#3a5080` (1px) |
| 포커스 | `#1a6fc7` (implied) |
| 에러 | `#e74c3c` |

### 5.4 스크롤바 스타일

| 속성 | 값 |
|------|-----|
| Width | 5px |
| Radius | 3px |
| Track | `#0d1527` |
| Thumb | `#2e4070` |
| Thumb Hover | `#3a5080` |

---

## 6. MRD/PRD 트레이서빌리티

| MRD ID | 요구사항 | 티어 | 화면 요소 | 구현 상태 |
|--------|---------|------|----------|----------|
| MR-WL-001 | 시스템은 환자/Study 목록을 표시해야 한다 | Tier 1 | DataGrid (PatientListView) | 부분 구현 |
| MR-WL-002 | 시스템은 Modality/날짜/상태 필터링을 지원해야 한다 | Tier 2 | FilterBar | 미구현 (기간 버튼만 구현됨) |
| MR-WL-003 | 시스템은 긴급도 인디케이터를 표시해야 한다 | Tier 1 | 행 좌측 테두리 + Status dot | 부분 구현 (Badge만) |
| MR-WL-004 | 시스템은 Study Detail Panel을 제공해야 한다 | Tier 2 | 230px Detail Panel | 미구현 |
| MR-WL-005 | 시스템은 Modality 배지를 표시해야 한다 | Tier 3 | ModalityBadge 컴포넌트 | 미구현 |
| MR-WL-006 | 시스템은 앱 헤더 네비게이션을 제공해야 한다 | Tier 2 | AppHeader + 탭 | 미구현 (단일 뷰 구조) |

---

## 7. 구현 갭 분석

### 7.1 구현된 항목

| 항목 | PPT 명세 | 현재 구현 | 비고 |
|------|---------|----------|------|
| 섹션 배지 | 황색 텍스트/남색 배경 | HnVue.Component.SectionBadge.Bg 토큰 | 일치 |
| 카운트 배지 | "Total : N" | Patients.Count 바인딩 Badge | 유사 |
| 검색 박스 | 키워드 검색 입력 | SearchQuery 바인딩 TextBox | 구현됨 |
| 기간 필터 | From/To DatePicker | 기간 버튼 (전체/오늘/3일/1주일/1개월) | 방식 다름 |
| 새로등록 버튼 | Primary 버튼 | HnVue.PrimaryButton 스타일 | 구현됨 |
| 현재촬영 버튼 | Secondary 버튼 | HnVue.OutlineButton (IsEnabled=False) | 버튼 존재, 기능 없음 |
| 판독완료 버튼 | Secondary 버튼 | HnVue.OutlineButton (IsEnabled=False) | 버튼 존재, 기능 없음 |
| 삭제 버튼 | Secondary 버튼 | HnVue.CancelButton (IsEnabled=False) | 버튼 존재, 기능 없음 |
| 체크박스 컬럼 | 선택 체크박스 | DataGridTemplateColumn CheckBox | 구현됨 |
| 환자ID 컬럼 | `#7bc8f5` Bold | Foreground="#7bc8f5" FontWeight=SemiBold | 일치 |
| 환자이름 컬럼 | `#e0e8f5` | DataGridTextColumn | 색상 확인 필요 |
| 성별 컬럼 | 텍스트 | DataGridTextColumn | 구현됨 |
| 생년월일 컬럼 | yyyy-MM-dd | StringFormat yyyy-MM-dd | 구현됨 |
| 등록일시 컬럼 | yyyy-MM-dd HH:mm | StringFormat yyyy-MM-dd HH:mm | 구현됨 |
| 행 선택 색상 | `#1f4878` | HnVue.Component.DataGrid.SelectionBg | 구현됨 |
| Detail Panel 너비 | 230px | ColumnDefinition Width="230" | 일치 |

### 7.2 미구현 항목 (갭)

| 항목 | PPT 명세 | 현재 상태 | 우선순위 |
|------|---------|----------|---------|
| 앱 헤더 | 30px 헤더 + 탭 네비게이션 | 없음 (MainWindow에서 처리 여부 확인) | P2 |
| Modality 컬럼 + 배지 | CT/MR/XR/US/NM 색상 배지 | 없음 | P1 (MR-WL-005) |
| 검사일시 컬럼 | StudyDate 별도 컬럼 | 없음 (등록일시만 있음) | P1 |
| Status dot 인디케이터 | 8px 컬러 점 + 텍스트 | IsEmergency Badge만 (점 없음) | P1 (MR-WL-003) |
| 행 좌측 3px 테두리 | 긴급도별 색상 테두리 | 없음 | P1 (MR-WL-003) |
| Modality/Status 드롭다운 필터 | ComboBox 필터 | 기간 버튼만 구현 | P2 (MR-WL-002) |
| From/To 날짜 필터 | DatePicker 두 개 | 없음 | P2 (MR-WL-002) |
| Detail Panel 내용 | 6개 섹션 전체 | PatientListView.xaml line 434에서 끊김 (미완성) | P1 (MR-WL-004) |
| ViewerPreview | 검은 배경 이미지 미리보기 | 없음 | P2 |
| InfoSection 환자 정보 | Key-Value 행 | 없음 | P1 |
| InfoSection Study 정보 | Key-Value 행 | 없음 | P1 |
| ToggleRow (Suspend/Hide) | 체크박스 토글 | 없음 | P3 |
| Action Buttons (패널 하단) | 뷰어열기/담당의/전달검색/출력/통보/삭제 | 없음 | P2 |
| 담당의 컬럼명 | 담당의 | 현재 "담당자"로 표시 | P3 |
| 테이블 행 좌측 테두리 자동화 | DataTrigger | 없음 | P1 |
| 진단명 컬럼 | StudyDescription | 없음 | P2 |

---

## 8. 개선 우선순위 (Phase 1-4)

### Phase 1 — 긴급 (MR Tier 1 충족)

목표: MR-WL-001, MR-WL-003 완전 준수

| 작업 ID | 작업 내용 | 관련 MR | 예상 파일 |
|--------|---------|--------|----------|
| WL-P1-01 | DataGrid에 Modality 컬럼 추가 (ModalityBadge 컴포넌트) | MR-WL-005 | PatientListView.xaml, PatientModel.cs |
| WL-P1-02 | Status 컬럼을 dot+텍스트 인디케이터로 변경 | MR-WL-003 | PatientListView.xaml |
| WL-P1-03 | DataGrid 행 좌측 3px 긴급도 테두리 구현 (DataTrigger) | MR-WL-003 | PatientListView.xaml, ComponentTokens.xaml |
| WL-P1-04 | Detail Panel 내용 완성 (InfoSection × 2, 환자/Study 정보) | MR-WL-004 | PatientListView.xaml, PatientListViewModel.cs |
| WL-P1-05 | 검사일시(StudyDate) 컬럼 추가 | MR-WL-001 | PatientListView.xaml, PatientModel.cs |
| WL-P1-06 | Modality 배지 색상 토큰 추가 (CoreTokens.xaml) | MR-WL-005 | CoreTokens.xaml |

### Phase 2 — 중요 (MR Tier 2 충족)

목표: MR-WL-002, MR-WL-004 완전 준수

| 작업 ID | 작업 내용 | 관련 MR | 예상 파일 |
|--------|---------|--------|----------|
| WL-P2-01 | FilterBar에 Modality ComboBox 추가 | MR-WL-002 | PatientListView.xaml, PatientListViewModel.cs |
| WL-P2-02 | FilterBar에 Status ComboBox 추가 | MR-WL-002 | 동일 |
| WL-P2-03 | FilterBar에 From/To DatePicker 추가 | MR-WL-002 | 동일 |
| WL-P2-04 | Detail Panel ViewerPreview 영역 구현 | MR-WL-004 | PatientListView.xaml |
| WL-P2-05 | Detail Panel ActionButtons 구현 | MR-WL-004 | PatientListView.xaml |
| WL-P2-06 | 앱 헤더 탭 네비게이션 구현 또는 확인 | — | MainWindow.xaml |

### Phase 3 — 개선 (UX 품질)

목표: PPT 디자인 완전 준수, MR Tier 3 충족

| 작업 ID | 작업 내용 | 관련 MR |
|--------|---------|--------|
| WL-P3-01 | 테이블 행 높이 36px → 24px 조정 (PPT 기준) | — |
| WL-P3-02 | 컬럼명 한국어 정규화 (담당자 → 담당의) | — |
| WL-P3-03 | Detail Panel PatientSearch 기능 구현 | MR-WL-004 |
| WL-P3-04 | ToggleRow (Suspend/Hide) 구현 | — |
| WL-P3-05 | 진단명(StudyDescription) 컬럼 추가 | — |
| WL-P3-06 | 툴바 버튼 기능 활성화 (현재촬영/판독완료/삭제) | — |

### Phase 4 — 폴리싱 (접근성 + 완성도)

| 작업 ID | 작업 내용 |
|--------|---------|
| WL-P4-01 | 스크롤바 커스텀 스타일 (5px, `#2e4070`) |
| WL-P4-02 | 행 오버 transition 애니메이션 (0.1s) |
| WL-P4-03 | 컬럼 정렬 아이콘 추가 (sortable 컬럼) |
| WL-P4-04 | 키보드 접근성 (Tab, 방향키) 검증 |
| WL-P4-05 | AutomationProperties.Name 전체 추가 |
| WL-P4-06 | 빈 목록 상태 UI (데이터 없을 때 빈 화면 처리) |

---

*이 문서는 PPT Slide 5 (HTML 코드), Slide 6 (디자인 토큰), `src/HnVue.UI/Views/PatientListView.xaml` 분석을 기반으로 작성됨.*
*토큰 권위 소스: `docs/slide6.xml` SECTION 1 (색상), SECTION 2 (타이포그래피), SECTION 3 (레이아웃)*
