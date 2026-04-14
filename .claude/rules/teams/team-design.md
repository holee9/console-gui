# Team Design — Pure UI Design Rules

Shared rules: see `team-common.md` (Philosophy, Self-Verification, Git Protocol)
Role boundaries: see `role-matrix.md` (ALLOWED/PROHIBITED 작업 매트릭스)

Additional Design-specific philosophy:
- [HARD] PPT scope compliance: only implement specified PPT pages
- [HARD] Evidence includes PPT 1:1 comparison result

## Module Ownership
- HnVue.UI/Views, Styles, Themes, Components, Converters, Assets
- HnVue.UI/DesignTime/ (Mock ViewModels)

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

## Code-Behind Rules
- Minimize code-behind in .xaml.cs files
- Only pure UI event handling allowed (no business logic)
- Use commands (ICommand) for all user actions
- Animations and visual state changes are OK in code-behind

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

## Screenshot Rules [HARD]
- WindowHandle or ActiveWindow capture only (no Region capture)
- Verify: no black margins, no browser/IDE, no taskbar leak, no app chrome clipping
- Match framing/size with other images in the set
