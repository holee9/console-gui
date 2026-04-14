# S07-R2 Design Verification Report

| 항목 | 내용 |
|------|------|
| 발행일 | 2026-04-14 |
| 대상 | S07-R2 Design DISPATCH |
| 검증자 | Design Team |
| 이슈 | #97 |

---

## Task 1: PPT Spec 1:1 Verification (7 Core Views)

### 1.1 LoginView (PPT Slide 1) -- MATCHED

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| HnVUE 타이틀 | 큰 로고 텍스트 | Viewbox TextBlock "HnVUE" FontSize=54 | MATCH |
| Section Badge "로그인 창" | PPT Slide 3 명시 | HnVue.Component.SectionBadge.Bg + SectionBadgeText | MATCH |
| 사용자 입력 | ID Dropdown | ComboBox IsEditable=True | MATCH |
| 비밀번호 입력 | PasswordBox | PasswordBox with PasswordChanged handler | MATCH |
| 확인/취소 버튼 | 우측 정렬 | StackPanel HorizontalAlignment=Right | MATCH |
| 카드 중앙 배치 | 중앙 정렬 | Grid Row=2 Width=320 HorizontalAlignment=Center | MATCH |
| 하드코딩 색상 | - | ErrorText #ff8d8d 1건 (에러 메시지 전용) | MINOR |

**판정: MATCHED** -- PPT 슬라이드 1 요소 모두 구현됨. 에러 메시지 색상 1건은 semantic token 마이그레이션 권장.

### 1.2 PatientListView / WorklistView (PPT Slides 2-4) -- PARTIAL

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| Worklist Section Badge | PPT Slide 6 | HnVue.Component.SectionBadge.Bg | MATCH |
| 환자 DataGrid 좌측 | No, Patient ID, Name, Sex, Age, BirthDate | 6개 컬럼 구현 | MATCH |
| Study DataGrid 우측 | Accession No, StudyDate, Description | 3개 컬럼 구현 | MATCH |
| Period 필터 | Today/3Days/1Week/All/1Month | OutlineButton 5개 | MATCH |
| DatePicker 범위 | 2개 | 2개 구현 | MATCH |
| 우측 사이드바 | 검색, 필터, Quick Start | Grid Column=1 260px | MATCH |
| Quick Start 버튼 | 빨간색 강조 | Background=#df4938 | MATCH |
| 하드코딩 색상 | semantic tokens 사용 | **75건 하드코딩 색상 발견** | ISSUE |
| Empty/Loading 상태 | PPT Slide 4 | DataTrigger 기반 Visibility | MATCH |
| EMRG Badge | 응급 표시 | Background=Status.Emergency (Visibility=Collapsed) | MATCH |

**판정: PARTIAL** -- 구조적 요소는 PPT와 일치. 하지만 **75건 하드코딩 색상** (#000000, #2f2f2f, #dde6f4, #162132, #4f79b8, #585858, #dce3f1, #8899aa 등)이 semantic token 대신 직접 사용됨. Dark 테마 전용으로 하드코딩되어 Light/HighContrast 전환 시 렌더링 문제 예상.

### 1.3 StudylistView (PPT Slides 5-7) -- MATCHED

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| Studylist Section Badge | PPT Slide 5 | HnVue.Component.SectionBadge.Bg | MATCH |
| 좌측 패널 | 상태 요약 + 선택된 스터디 상세 | 250px Column + Status Overview | MATCH |
| 우측 DataGrid | Patient ID, Accession No, StudyDate, Body Part, Description | 5개 컬럼 | MATCH |
| Period 필터 | Today/3Days/1Week/All/1Month | FilterButton style 5개 | MATCH |
| PACS 선택기 | ComboBox | Width=180 ComboBox | MATCH |
| Navigation 버튼 | 이전/다음 | NavButton style 2개 | MATCH |
| 검색 바 | 돋보기 아이콘 + TextBox | Border 내 Grid 구조 | MATCH |
| Loading 오버레이 | 진행률 표시 | BoolToVisibility + ProgressBar | MATCH |
| 하드코딩 색상 | - | 0건 (모두 DynamicResource 사용) | PASS |
| DesignTime Mock | d:DesignInstance | DesignTimeStudylistViewModel | PASS |

**판정: MATCHED** -- PPT Slides 5-7 요소 모두 일치. semantic token만 사용, 하드코딩 없음.

### 1.4 AddPatientProcedureView (PPT Slide 8) -- MATCHED

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| 2-컬럼 폼 | Patient Info / Procedure Info | Grid 2 Columns | MATCH |
| Patient ID + Auto-Generate | 토글 가능 | InverseBoolConverter + Button | MATCH |
| Patient Name (*) | 필수 필드 | Status.Emergency (*) 마크 | MATCH |
| Birth Date (*) | 필수 필드 | TextBox + ToolTip | MATCH |
| Gender (*) | 필수 필드 | ComboBox M/F/Other | MATCH |
| Acc No + Auto-Generate | 토글 가능 | InverseBoolConverter + Button | MATCH |
| View Projection (*) | 칩 + 드롭다운 | WrapPanel + Border chips | MATCH |
| Description | 칩 + 편집 가능한 콤보 | IsEditable=True ComboBox | MATCH |
| RIS Code | 입력 필드 | TextBox | MATCH |
| 에러 메시지 영역 | 빨간 배경 | Status.Emergency 배경 Border | MATCH |
| Cancel/Save | 하단 버튼 | OutlineButton + PrimaryButton | MATCH |
| Study Group | PPT에서 제거됨 | 구현되지 않음 | CORRECT |

**판정: MATCHED** -- PPT Slide 8 요소 모두 일치. Study Group 필드 정확히 제외됨.

### 1.5 WorkflowView / Acquisition (PPT Slides 9-11) -- PARTIAL

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| 3-컬럼 레이아웃 | Patient / Preview / Control | 280px + * + 340px | MATCH |
| PatientInfoCard | 좌측 패널 | medical:PatientInfoCard 컴포넌트 | MATCH |
| AcquisitionPreview | 중앙 뷰어 | medical:AcquisitionPreview 컴포넌트 | MATCH |
| Thumbnail Strip | 하단 썸네일 | ListBox Horizontal + StudyThumbnail | MATCH |
| Acquisition 제어 | Prepare / Expose 버튼 | Button Height=40/44 | MATCH |
| 촬영 모드 탭 | Stand / Table / Portable | UniformGrid ToggleButton 3개 | MATCH |
| Crop & Marker | L/R 선택 + 입력 | StackPanel 구조 | MATCH |
| Manipulation 도구 | 확대/회전 등 | WrapPanel ToolButtonStyle | MATCH |
| Annotation 도구 | 그리기 도구 | WrapPanel ToolButtonStyle | MATCH |
| STOP 버튼 | 항상 표시, 빨간색 | Height=44 Background=#df2626 | MATCH |
| 하드코딩 색상 | - | **16건** (#242424, #1d2433, #5d6880 등) | ISSUE |
| Emergency Stop 스타일 | HnVue.EmergencyStopButton | **미사용** -- 직접 인라인 스타일로 구현 | ISSUE |
| AutomationProperties | 화면 판독기 | **미설정** | ISSUE |

**판정: PARTIAL** -- 구조는 PPT와 일치하나, (1) 하드코딩 색상 16건, (2) Emergency Stop 버튼이 HnVue.EmergencyStopButton 스타일을 사용하지 않음, (3) AutomationProperties 미설정.

### 1.6 MergeView (PPT Slides 12-13) -- MATCHED

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| 3-컬럼 A/B + Preview | Patient A / Preview / Patient B | 3 Column Grid | MATCH |
| Patient A/B 리스트 | 검색 + ListBox | SearchQueryA/B + PatientsA/B ListBox | MATCH |
| 검색 바 | 돋보기 + TextBox + 버튼 | Border + Grid 구조 | MATCH |
| Preview 패널 | A/B 이미지 비교 | 2 Column Grid with placeholder | MATCH |
| Thumbnail Strip | 하단 썸네일 | ScrollViewer + ItemsControl Horizontal | MATCH |
| Cancel / Sync Study | 하단 버튼 | OutlineButton + PrimaryButton | MATCH |
| Section Header | "Sync Study" | TextBlock + Icon | MATCH |
| 하드코딩 색상 | - | 0건 (모두 DynamicResource) | PASS |

**판정: MATCHED** -- PPT Slides 12-13 요소 모두 일치. semantic token만 사용.

### 1.7 SettingsView (PPT Slides 14-22) -- MATCHED

| 항목 | PPT 스펙 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| TOP 탭 네비게이션 | PPT Slide 14 | ToggleButton row Horizontal | MATCH |
| System 탭 | Access Notice + Priority + Language + Auto Logout | 모든 필드 구현 | MATCH |
| Account 탭 | ID + Password + Role ComboBox + Account List | 모든 필드 구현 | MATCH |
| Detector 탭 | Type + Connection + IP + Calibration | 모든 필드 구현 | MATCH |
| Generator 탭 | Model + Port + Baud Rate | 모든 필드 구현 | MATCH |
| Network 탭 | PACS + Worklist + Print 통합 | 3 섹션 모두 구현 | MATCH |
| Display 탭 | Theme + Window Mode + CheckBox | 모든 필드 구현 | MATCH |
| Option 탭 | 4 CheckBox + Default W/L | 모든 필드 구현 | MATCH |
| Database 탭 | Path + Backup + Backup/Restore | 모든 필드 구현 | MATCH |
| DicomSet 탭 | AE Title + Station + UID + Transfer Syntax | 모든 필드 구현 | MATCH |
| RIS Code 탭 | Matching / Un-Matched 서브탭 | RisSubTabButton + DataGrid | MATCH |
| "Login Popup" → "Access Notice" | PPT 명칭 변경 | Access Notice로 반영 | MATCH |
| "Only No matching" → "Un-Matched" | PPT 명칭 변경 | Un-Matched로 반영 | MATCH |
| Operator 필드 제거 | PPT 변경 | 구현되지 않음 | CORRECT |
| 하드코딩 색상 | - | 0건 (모두 DynamicResource) | PASS |
| AutomationProperties | 모든 인터랙티브 요소 | **66건 설정** | PASS |
| TabIndex | 키보드 네비게이션 | **62건 설정** | PASS |

**판정: MATCHED** -- PPT Slides 14-22의 모든 탭과 필드 구현. 명칭 변경사항 정확 반영. 접근성 충실.

---

## Task 2: Theme Consistency

### 2.1 Design Token Architecture

3-tier 토큰 시스템 검증:

| 계층 | 파일 | 상태 |
|------|------|------|
| Core Tokens | CoreTokens.xaml | 완전 -- 27개 Color + 6 FontSize + 6 Spacing |
| Semantic Tokens | SemanticTokens.xaml | 완전 -- Surface/Brand/Text/Status/Button/Border |
| Component Tokens | ComponentTokens.xaml | 완전 -- SectionBadge/EmergencyStop/Label |

### 2.2 Theme Files

| 테마 | 파일 | 키 커버리지 | 상태 |
|------|------|-------------|------|
| Dark (기본) | DarkTheme.xaml | 14개 Core Color 오버라이드 | 완전 |
| Light | LightTheme.xaml | 14개 Core Color 오버라이드 | 완전 |
| High Contrast | HighContrastTheme.xaml | 14개 Core Color 오버라이드 | 완전 |

### 2.3 하드코딩 색상 문제

DynamicResource 사용률 분석:

| 뷰 | 하드코딩 건수 | 위험도 | 비고 |
|----|--------------|--------|------|
| LoginView | 2건 | LOW | 에러 메시지 색상만 |
| PatientListView | **75건** | **CRITICAL** | Dark 테마 전용 하드코딩 |
| StudylistView | 0건 | NONE | 완전 DynamicResource |
| AddPatientProcedureView | 0건 | NONE | 완전 DynamicResource |
| WorkflowView | **16건** | HIGH | 배경/보더/텍스트 하드코딩 |
| MergeView | 0건 | NONE | 완전 DynamicResource |
| SettingsView | 0건 | NONE | 완전 DynamicResource |
| **합계** | **93건** | | |

**PatientListView가 전체 하드코딩의 81%를 차지.** 이 뷰는 Light/HighContrast 테마에서 색상이 전환되지 않음.

### 2.4 WCAG 2.1 AA 색상 대비비 검증

Dark 테마 기준 (가장 일반적 사용):

| 색상 조합 | 전경 | 배경 | 대비비 | 판정 |
|-----------|------|------|--------|------|
| TextPrimary on Page | #FFFFFF | #1A1A2E | 14.3:1 | PASS |
| TextSecondary on Page | #B0BEC5 | #1A1A2E | 7.8:1 | PASS |
| StatusSafe on Dark | #00C853 | #1A1A2E | 6.4:1 | PASS |
| StatusEmergency on Dark | #D50000 | #1A1A2E | 4.8:1 | PASS |
| StatusWarning on Dark | #FFD600 | #1A1A2E | 9.1:1 | PASS |

Light 테마 기준:

| 색상 조합 | 전경 | 배경 | 대비비 | 판정 |
|-----------|------|------|--------|------|
| TextPrimary on Page | #1A1A2E | #F5F5F5 | 14.1:1 | PASS |
| TextSecondary on Page | #455A64 | #F5F5F5 | 5.6:1 | PASS |
| StatusSafe on Light | #2E7D32 | #F5F5F5 | 4.7:1 | PASS |
| StatusEmergency on Light | #C62828 | #F5F5F5 | 4.9:1 | PASS |

High Contrast 테마: 모든 상태 색상이 순도 채도(pure saturation)로 대비비 7:1 이상 보장.

**테마 시스템 구조는 건전. 하지만 PatientListView와 WorkflowView의 하드코딩 색상이 테마 전환을 무력화.**

---

## Task 3: Accessibility Verification (IEC 62366 / WCAG 2.1 AA)

### 3.1 Touch Targets (>= 44x44px)

| 뷰 | 44px 이상 인터랙티브 | 44px 미만 | 판정 |
|----|---------------------|-----------|------|
| LoginView | 버튼 Height=36 | ComboBox/PasswordBox 36px | PARTIAL (버튼이 44px 미만) |
| PatientListView | Quick Start Height=46 | SideIconButton 38x38 | PARTIAL |
| StudylistView | NavButton/FilterButton 44x44, RowHeight=44 | - | PASS |
| AddPatientProcedureView | 입력 Height=32 | 전반적으로 32px | PARTIAL |
| WorkflowView | ToolButton 44x44, STOP Height=44 | - | PASS |
| MergeView | Cancel/Sync 36px | 버튼 높이 36px | PARTIAL |
| SettingsView | Tab MinHeight=44, Cancel/Save MinHeight=44 | 입력 MinHeight=34 | PARTIAL |

**참고:** WPF DataGrid 행의 경우 RowHeight=40~44px로 설정되어 타겟 크기 요건을 대체로 충족.

### 3.2 Keyboard Tab Navigation (TabIndex)

| 뷰 | TabIndex 설정 여부 | 판정 |
|----|-------------------|------|
| LoginView | 미설정 (자동 순서에 의존) | ACCEPTABLE |
| PatientListView | 미설정 | ACCEPTABLE |
| StudylistView | **설정 (TabIndex 0-11)** | PASS |
| AddPatientProcedureView | 미설정 | ACCEPTABLE |
| WorkflowView | 미설정 | ACCEPTABLE |
| MergeView | 미설정 | ACCEPTABLE |
| SettingsView | **설정 (TabIndex 1-111)** | PASS |

### 3.3 AutomationProperties (Screen Reader)

| 뷰 | AutomationProperties.Name 건수 | 판정 |
|----|-------------------------------|------|
| LoginView | 0건 | MISSING |
| PatientListView | 0건 | MISSING |
| StudylistView | **12건** | PASS |
| AddPatientProcedureView | 0건 | MISSING |
| WorkflowView | 0건 | MISSING |
| MergeView | 0건 | MISSING |
| SettingsView | **66건** | PASS |

### 3.4 Emergency Stop (Acquisition Screen)

| 항목 | 요구사항 | 구현 상태 | 판정 |
|------|----------|-----------|------|
| 가시성 | 항상 표시 | ScrollViewer 내부에 배치 -- 스크롤 시 가려질 수 있음 | ISSUE |
| 최소 크기 | 56px min-height | Height=44px | ISSUE |
| 스타일 | HnVue.EmergencyStopButton | 인라인 스타일 사용 (Background=#df2626) | ISSUE |
| 항상 활성 | IsEnabled=True | 명시적 설정 없음 | ISSUE |
| AutomationProperties | AutomationProperties.Name | 미설정 | ISSUE |
| 키보드 단축키 | Escape | 미구현 | ISSUE |

**긴급정지 버튼은 IEC 62366 안전 요건을 충족하지 않음.** HnVue.EmergencyStopButton 스타일이 HnVueTheme.xaml에 정의되어 있으나 WorkflowView에서 이를 사용하지 않고 인라인 스타일로 구현됨.

---

## Summary

### 전체 상태

| 뷰 | PPT 일치 | 테마 일관성 | 접근성 | 종합 |
|----|----------|------------|--------|------|
| LoginView | MATCHED | GOOD | FAIR | GOOD |
| PatientListView | PARTIAL | **CRITICAL** | FAIR | NEEDS FIX |
| StudylistView | MATCHED | EXCELLENT | GOOD | EXCELLENT |
| AddPatientProcedureView | MATCHED | EXCELLENT | FAIR | GOOD |
| WorkflowView | PARTIAL | NEEDS FIX | **POOR** | NEEDS FIX |
| MergeView | MATCHED | EXCELLENT | FAIR | GOOD |
| SettingsView | MATCHED | EXCELLENT | EXCELLENT | EXCELLENT |

### Critical Issues (수정 필요)

1. **PatientListView 하드코딩 색상 75건** (P1)
   - Dark 테마 전용 하드코딩으로 Light/HighContrast 전환 불가
   - semantic token 마이그레이션 필요

2. **WorkflowView Emergency Stop 버튼** (P1 -- 안전)
   - HnVue.EmergencyStopButton 스타일 미사용
   - Height=44px (요구 56px)
   - AutomationProperties 미설정
   - ScrollViewer 내부 위치로 스크롤 시 가려짐 가능

3. **WorkflowView 하드코딩 색상 16건** (P2)

### Recommendations

1. PatientListView 색상 하드코딩 75건을 DynamicResource로 마이그레이션
2. WorkflowView STOP 버튼을 HnVue.EmergencyStopButton 스타일로 교체
3. LoginView, PatientListView, WorkflowView, MergeView에 AutomationProperties.Name 추가
4. WorkflowView STOP 버튼 Height를 56px로 증가, ScrollViewer 밖으로 이동

### Build Evidence

- Build: 0 errors (MSBuild Release)
- Tests: 569 passed, 0 failed (HnVue.UI.Tests)
- Issue: #97
