# Team Design — Pure UI Design Rules

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

## Issue Protocol
- Theme/design token changes: notify Coordinator
- Emergency Stop position changes: create issue with QA/RA review required
