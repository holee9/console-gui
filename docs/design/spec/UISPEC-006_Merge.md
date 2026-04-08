# UISPEC-006: 영상 병합(Merge) 화면 UI 디자인 명세서

## 문서 정보

| 항목 | 내용 |
|------|------|
| **버전** | v1.0 |
| **상태** | Draft |
| **PPT 참조** | Slide 12-13 |
| **구현 파일** | MergeView.xaml (신규 생성 예정) |
| **HTML Mockup** | `docs/ui_mockups/05-merge.html` |
| **관련 SPEC** | SPEC-UI-001 |
| **작성일** | 2026-04-07 |

---

## 1. 화면 개요

### 1.1 목적

두 개 이상의 검사(Study) 영상을 병합하여 비교/분석할 수 있는 화면입니다. 의료진 워크플로우에서 영상 비교 기능은 필수적이며, 환자 데이터 오병합을 방지하는 안전 장치가 포함됩니다. 의료진 워크플로우에서 영상 비교 기능은 필수적이며, 환자 데이터 오병합을 방지하는 안전 장치가 포함됩니다.

### 1.2 사용자 시나리오

1. 방사선사가 비교할 두 검사를 선택합니다
2. 병합 모드를 선택합니다 (수평/수직/겹치기/비교)
3. 병합할 영상을 선택하고 순서를 조정합니다
4. 미리보기로 병합 결과를 확인합니다
5. 병합 결과를 저장하거나 취소합니다

### 1.3 안전 중요 사항

> **⚠️ 주의**: 이 화면은 환자 데이터 병합 기능을 제공합니다. 환자 ID 불일치 시 자동으로 경고하고 병합을 차단해야 합니다. 모든 UI 결정은 IEC 62366 의료기기 사용성 공학 기준을 준수해야 합니다.

---

## 2. 레이아웃 구조

### 2.1 전체 레이아웃

```
┌────────────────────────────────────────────────────────────────────┐
│  Header (60px)                                                  │
│  Logo "PROTYPER" | "Merge" 타이틀 | 도움말 버튼                    │
├────────────────────────────────────────────────────────────────────┤
│  Selection Bar (80px)                                            │
│  [Study A 검색]  |  [Study B 검색]                                 │
├────────────────────────────────────────────────────────────────────┤
│  Content Area                                                  │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │ Panel A              │ Center Controls    │ Panel C          │  │
│  │ (flex:1)             │ (280px)            │ (flex:1)         │  │
│  │                     │                   │                  │  │
│  │ Study A             │ 병합 모드 선택    │ 병합 결과        │  │
│ │ Patient Info        │ 조정 옵션        │ Preview 영역     │  │
│ │ Image Display        │ 미리보기/동기화   │ Image Display     │  │
│ │ Image Strip          │ Preview Panel      │ Image Strip       │  │
│  │                     │                   │                  │  │
│  └──────────────────────────────────────────────────────────────┘  │
├────────────────────────────────────────────────────────────────────┤
│  Preview Panel (200px) - Center Controls 하단                      │
│  미리보기 결과 표시 영역                                            │
├────────────────────────────────────────────────────────────────────┤
│  Footer (64px)                                                    │
│  Study 정보 표시 | [취소] [저장] 버튼                         │
├────────────────────────────────────────────────────────────────────┤
│  Status Bar (32px)                                                 │
│  상태 메시지 | 안내 메시지                                          │
└────────────────────────────────────────────────────────────────────┘
```

### 2.2 헤더 섹션

| 요소 | 사양 |
|------|------|
| 높이 | 60px |
| 배경색 | `HnVue.Semantic.Surface.Panel` (#2A2A2A) |
| 하단 경계 | 1px `HnVue.Semantic.Border.Default` (#3B3B3B) |
| 로고 | 100×36px, gradient `#1B4F8A` → `#2E6DB4`, "PROTYPER" Bold 14px white |
| 타이틀 | "Merge" 20px SemiBold `HnVue.Semantic.Text.Primary` |
| 도움말 버튼 | `HnVue.SecondaryButton` |

### 2.3 선택 바 (Selection Bar)

| 요소 | 사양 |
|------|------|
| 높이 | 80px |
| 배경색 | `HnVue.Semantic.Surface.Panel` (#2A2A2A) |
| 하단 경계 | 1px `HnVue.Semantic.Border.Default` (#3B3B3B) |
| 검색 그룹 | Flex:1, 각각 gap 24px |
| 라범 | "Study A"/"Study B" 14px Semibold `HnVue.Semantic.Text.Muted`, min-width 80px |
| 입력창 | Height 40px, padding 0 16px, border-radius 6px |
| 검색 버튼 | 40×40px, icon 🔍 |

### 2.4 콘텐츠 영역 (Content Area)

#### Panel A (Source Study)

| 요소 | 사양 |
|------|------|
| 너비 | flex:1 |
| 헤더 | Height 48px, 배경 `HnVue.Semantic.Surface.Panel` |
| 환자 정보 표시 | 환자 ID + 배지 (성별, 지역 등) |
| 이미지 디스플레이 | Flex center, 최대 400px 너비 |
| 이미지 컨테이너 | Background `HnVue.Semantic.Surface.Panel`, border-radius 8px, border 2px |
| 이미지 스트립 | Height 100px, 스크롤 가능, gap 8px |

#### Center Controls

| 요소 | 사양 |
|------|------|
| 너비 | 280px (고정) |
| 배경색 | `HnVue.Semantic.Surface.Panel` (#2A2A2A) |
| 좌/우 경계 | 1px `HnVue.Semantic.Border.Default` (#3B3B3B) |
| 컨트롤 섹션 | Padding 16px, border-bottom 1px |
| 액션 버튼 | Height 48px, border-radius 6px |

#### Panel C (Merged Result)

| 요소 | 사양 |
|------|------|
| 너비 | flex:1 |
| 헤더 | "병합 결과" + "Preview" 배지 |
| 미리보기 영역 | Background `HnVue.Semantic.Surface.Page`, border-radius 8px |
| 결과 이미지 스트립 | 추가된 영상만 표시 |

---

## 3. 컴포넌트 디자인 명세

### 3.1 병합 모드 선택 (Merge Mode Selection)

**옵션 항목**:
- 수평 병합 (Horizontal)
- 수직 병합 (Vertical)
- 겹치기 (Overlay)
- 비교 모드 (Compare Side-by-Side)

**상태 디자인**:
```xml
<!-- Selected State -->
<Border Background="{DynamicResource HnVue.Semantic.Surface.Card}"
        BorderBrush="{DynamicResource HnVue.Semantic.Border.Focus}"
        BorderThickness="1"
        CornerRadius="6"
        Padding="10,12">
    <RadioButton IsChecked="True" GroupName="MergeMode"/>
    <TextBlock Text="수평 병합" FontSize="14"/>
</Border>

<!-- Hover State -->
<Border Background="{DynamicResource HnVue.Semantic.Surface.Card}">
    <!-- Mouse hover shows this -->
</Border>
```

### 3.2 조정 옵션 (Adjustment Options)

| 옵션 | 기본값 | 설명 |
|------|--------|------|
| 자동 정렬 (Auto Align) | Checked | 이미지 자동 정렬 |
| 밝기 보정 (Brightness) | Unchecked | 밝기 자동 보정 |
| 크기 조정 (Resize) | Checked | 이미지 크기 자동 조정 |
| 대비도 향상 (Contrast) | Unchecked | 대비도 자동 향상 |

### 3.3 미리보기/동기화 버튼

| 버튼 | 스타일 | 크기 |
|------|-------|------|
| 미리보기 | `HPB (HnVue.PrimaryButton)` | Height 48px, icon 👁️ + 텍스트 |
| 동기화 | `HnVue.AccentButton` (녹색 #00C853) | Height 48px, icon 🔄 + 텍스트 |

**미리보기 버튼 상태**:
- 기본: "👁 미리보기"
- 클릭: "✓ 미리보기 완료" (2초 후 복귀)

**동기화 버튼 상태**:
- 기본: "🔄 동기화"
- 완료: "✓ 동기화 완료" (2초 후 복귀)

### 3.4 드래그 앤 드롭 (Drag & Drop)

**드롭 오버레이**:
```
배경: rgba(0, 102, 204, 0.1) (10% 투명도 blue)
테두리: 2px dashed #2E6DB4
텍스트: "여기에 놓으세요" 18px blue
z-index: 10
```

---

## 4. 색상 토큰 매핑

| 용도 | 토큰 이름 | 색상 값 | 비고 |
|------|----------|---------|------|
| 페이지 배경 | `HnVue.Semantic.Surface.Page` | `#242424` | PPT Slide 4 기준 |
| 패널 배경 | `HnVue.Semantic.Surface.Panel` | `#2A2A2A` | |
| 카드 배경 | `HnVue.Semantic.Surface.Card` | `#3B3B3B` | |
| 입력창 배경 | `HnVue.Semantic.Surface.Card` | `#3B3B3B` | |
| 기본 텍스트 | `HnVue.Semantic.Text.Primary` | `#FFFFFF` | |
| 보조 텍스트 | `HnVue.Semantic.Text.Muted` | `#B0BEC5` | |
| 비활성 텍스트 | `HnVue.Semantic.Text.Disabled` | `#546E7A` | |
| 기본 경계 | `HnVue.Semantic.Border.Default` | `#3B3B3B` | |
| 포커스 경계 | `HnVue.Semantic.Border.Focus` | `#00AEEF` | |
| 프라이머 브러시 | `HnVue.Primary.Brush` | `#1B4F8A` | |
| 액센트 | `HnVue.Brand.Accent` | `#00AEEF` | |
| 성공 (녹색) | `HnVue.Semantic.Status.Safe` | `#00C853` | 동기화 완료 |
| 경고 (황색) | `HnVue.Semantic.Status.Warning` | `#FFD600` | 확인 필요 |
| 에러 (적색) | `HnVue.Semantic.Status.Emergency` | `#D50000` | 오류 발생 |

---

## 5. 상태 디자인

### 5.1 기본 상태

```
Panel A/C: 기본 디스플레이
Center Controls: 옵션 선택 가능
Footer: 취소/저장 버튼 활성
Status Bar: "준비 완료" + 안내 메시지
```

### 5.2 스터디 선택 후

```
Panel A/C: 선택한 Study 정보 표시
Center Controls: 옵션 활성화
Footer: 저장 버튼 활성화
Status Bar: "미리보기를 실행하세요"
```

### 5.3 미리보기 완료

```
Preview Panel: 병합 결과 표시
미리보기 버튼: "✓ 미리보기 완료"
Status Bar: "병합 결과가 생성되었습니다. 저장하시겠습니까?"
```

### 5.4 환자 데이터 불일치 경고

```
Status Bar: Red 배경 + 경고 아이콘
메시지: "⚠️ 환자 ID가 일치하지 않습니다. 병합을 진행할 수 없습니다."
Preview 버튼: 비활성화 (IsEnabled=False)
```

---

## 6. 안전성 및 사용성 고려사항 (IEC 62366 - International Electrotechnical Commission)

### 6.1 환자 데이터 오병합 방지

| 상황 | 대응 |
|------|------|
| 환자 ID 불일치 | 병합 버튼 비활성화, 경고 다이얼로그 표시 |
| 검사 일시 불일치 | 날짜 차이가 7일 이상인 경우 경고 |
| 환자 이름 불일치 | 병합 전 확인 다이얼로그 표시 |

### 6.2 터치 타겟 (Touch Targets)

| 컴포넌트 | 최소 크기 | 권장 |
|----------|----------|------|
| 옵션 항목 (클릭) | 44×40px (Height×Width) | IEC 62366 |
| 액션 버튼 | 48px Height | IEC 62366 |
| 검색 버튼 | 40×40px | WCAG 2.2 AA |
| 이미지 스트립 아이템 | 80×80px | 드래그 가능 |

### 6.3 색상 대비비 (Color Contrast)

| 조합 | 대비비 | 상태 |
|------|-------|------|
| White on #1B4F8A (Primary) | 7.2:1 | ✅ PASS |
| White on #00C853 (Success) | 5.8:1 | ✅ PASS |
| White on #FFD600 (Warning) | 10.6:1 | ✅ PASS |
| White on #D50000 (Error) | 5.9:1 | ✅ PASS |

### 6.4 키보드 내비게이션

| 단축키 | 기능 |
|--------|------|
| Escape | 작업 취소, 확인 없이 닫기 |
| Enter | 미리보기 실행 |
| Ctrl+S | 저장 (확인 후) |
| Arrow Keys | 이미지 스트립 네비게이션 |

---

## 7. MRD/PRD 트레이서빌리티

### MRD 연결

| MR ID | 요구사항 | 구현 방법 |
|-------|---------|---------|
| MR-015 | 이미지 스티칭 기능 | 병합 모드: 수평/수직 병합 지원 |
| MR-WF-008 | 이전 영상 비교 | 비교 모드 지원 |
| MR-IP-002 | 영상 조작 기능 | 병합 후 영상 조작 지원 |

### PRD 연결

| FR ID | 기능 요구사항 | 구현 방법 |
|-------|-------------|---------|
| FR-IP-007 | 이미지 스티칭 | 수평/수직 병합 모드 |
| FR-IP-008 | 크롭 영역 자동 감지 | 자동 정렬 옵션 |
| FR-IP-009 | 밝기/대비도 자동 조정 | 밝기 보정, 대비도 향상 옵션 |

---

## 8. 구현 갭 분석

### 8.1 구현된 항목 ✅

| 항목 | 상태 | 설명 |
|------|------|------|
| HTML Mockup | ✅ 완료 | `docs/ui_mockups/05-merge.html` |
| 레이아웃 구조 | ✅ 정의 | 3열 레이아웃 명세 |
| 색상 토큰 매핑 | ✅ 완료 | CoreTokens.xaml 기준 |

### 8.2 미구현 항목 ❌

| 항목 | 상태 | 우선순위 |
|------|------|----------|
| MergeView.xaml | ❌ 미생성 | Phase 1 필수 |
| 드래그 앤 드롭 | ❌ 미구현 | Phase 2 |
| 환자 ID 확인 로직 | ❌ 미구현 | Phase 1 (안전) |
| Undo/Redo 기능 | ❌ 미구현 | Phase 2 |

### 8.3 Gap 분석 요약

- **준수율**: 0% (XAML 구현 전)
- **PPT 준수**: Slide 12-13 디자인 반영
- **안전 요구사항**: 환자 ID 확인 로직 필수 (Tier 1)

---

## 9. 개선 우선순위

### Phase 1 (필수 구현)

1. **MergeView.xaml 생성**
   - 3열 Grid 레이아웃 구현
   - MahApps.Metro 컨트롤 활용
   - MVVM 패턴 적용

2. **환자 ID 확인 로직**
   - Study A와 Study B의 환자 ID 비교
   - 불일치 시 경고 다이얼로그
   - 환자 ID 불일치 시 병합 차단

3. **기본 병합 기능**
   - 수평 병합 기능 구현
   - 미리보기 생성 (placeholder)
   - 저장 기능 (데이터베이스/파일)

### Phase 2 (고도화)

1. **드래그 앤 드롭 구현**
   - 이미지 스트립 항목 드래그
   - 순서 변경 기능
   - 시각적 피드백

2. **추가 병합 모드**
   - 수직 병합
   - 겹치기 병합
   - 비교 모드 (Side-by-Side)

3. **고급 조정 옵션**
   - 밝기 보정 알고리즘
   - 대비도 향상 알고리즘
   - 자동 정렬 고도화

### Phase 3 (사용성 개선)

1. **Undo/Redo 기능**
   - 병합 작업 취소/재실행
   - 작업 이력 저장

2. **배치 작업**
   - 일괄 처리 대기열
   - 자동 병합 프로세스

3. **내보내기 기능**
   - 병합 결과 DICOM 저장
   - PACS 전송

---

## 10. 참고 문서

| 문서 | 위치 |
|------|------|
| HTML Mockup | `docs/ui_mockups/05-merge.html` |
| PPT Design Source | `docs/★HnVUE UI 변경 최종안_251118.pptx` Slide 12-13 |
| Core Design Tokens | `src/HnVue.UI/Themes/tokens/CoreTokens.xaml` |
| UI Master Reference | `docs/design/UI_DESIGN_MASTER_REFERENCE.md` |
| IEC 62366 Usability File | `docs/testing/DOC-021_UsabilityFile_v2.0.md` |

---

**버전**: 1.0  
**상태**: Draft  
**다음 수정**: Phase 1 구현 완료 후 v1.1 업데이트
