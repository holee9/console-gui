# Team Design — Pure UI Design Rules (No Coder)

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)

## Design Team Identity [HARD]

> **Design Team has NO coders. Design Team does DESIGN CODIFICATION only.**

- [HARD] Design Team = PPT/Figma 디자인을 XAML로 코드화하는 팀
- [HARD] Design Team은 기능구현(business logic, service, ViewModel, domain model)을 하지 않음
- [HARD] C# 코드 작성 범위: XAML code-behind (InitializeComponent + 순수 UI 이벤트) ONLY
- [HARD] PPT scope compliance: only implement specified PPT pages
- [HARD] Evidence includes PPT 1:1 comparison result

## Module Ownership (Design Codification Only)

Design Team이 소유하고 직접 수정하는 파일:

**XAML 파일 (핵심 산출물):**
- HnVue.UI/Views/*.xaml (View 레이아웃, 데이터 바인딩)
- HnVue.UI/Styles/*.xaml (스타일 리소스)
- HnVue.UI/Themes/**/*.xaml (테마, 디자인 토큰)
- HnVue.UI/Components/**/*.xaml (커스텀 컨트롤 템플릿)
- src/Styles/, src/Themes/ (프로젝트 외부 공유 리소스)

**XAML Code-Behind (최소):**
- HnVue.UI/Views/*.xaml.cs — InitializeComponent() + ViewModel DI 생성자 ONLY
- WPF 보안 제약 코드 (예: PasswordBox.PasswordChanged) — 허용하지만 패턴 복사만

**DesignTime:**
- HnVue.UI/DesignTime/ (Mock ViewModel for VS2022 디자이너)

**문서:**
- docs/style_guide/, docs/ui_mockups/

## Design Team이 절대 만들지 않는 것 [HARD]

| 금지 영역 | 예시 | 담당 팀 |
|-----------|------|---------|
| Service 클래스 | ThemeRollbackService.cs | Coordinator |
| ViewModel 클래스 | LoginViewModel.cs | Coordinator |
| MVVM 인프라 | ViewModelBase.cs, RelayCommand.cs | Team A (Common) |
| Domain Enum/Model | DoseLevel enum, SafeState enum | Team A (Common) 또는 Team B |
| Interface 정의 | ILoginViewModel.cs | Coordinator (UI.Contracts) |
| Converter with domain logic | SafeStateToColorConverter.cs | 아래 분담표 참조 |
| DI Registration | App.xaml.cs ServiceCollection | Coordinator |
| 비즈니스 로직 code-behind | DB 호출, API 호출, 계산 로직 | Team A/B |

## Converter 분담 기준 [IMPORTANT]

Converter는 "시각 변환"과 "도메인 변환"의 경계에 있어 명확한 분담 필요:

| Converter 유형 | 예시 | 소유 팀 | 이유 |
|---------------|------|---------|------|
| 순수 시각 변환 | BoolToVisibilityConverter, InverseBoolConverter, CountToVisibilityConverter | **Design** | 도메인 무관, XAML 바인딩 헬퍼 |
| 탭/상태 표시 | ActiveTabToVisibilityConverter, StringEqualityToBoolConverter | **Design** | UI 상태 전환, 도메인 무관 |
| 도메인 enum → 색상 | SafeStateToColorConverter, StatusToBrushConverter | **Team B** (작성) + **Design** (색상값 제안) | SafeState는 도메인 모델, 변경 시 Team B가 수정해야 함 |
| 도메인 데이터 → 표시형식 | AgeFromBirthDateConverter, DateOnlyToStringConverter | **Team B** | 환자 데이터 모델 의존 |
| 멀티바인딩 로직 | MultiBoolAndConverter, MultiBoolOrConverter | **Design** | 순수 논리 연산, 도메인 무관 |

## Custom Control(Components/) 분담 기준 [IMPORTANT]

| Component 유형 | 예시 | 소유 팀 | 이유 |
|---------------|------|---------|------|
| 순수 레이아웃 컨트롤 | Sidebar, Card | **Design** | DependencyProperty만, 도메인 무관 |
| UI 인프라 컨트롤 | MedicalButton, MedicalTextBox | **Design** | 접근성/디자인 표준 구현 |
| 도메인 표시 컨트롤 | AcquisitionPreview, PatientInfoCard, StudyThumbnail | **Team B** (C# 로직) + **Design** (XAML 템플릿) | 의료 도메인 속성(DoseLevel, ExposureInfo) 포함 |
| Modal/Toast (동작 포함) | Modal.xaml.cs, Toast.xaml.cs | **Coordinator** | 앱 전역 서비스 연동 필요 |

## 기존 코드 재배치 권고 (Refactoring Backlog)

현재 HnVue.UI 프로젝트 내에 Design Team 범위를 넘는 코드가 존재:

| 파일 | 현재 위치 | 권고 이동 위치 | 담당 팀 |
|------|-----------|---------------|---------|
| ViewModelBase.cs, RelayCommand.cs | HnVue.UI/Components/Common/ | HnVue.Common (또는 삭제 후 CommunityToolkit.Mvvm 사용) | Team A |
| ThemeRollbackService.cs | HnVue.UI/Services/ | HnVue.UI.ViewModels/ 또는 별도 서비스 | Coordinator |
| DoseLevel enum | AcquisitionPreview.cs 내부 | HnVue.Common.Enums/ | Team A |
| SafeStateToColorConverter.cs | HnVue.UI/Converters/ | 위치 유지, 소유권 Team B로 변경 | Team B |
| AgeFromBirthDateConverter.cs | HnVue.UI/Converters/ | 위치 유지, 소유권 Team B로 변경 | Team B |

## 기능 요청 시 Design Team 행동 프로토콜

Design Team이 DISPATCH에서 기능구현이 필요한 작업을 받았을 때:

1. XAML 레이아웃과 바인딩 구조만 구현
2. 필요한 ViewModel 속성/커맨드를 목록으로 정리
3. DISPATCH Status에 "NEEDS_VIEWMODEL: {속성 목록}" 기재
4. Coordinator가 ViewModel 구현 후 연동

## Dependency Restrictions (Architecture Tests Enforce)

- ALLOWED references: HnVue.Common, HnVue.UI.Contracts, MahApps.Metro, CommunityToolkit.Mvvm, LiveChartsCore
- FORBIDDEN references: HnVue.Data, HnVue.Security, HnVue.Workflow, HnVue.Imaging, HnVue.Dicom, HnVue.Dose, HnVue.PatientManagement, HnVue.Incident, HnVue.Update, HnVue.SystemAdmin, HnVue.CDBurning
- No direct business logic in Views — use data binding only

## DESIGN_TO_XAML_WORKFLOW.md 5-Phase Compliance

- Phase 1: Design Creation (Pencil/Figma)
- Phase 2: Design Review Gate (QA/RA for Emergency Stop, IEC 62366)
- Phase 3: Design Token Extraction (CoreTokens.xaml)
- Phase 4: WPF XAML Implementation
- Phase 5: Implementation Verification

## MahApps.Metro Theme Standards

- All custom styles extend MahApps base styles
- Support 3 themes: Light, Dark, High Contrast
- Theme switching must be runtime-capable
- Use MahApps resource keys for consistency

## DesignTime Mock Pattern

- All Views must render in VS2022 designer without running the app
- Use d:DataContext with DesignInstance for mock data
- Mock ViewModels in src/HnVue.UI/DesignTime/ folder
- Mock data must be representative (Korean names, realistic IDs)

## Accessibility Requirements (IEC 62366 / WCAG 2.1 AA)

- Color contrast ratio >= 4.5:1
- Touch targets >= 44x44px
- Keyboard Tab navigation for all interactive elements
- Screen reader compatible (AutomationProperties)
- Emergency Stop button always visible on Acquisition screen

## Code-Behind Rules [HARD]

- [HARD] code-behind에서 허용: InitializeComponent(), ViewModel DI 생성자, WPF 보안 제약 이벤트 핸들러
- [HARD] code-behind에서 금지: DB 호출, API 호출, 비즈니스 계산, 상태 변경 로직
- [HARD] 애니메이션과 VisualState 변경은 XAML Storyboard/VisualStateManager 우선, 불가시에만 code-behind
- [HARD] 새 코드비하인드 패턴 필요 시: 기존 패턴(LoginView.xaml.cs PasswordBox 참조) 복사만 허용

## PPT Spec Scope Compliance [HARD — Issue #59]

When implementing from PPT/design spec pages:

- [HARD] Before starting, explicitly list the EXACT XAML files to be modified and confirm they match the specified pages ONLY
- [HARD] Never implement UI elements not shown on the specified PPT pages — even if they seem "complementary"
- [HARD] Thumbnail strips, image viewer panels, and acquisition-related components belong EXCLUSIVELY to Image Viewer / Acquisition specs (PPT slides 9-11) — never include in Worklist/Studylist views
- [HARD] After implementation, perform 1:1 comparison of each implemented element against the source PPT pages
- [HARD] If a PPT page says "이 사양서는 X만 해당합니다", treat ALL other screen types as forbidden for this task

Scope boundary map (from HnVUE UI PPT):
- Slides 1: LoginView.xaml ONLY
- Slides 2-4: PatientListView.xaml (Worklist) ONLY
- Slides 5-7: StudylistView.xaml (Studylist) ONLY
- Slides 8: AddPatientProcedureView.xaml ONLY
- Slides 9-11: WorkflowView.xaml (Acquisition) — includes thumbnail strip
- Slides 12-13: MergeView.xaml ONLY
- Slides 14-22: SettingsView.xaml ONLY

## Issue Protocol

- Theme/design token changes: notify Coordinator
- Emergency Stop position changes: create issue with QA/RA review required
- PPT spec scope violation: create issue with `team-design` + `bug` labels
- 기능구현 필요 발견: create issue with `needs-viewmodel` label + notify Coordinator

## Screenshot Rules [HARD]

- WindowHandle or ActiveWindow capture only (no Region capture)
- Verify: no black margins, no browser/IDE, no taskbar leak, no app chrome clipping
- Match framing/size with other images in the set
