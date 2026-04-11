---
name: hnvue-skill-ui
description: >
  HnVue Team Design pure UI engineering skill. Encodes WPF XAML with MahApps.Metro theming,
  3-tier design token architecture (Core/Semantic/Component), IEC 62366 accessibility (WCAG 2.1 AA),
  PPT-to-XAML codification workflow, DesignTime mock patterns, and component hierarchy.
  Design Team does NO business logic. Loaded by hnvue-ui agent.
  Triggers on: XAML, view, style, theme, MahApps, component, converter, accessibility, PPT, design token.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, ui, wpf, xaml, mahapps, design-system, accessibility, ppt"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["xaml", "view", "style", "theme", "mahapps", "component", "converter", "accessibility", "ppt", "design token", "wcag", "iec 62366"]
  agents: ["hnvue-ui"]
---

# HnVue UI Design Engineering Skill

Senior-level domain knowledge for HnVue WPF UI design codification. Design Team creates XAML only — no business logic, no ViewModels, no services.

## 1. Identity Rule [HARD]

Design Team = PPT/Figma design -> XAML codification team.
- XAML files and code-behind (InitializeComponent + pure UI events) ONLY
- NO service classes, NO ViewModels, NO domain models, NO DI registration
- When functionality is needed: report `NEEDS_VIEWMODEL: {property list}` to Coordinator

## 2. View Inventory (12 Views)

| View | PPT Slides | Path |
|------|-----------|------|
| LoginView | 1 | src/HnVue.UI/Views/LoginView.xaml |
| PatientListView | 2-4 | src/HnVue.UI/Views/PatientListView.xaml |
| StudylistView | 5-7 | src/HnVue.UI/Views/StudylistView.xaml |
| AddPatientProcedureView | 8 | src/HnVue.UI/Views/AddPatientProcedureView.xaml |
| WorkflowView | 9-11 | src/HnVue.UI/Views/WorkflowView.xaml |
| MergeView | 12-13 | src/HnVue.UI/Views/MergeView.xaml |
| SettingsView | 14-22 | src/HnVue.UI/Views/SettingsView.xaml |
| ImageViewerView | - | src/HnVue.UI/Views/ImageViewerView.xaml |
| DoseDisplayView | - | src/HnVue.UI/Views/DoseDisplayView.xaml |
| CDBurnView | - | src/HnVue.UI/Views/CDBurnView.xaml |
| SystemAdminView | - | src/HnVue.UI/Views/SystemAdminView.xaml |
| QuickPinLockView | - | src/HnVue.UI/Views/QuickPinLockView.xaml |

**PPT Scope Compliance [CRITICAL]:** Only implement specified PPT pages. No UI elements beyond PPT definition. See Issue #59.

## 3. Design Token Architecture (3-Tier)

**CoreTokens.xaml** — Raw primitives:
- Colors: `HnVue.Core.Color.Primary`, `HnVue.Core.Color.StatusSafe`, etc.
- FontSize: `HnVue.Core.FontSize.Base` (14px)
- Spacing: `HnVue.Core.Spacing.md` (16px)

**SemanticTokens.xaml** — Meaningful aliases:
- `HnVue.Semantic.Button.Primary`, `HnVue.Semantic.Button.Danger`
- `HnVue.Semantic.Border.Default`, `HnVue.Semantic.Text.Body`

**ComponentTokens.xaml** — Per-component overrides:
- Specific overrides for complex components (e.g., sidebar, header)

**Rules:**
- Views reference Semantic tokens, never Core directly
- Components may reference Component tokens for self-contained styling
- All tokens use `DynamicResource` for runtime theme switching

## 4. Theme System (3 Themes)

**DarkTheme.xaml (Default):**
- Brand=#1B4F8A, Accent=#00AEEF
- Safe=#00C853, Warning=#FFD600, Emergency=#D50000

**LightTheme.xaml:** Adapted light colors with equivalent status hierarchy

**HighContrastTheme.xaml (OR environments):**
- Saturated clinical colors: Green=#00FF00, Yellow=#FFFF00, Orange=#FF8800, Red=#FF0000

**MahApps.Metro integration:**
- All custom styles extend MahApps base styles
- Theme switching via MahApps ThemeManager (DynamicResource merge strategy)
- MetroWindow as base window class

## 5. Component Hierarchy

**Common:** Modal.xaml, Toast.xaml, Card.cs (MedicalButton, MedicalTextBox)
**Layout:** Header.xaml (navigation bar), Sidebar.cs, StatusBar.xaml (TLS warning)
**Medical:** PatientInfoCard.xaml, StudyThumbnail.xaml, AcquisitionPreview.xaml

**Ownership boundaries:**
- Pure layout/visual components: Design Team owns
- Medical domain components (C# logic): Team B owns C#, Design owns XAML template
- Modal/Toast with service logic: Coordinator owns C#

## 6. Converter Ownership

| Type | Examples | Owner |
|------|---------|-------|
| Pure visual | BoolToVisibility, InverseBool, NullToVisibility | Design |
| Tab/state UI | ActiveTabToVisibility, StringEqualityToBool | Design |
| Domain enum->color | SafeStateToColorConverter | Team B |
| Domain data->display | AgeFromBirthDate, DateOnlyToString | Team B |
| Multi-binding logic | MultiBoolAnd, MultiBoolOr | Design |

## 7. Accessibility (IEC 62366 / WCAG 2.1 AA)

- Color contrast ratio >= 4.5:1
- Touch targets >= 44x44px
- Keyboard Tab navigation for all interactive elements
- AutomationProperties.Name on all interactive controls
- Emergency Stop button: always visible on Acquisition screen, 56px min-height, always enabled
- Style key: `HnVue.EmergencyStopButton`

## 8. DesignTime Mock Pattern

- All Views must render in VS2022 designer without app running
- Use `d:DataContext` with `d:DesignInstance` for mock data
- Mock ViewModels in src/HnVue.UI/DesignTime/
- Korean names, realistic Korean patient IDs in mock data

## 9. Code-Behind Rules [HARD]

**Allowed:**
- InitializeComponent()
- ViewModel DI constructor
- WPF security constraint handlers (e.g., PasswordBox.PasswordChanged)
- Copy existing patterns only (reference: LoginView.xaml.cs PasswordBox pattern)

**Forbidden:**
- DB calls, API calls, business calculations, state change logic
- Animations: XAML Storyboard/VisualStateManager first, code-behind only if impossible

## 10. Dependency Restrictions (Architecture Tests Enforce)

**ALLOWED:** HnVue.Common, HnVue.UI.Contracts, MahApps.Metro, CommunityToolkit.Mvvm, LiveChartsCore
**FORBIDDEN:** HnVue.Data, HnVue.Security, HnVue.Workflow, HnVue.Imaging, HnVue.Dicom, HnVue.Dose, HnVue.PatientManagement, HnVue.Incident, HnVue.Update, HnVue.SystemAdmin, HnVue.CDBurning

## 11. Quality Enforcement Protocol [HARD]

Before writing any XAML, read `${CLAUDE_SKILL_DIR}/references/ui-patterns.md` for:
- PPT scope boundary verification (MUST confirm slide number first)
- Design token usage patterns (hardcoded colors = REJECTION)
- Accessibility checklist (every interactive element verified)
- Post-implementation visual verification checklist

**Implementation flow:**
1. Read references/ui-patterns.md Pre-Implementation Checklist
2. Confirm PPT slide number and list exact files to modify
3. Write XAML using DynamicResource semantic tokens only
4. Verify accessibility: AutomationProperties, TabIndex, MinHeight/MinWidth
5. Run Post-Implementation Verification (build + visual comparison)
6. Only report COMPLETED with build evidence + PPT 1:1 comparison
