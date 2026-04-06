# UI-ARCH-001: GUI Replaceable Architecture Research Report v1.0

| Item | Value |
|------|-------|
| Document ID | UI-ARCH-001 |
| Version | 1.0 |
| Date | 2026-04-06 |
| Author | MoAI (Strategy Research) |
| Status | Research Complete |
| Classification | Architecture Decision Record |

---

## 1. Executive Summary

### 1.1 Goal

HnVue Console-GUI Application has a design requirement: **GUI must be replaceable independently from functional modules**. This document investigates whether this is realistic, what industry patterns exist, and provides a concrete implementation strategy for a 2-developer team.

### 1.2 Conclusion

**GUI Replaceable Architecture is REALISTIC and ACHIEVABLE** for HnVue.

| Factor | Assessment |
|--------|-----------|
| Technical Feasibility | **HIGH** - WPF + MVVM natively supports View replacement |
| Existing Foundation | **STRONG** - Separate HnVue.UI assembly, CommunityToolkit.Mvvm, MahApps.Metro, DI infrastructure already in place |
| Team Capacity (2 devs) | **ADEQUATE** - Progressive decoupling strategy avoids upfront overengineering |
| Industry Precedent | **ABUNDANT** - Standard pattern in industrial HMI/SCADA, medical device SW, and composite WPF applications |
| Recommended Approach | **Interface Contract + Design Token + DataTemplate-based View Mapping** (no Prism needed) |

### 1.3 Key Insight

The project already has 70% of the infrastructure needed. The remaining work is:
1. Formalizing the interface contract between UI and modules (`HnVue.UI.Contracts`)
2. Separating ViewModels from Views into distinct assemblies
3. Structuring Design Tokens as a layered ResourceDictionary system
4. Establishing a UI Design folder hierarchy independent of module structure

---

## 2. Current State Analysis

### 2.1 Architecture Summary

```
Layer 6: HnVue.App           (Shell, DI Composition Root)
Layer 5: HnVue.UI            (Views + ViewModels + Themes)  <-- TARGET FOR DECOUPLING
Layer 4: HnVue.Workflow       (State Machine)
Layer 3: HnVue.[Modules]     (Dose, Dicom, Imaging, etc.)
Layer 2: HnVue.Security      (Auth, RBAC, Audit)
Layer 1: HnVue.Data          (EF Core, Repository)
Layer 0: HnVue.Common        (Interfaces, DTOs, Enums)
```

### 2.2 Existing Strengths

| Strength | Detail |
|----------|--------|
| Separate UI Assembly | `HnVue.UI` is already a standalone project, not embedded in App |
| Modern MVVM Framework | CommunityToolkit.Mvvm 8.2.2 with `[ObservableProperty]`, `[RelayCommand]` |
| Theme Infrastructure | MahApps.Metro 2.4.10 with `ThemeManager.Current.ChangeTheme()` API |
| DI Container | Microsoft.Extensions.DependencyInjection with centralized registration |
| Interface-based Services | 17 service interfaces defined in HnVue.Common |
| Result Monad | `Result<T>` pattern for type-safe error propagation |

### 2.3 Current Limitations

| Limitation | Impact | Resolution |
|-----------|--------|------------|
| Views + ViewModels coexist in `HnVue.UI` | Cannot replace Views without replacing ViewModels | Separate into two assemblies |
| Single theme file (`HnVueTheme.xaml`) | No structured design token system | Build layered token architecture |
| No explicit UI-Module interface contract | Coupling points are implicit | Create `HnVue.UI.Contracts` |
| Some ViewModels use WPF types (`BitmapSource`) | Platform dependency in ViewModel | Abstract via interfaces |

---

## 3. Industry Research: Patterns & Precedents

### 3.1 MVVM Strict Separation

WPF's data binding infrastructure natively supports MVVM, where Views bind to ViewModel properties without direct coupling. The key rules for GUI replaceability:

| Rule | Description | Violation Impact |
|------|-------------|-----------------|
| ViewModel ignores View | No View object access from ViewModel, even via interfaces | GUI replacement impossible |
| Minimal code-behind | Only pure UI logic (animations, focus) in code-behind | Business logic leakage |
| No WPF types in ViewModel | Avoid `Visibility`, `Brush`, `BitmapSource` in ViewModels | Platform dependency |
| DataTemplate-based mapping | View-ViewModel connection via declarative DataTemplate | Hard-coded coupling |

**DataTemplate-based View Replacement**: WPF's `DataTemplateSelector` allows defining multiple Views for the same ViewModel and switching at runtime. `ContentControl.Content` binds to ViewModel, and DataTemplate auto-renders the appropriate View.

**Industry Adoption**: Standard practice. Every serious WPF application uses MVVM.

**2-dev Team Feasibility**: HIGH - Already using CommunityToolkit.Mvvm, minimal additional learning.

### 3.2 Skinnable/Themeable Architecture

WPF's ResourceDictionary system allows defining colors, templates, icons, and styles in XAML. Using `DynamicResource` (not `StaticResource`), swapping the ResourceDictionary at runtime instantly updates the entire UI.

**Three Approaches:**

| Approach | Mechanism | Pros | Cons |
|----------|-----------|------|------|
| Compiled ResourceDictionary | Add/remove from `MergedDictionaries` | Stable, fast | Requires recompilation for new themes |
| Loose XAML Loading | `XamlReader.Load()` at runtime | Deploy-time customization | Security validation needed |
| MahApps.Metro ThemeManager | `ThemeManager.Current.ChangeTheme()` | Already in project | Limited to MahApps themes |

**HnVue Recommendation**: Start with MahApps.Metro ThemeManager (already available), then add custom ResourceDictionary layers for design tokens.

### 3.3 Plugin-based UI (Prism/MEF)

Prism provides Shell, Region, Module concepts for composite WPF applications. Modules can be loaded at runtime from DLL files.

**Assessment for HnVue:**

| Criterion | Score | Rationale |
|-----------|-------|-----------|
| Power | 9/10 | Full runtime UI composition |
| Complexity | 8/10 | Major refactoring of existing codebase |
| 2-dev suitability | 3/10 | Overengineering for 14-module project |
| Learning curve | 7/10 | Significant new concepts (Region, Module, Navigation) |

**Verdict: NOT RECOMMENDED** for HnVue. CommunityToolkit.Mvvm + Interface-based separation achieves the same goals with far less complexity.

### 3.4 UI Shell Pattern

Visual Studio-style Shell architecture where the main window defines named regions (Header, Navigation, Content, Status) and modules inject Views into those regions at runtime.

**Applicable to HnVue**: `MainWindow.xaml` already serves as Shell. Enhancement needed:
- Define named `ContentControl` regions
- Central navigation service manages View transitions
- Each module registers its Views with the navigation service

**Feasibility**: HIGH - Can be implemented with basic WPF without external frameworks.

### 3.5 Design System / Design Tokens

Design Tokens abstract design decisions (colors, typography, spacing) into named variables. In WPF, they map naturally to ResourceDictionary key-value pairs.

**Token Hierarchy:**

```
Level 1: Core Tokens         (Color palette, font sizes, spacing scale)
Level 2: Semantic Tokens      (PrimaryButtonBg, ErrorText, WarningBorder)
Level 3: Component Tokens     (DataGridHeaderBg, ChartAxisColor)
```

**Industry Adoption**: Originally from web (Salesforce Lightning, Material Design), now spreading to desktop. MahApps.Metro, WPF UI, and Material Design XAML Toolkit all use token-like systems.

### 3.6 Real-World Precedents

| Project/Company | Pattern | Result |
|----------------|---------|--------|
| WPF UI (lepoco/wpfui) | Fluent Design with fully separated themes/navigation | 8,000+ GitHub stars, proven at scale |
| ModernWpf (Kinnara) | Light/Dark theme switching with stock WPF controls | Clean separation demonstrated |
| Material Design XAML Toolkit | Complete skin system for WPF | Most popular WPF design library |
| OAS (Open Automation Software) | Same business logic driving WPF panels AND Blazor dashboards | Proves UI layer replaceability in industrial context |
| Mesta Automation Simple-HMI | WPF + MVVM for industrial HMI | Open-source reference for HMI architecture |
| Adonis UI | Consistent modern design with easy theme replacement | Validates token-based approach |

---

## 4. Recommended Architecture

### 4.1 Target Project Structure

```
src/
  HnVue.App/                          [Shell - Composition Root]
    Program.cs                         DI registration, startup
    MainWindow.xaml                    Shell with named regions
    MainWindow.xaml.cs                 Minimal code-behind

  HnVue.UI.Contracts/                  [NEW - Interface Layer]
    Navigation/
      INavigationService.cs            View navigation contract
      INavigationAware.cs              ViewModel navigation lifecycle
      NavigationToken.cs               Type-safe navigation identifiers
    Dialogs/
      IDialogService.cs                Dialog display contract
      DialogResult.cs                  Dialog result types
    Notifications/
      INotificationService.cs          Toast/snackbar notifications
    Theming/
      IThemeService.cs                 Theme switching contract
      ThemeInfo.cs                     Theme metadata
    ViewModels/
      IViewModelBase.cs                Common ViewModel contract
      IPatientListViewModel.cs         Per-feature ViewModel contracts
      IImageViewerViewModel.cs
      IWorkflowViewModel.cs
      IDoseDisplayViewModel.cs
      ICDBurnViewModel.cs
      ISystemAdminViewModel.cs
    Events/
      NavigationRequestedMessage.cs    Typed messages for Messenger
      SessionTimeoutMessage.cs
      WorkflowStateChangedMessage.cs

  HnVue.UI.ViewModels/                [NEW - ViewModel Assembly]
    ViewModels/
      MainViewModel.cs
      LoginViewModel.cs
      PatientListViewModel.cs
      WorkflowViewModel.cs
      ImageViewerViewModel.cs
      DoseDisplayViewModel.cs
      DoseViewModel.cs
      CDBurnViewModel.cs
      SystemAdminViewModel.cs
      QuickPinLockViewModel.cs
    Services/
      NavigationService.cs             INavigationService implementation
      DialogService.cs                 IDialogService implementation
      ThemeService.cs                  IThemeService implementation

  HnVue.UI/                           [View Assembly - REPLACEABLE]
    Views/
      LoginView.xaml
      PatientListView.xaml
      WorkflowView.xaml
      ImageViewerView.xaml
      DoseDisplayView.xaml
      CDBurnView.xaml
      SystemAdminView.xaml
      QuickPinLockView.xaml
    Controls/                          Reusable custom controls
      DoseGaugeControl.xaml
      ImageToolbar.xaml
      StatusIndicator.xaml
    Converters/
      BoolToVisibilityConverter.cs
      NullToVisibilityConverter.cs
      SafeStateToColorConverter.cs
    Themes/
      _ThemeLoader.cs                  Dynamic ResourceDictionary loader
      tokens/
        CoreTokens.xaml                Colors, fonts, spacing (Level 1)
        SemanticTokens.xaml            Meaningful names (Level 2)
        ComponentTokens.xaml           Per-component tokens (Level 3)
      light/
        LightTheme.xaml                Light theme overrides
      dark/
        DarkTheme.xaml                 Dark theme overrides
      high-contrast/
        HighContrastTheme.xaml         Industrial/accessibility theme
    DataTemplates/
      ViewMappings.xaml                DataTemplate View-ViewModel bindings
    Resources/
      Icons/                           SVG/XAML icon resources
      Images/                          Static image assets
      Animations/                      Storyboard animations

  HnVue.Common/                        [Unchanged]
  HnVue.Data/                          [Unchanged]
  HnVue.Security/                      [Unchanged]
  HnVue.Workflow/                      [Unchanged]
  HnVue.Dicom/                         [Unchanged]
  HnVue.Imaging/                       [Unchanged]
  HnVue.Dose/                          [Unchanged]
  HnVue.Incident/                      [Unchanged]
  HnVue.Update/                        [Unchanged]
  HnVue.PatientManagement/             [Unchanged]
  HnVue.SystemAdmin/                   [Unchanged]
  HnVue.CDBurning/                     [Unchanged]
```

### 4.2 Dependency Graph

```
                    HnVue.App (Shell)
                   /    |    \     \
                  /     |     \     \
    HnVue.UI   HnVue.UI.ViewModels  HnVue.[Modules]...
       |              |         \         |
       |              |          \        |
       v              v           v       v
    HnVue.UI.Contracts          HnVue.Common
                                    |
                                    v
                                HnVue.Data
```

**Critical dependency rules:**
- `HnVue.UI` depends ONLY on `HnVue.UI.Contracts` (never on modules)
- `HnVue.UI.ViewModels` depends on `HnVue.UI.Contracts` + `HnVue.Common`
- `HnVue.[Module]` depends on `HnVue.Common` (never on UI)
- `HnVue.App` references everything (composition root)

### 4.3 GUI Replacement Mechanism

To replace the GUI:

1. Create a new project (e.g., `HnVue.UI.Alternative`)
2. Reference only `HnVue.UI.Contracts`
3. Implement Views that bind to the same ViewModel contracts
4. In `HnVue.App`, swap the project reference from `HnVue.UI` to `HnVue.UI.Alternative`
5. Update `ViewMappings.xaml` DataTemplates

No business logic, no module code, no ViewModel code needs to change.

---

## 5. Integration Contract Specification

### 5.1 Navigation Contract

```
INavigationService
  NavigateTo(NavigationToken token) : void
  NavigateTo(NavigationToken token, object parameter) : void
  GoBack() : bool
  CanGoBack : bool

INavigationAware (implemented by ViewModels)
  OnNavigatedTo(object parameter) : void
  OnNavigatedFrom() : void

NavigationToken (enum or string constants)
  Login, PatientList, Workflow, ImageViewer,
  DoseDisplay, CDBurn, SystemAdmin, QuickPinLock
```

### 5.2 Dialog Contract

```
IDialogService
  ShowConfirmAsync(title, message) : Task<bool>
  ShowErrorAsync(title, message) : Task
  ShowWarningAsync(title, message) : Task
  ShowInputAsync(title, prompt) : Task<string?>
  ShowCustomAsync<TViewModel>(TViewModel vm) : Task<DialogResult>
```

### 5.3 Theme Contract

```
IThemeService
  CurrentTheme : ThemeInfo
  AvailableThemes : IReadOnlyList<ThemeInfo>
  ApplyTheme(ThemeInfo theme) : void
  ThemeChanged : event EventHandler<ThemeInfo>

ThemeInfo
  Id : string
  DisplayName : string
  IsDark : bool
  AccentColor : string (hex)
```

### 5.4 ViewModel Base Contract

```
IViewModelBase
  IsLoading : bool
  ErrorMessage : string?
  IsInitialized : bool
  InitializeAsync() : Task
  CleanupAsync() : Task
```

### 5.5 Event Bus (Messenger Pattern)

Using CommunityToolkit.Mvvm's `WeakReferenceMessenger`:

| Message | Sender | Receiver | Payload |
|---------|--------|----------|---------|
| NavigationRequestedMessage | Any ViewModel | MainViewModel | NavigationToken + Parameter |
| SessionTimeoutMessage | MainViewModel | All | TimeRemaining (seconds) |
| WorkflowStateChangedMessage | WorkflowViewModel | DoseDisplay, ImageViewer | WorkflowState enum |
| PatientSelectedMessage | PatientListViewModel | Workflow, ImageViewer | PatientId |
| EmergencyStopMessage | DoseDisplayViewModel | Workflow, MainViewModel | Severity level |
| LoginSucceededMessage | LoginViewModel | MainViewModel | UserSession info |

---

## 6. Design Token Architecture

### 6.1 Token Hierarchy

```
CoreTokens.xaml (Level 1 - Primitive Values)
  ├── Colors: Primary, Secondary, Accent, Neutral palette
  ├── Typography: FontFamily, FontSize scale (Caption to H1)
  ├── Spacing: 4px base unit scale (4, 8, 12, 16, 24, 32, 48)
  └── Radii: CornerRadius scale (2, 4, 8, 12)

SemanticTokens.xaml (Level 2 - Meaningful Names)
  ├── Surfaces: PageBackground, CardBackground, OverlayBackground
  ├── Text: TextPrimary, TextSecondary, TextDisabled, TextOnPrimary
  ├── Borders: BorderDefault, BorderFocused, BorderError, BorderWarning
  ├── Interactive: ButtonPrimary, ButtonSecondaryBg, ButtonDangerBg
  └── Status: StatusSafe(green), StatusWarning(yellow), StatusBlocked(orange), StatusEmergency(red)

ComponentTokens.xaml (Level 3 - Per-Component)
  ├── DataGrid: HeaderBg, RowAlternateBg, SelectionBg
  ├── Chart: AxisColor, GridLineColor, SeriesColors[]
  ├── StatusBar: Background, TextColor, SeparatorColor
  └── ImageViewer: ToolbarBg, RulerColor, AnnotationColor
```

### 6.2 Theme Override Mechanism

Each theme (Light, Dark, HighContrast) overrides only the Core Tokens. Semantic and Component tokens reference Core tokens via `DynamicResource`, so they automatically update when the theme changes.

```
Application Load Order:
  1. CoreTokens.xaml          (base palette)
  2. SemanticTokens.xaml      (references Core via DynamicResource)
  3. ComponentTokens.xaml     (references Semantic via DynamicResource)
  4. [Theme]/ThemeOverrides.xaml  (overrides Core tokens only)
```

Switching themes = replacing step 4 only.

### 6.3 Safety-Critical Color Mapping

Medical device UI requires specific color semantics per IEC 62366:

| State | Color | Token Name | Hex (Light) | Hex (Dark) |
|-------|-------|-----------|-------------|------------|
| Safe/Idle | Green | StatusSafe | #2E7D32 | #66BB6A |
| Warning | Yellow/Amber | StatusWarning | #F57F17 | #FFD54F |
| Blocked | Orange | StatusBlocked | #E65100 | #FF9800 |
| Emergency | Red | StatusEmergency | #C62828 | #EF5350 |
| Radiation Active | Blue pulse | StatusRadiation | #1565C0 | #42A5F5 |

These tokens are **non-negotiable** and must be preserved across ALL theme variants for regulatory compliance.

---

## 7. UI Design Folder System (Separated from Modules)

### 7.1 Design Documentation Structure

```
docs/
  design/                              [NEW - UI Design Root]
    README.md                          Design system overview & principles
    
    specifications/                    Design specifications
      UI-SPEC-001_DesignSystem.md      Design token definitions & usage
      UI-SPEC-002_NavigationFlow.md    Screen flow & navigation map
      UI-SPEC-003_ComponentLibrary.md  Reusable component catalog
      UI-SPEC-004_Accessibility.md     WCAG/IEC 62366 compliance
      UI-SPEC-005_Responsiveness.md    Resolution & DPI adaptation
    
    wireframes/                        Screen layouts
      login.md                         Login screen wireframe
      patient-list.md                  Patient list wireframe
      workflow.md                      Workflow control wireframe
      image-viewer.md                  Image viewer wireframe
      dose-display.md                  Dose monitoring wireframe
      cd-burn.md                       CD burning wireframe
      system-admin.md                  System admin wireframe
      main-shell.md                    Shell layout with regions
    
    usability/                         Usability testing
      test-plan.md                     Usability test plan (IEC 62366)
      test-results/                    Test result records
      heuristic-evaluation.md          Nielsen heuristic evaluation
      task-analysis.md                 Task flow analysis
    
    themes/                            Theme design documentation
      light-theme.md                   Light theme color map
      dark-theme.md                    Dark theme color map
      high-contrast.md                 High contrast for clinical use
    
    research/                          UI research & benchmarks
      competitor-analysis.md           Competitor UI comparison
      user-interviews.md               User interview summaries
      UI-ARCH-001_*.md                 This document (architecture)
    
    changelog.md                       Design change history
```

### 7.2 Separation Principle

```
docs/design/          - WHAT the UI should look like (designer domain)
                        - Screen specifications
                        - Usability requirements
                        - Theme definitions
                        - Changed by: UX decisions, usability testing results

docs/planning/        - WHAT the system should do (architect domain)
                        - FRS, SRS, SAD, SDS
                        - Module specifications
                        - Changed by: Requirements changes

src/HnVue.UI/        - HOW the UI is implemented (developer domain)
                        - XAML Views
                        - Resource dictionaries
                        - Changed by: Implementation work

src/HnVue.[Module]/  - HOW features work (developer domain)
                        - Business logic
                        - Changed by: Feature development
```

**Key Principle**: Modifying anything in `docs/design/` never requires touching `docs/planning/`. Modifying `src/HnVue.UI/` never requires touching `src/HnVue.[Module]/`.

---

## 8. Integration Guide: UI-to-Module Binding

### 8.1 How a View Connects to a Module Feature

```
Step 1: Module defines service interface
  HnVue.Common/Services/IPatientService.cs
    GetPatientsAsync() : Task<Result<List<PatientDto>>>

Step 2: Module implements service
  HnVue.PatientManagement/PatientService.cs
    implements IPatientService

Step 3: UI.Contracts defines ViewModel contract
  HnVue.UI.Contracts/ViewModels/IPatientListViewModel.cs
    Patients : ObservableCollection<PatientDto>
    LoadPatientsCommand : IRelayCommand
    SelectedPatient : PatientDto?

Step 4: UI.ViewModels implements ViewModel
  HnVue.UI.ViewModels/PatientListViewModel.cs
    constructor(IPatientService patientService)
    implements IPatientListViewModel

Step 5: UI creates View bound to ViewModel
  HnVue.UI/Views/PatientListView.xaml
    DataContext type: IPatientListViewModel
    Binds to Patients, LoadPatientsCommand, SelectedPatient

Step 6: App registers all in DI
  HnVue.App/Program.cs
    services.AddTransient<IPatientService, PatientService>();
    services.AddTransient<IPatientListViewModel, PatientListViewModel>();

Step 7: DataTemplate maps ViewModel to View
  HnVue.UI/DataTemplates/ViewMappings.xaml
    <DataTemplate DataType="{x:Type contracts:IPatientListViewModel}">
      <views:PatientListView />
    </DataTemplate>
```

### 8.2 Adding a New Feature Module Without Touching UI

```
1. Create HnVue.NewFeature project
2. Define INewFeatureService in HnVue.Common
3. Implement NewFeatureService in HnVue.NewFeature
4. Define INewFeatureViewModel in HnVue.UI.Contracts
5. Implement NewFeatureViewModel in HnVue.UI.ViewModels
6. Create NewFeatureView.xaml in HnVue.UI
7. Add DataTemplate mapping in ViewMappings.xaml
8. Register in DI (HnVue.App)
9. Add NavigationToken for the new screen
```

Steps 1-4 are module work. Steps 5-8 are UI work. They can be done independently.

### 8.3 Replacing the Entire GUI

```
1. Create new project: HnVue.UI.V2
2. Reference HnVue.UI.Contracts (same contracts)
3. Implement all Views with new design
4. Create new ViewMappings.xaml
5. Create new Design Tokens (CoreTokens, SemanticTokens, ComponentTokens)
6. In HnVue.App: replace HnVue.UI reference with HnVue.UI.V2
7. Update App.xaml MergedDictionaries to point to V2 resources
```

**Zero changes needed in**: HnVue.Common, HnVue.Data, HnVue.Security, HnVue.Workflow, HnVue.Dicom, HnVue.Imaging, HnVue.Dose, HnVue.Incident, HnVue.Update, HnVue.PatientManagement, HnVue.SystemAdmin, HnVue.CDBurning, HnVue.UI.ViewModels.

---

## 9. Feasibility Assessment

### 9.1 Approach Comparison Matrix

| Approach | Feasibility | Benefit | Complexity | 2-Dev Fit | Priority |
|----------|------------|---------|-----------|-----------|----------|
| MVVM Strict Separation | 9/10 | 9/10 | 3/10 | Excellent | **P1** |
| Design Token System | 8/10 | 8/10 | 4/10 | Good | **P2** |
| UI Shell Pattern (Regions) | 8/10 | 7/10 | 4/10 | Good | **P3** |
| Interface Contract Layer | 7/10 | 9/10 | 5/10 | Good | **P4** |
| ViewModel Separate Assembly | 6/10 | 7/10 | 6/10 | Feasible | **P5** |
| Prism Plugin Architecture | 4/10 | 8/10 | 8/10 | Poor | **Skip** |

### 9.2 Risk Analysis

| Risk | Probability | Impact | Mitigation |
|------|------------|--------|------------|
| Over-abstraction slows development | Medium | High | Apply progressively, not all at once |
| ViewModel separation breaks existing tests | Low | Medium | Move files, update namespaces; tests logic unchanged |
| WPF type leakage into ViewModels | Medium | Medium | Code review checklist, static analysis |
| Design Token maintenance overhead | Low | Low | Token hierarchy keeps changes localized |
| Team resistance to new structure | Low | Medium | Document benefits, show before/after |

### 9.3 Effort Estimation

| Phase | Scope | Impact |
|-------|-------|--------|
| Phase 1: Design Token Setup | Create token XAML files, migrate `DynamicResource` usage | Theme switching works |
| Phase 2: UI.Contracts Project | Define interfaces for Navigation, Dialog, Theme, ViewModel base | Contract layer established |
| Phase 3: ViewModel Separation | Move ViewModels to `HnVue.UI.ViewModels`, update references | Views become replaceable |
| Phase 4: DataTemplate Mapping | Implement `ViewMappings.xaml`, Shell regions | Full GUI replaceability |
| Phase 5: PoC Alternative GUI | Create minimal alternative View assembly | Validates architecture |

---

## 10. Comparison with Alternative Approaches

### 10.1 Why NOT Prism?

| Factor | Prism | Proposed Approach |
|--------|-------|-------------------|
| Learning curve | 40+ hours for team | 8-12 hours for team |
| Codebase refactoring | Major (all bootstrapping changes) | Incremental (project by project) |
| Runtime complexity | Module discovery, Region injection | Simple DI + DataTemplate |
| Debugging difficulty | Prism's event aggregator, navigation journal | Standard WPF data binding |
| Maintenance burden | Prism version upgrades, breaking changes | No external framework dependency |
| Overkill threshold | Designed for 50+ module enterprise apps | HnVue has 14 modules |

### 10.2 Why NOT Blazor Hybrid?

Blazor Hybrid (WebView2 in WPF) would allow web-based UI that's easily replaceable, but:
- Medical device certification requires consistent rendering behavior
- WebView2 introduces browser engine dependency
- Performance overhead for real-time image display
- Not suitable for ImageViewer (pixel-level control needed)

### 10.3 Why NOT MAUI?

.NET MAUI could replace WPF, but:
- MAUI desktop support is less mature than WPF
- Medical device validation would need complete re-testing
- No compelling benefit over WPF for Windows-only deployment
- MahApps.Metro ecosystem doesn't exist for MAUI

---

## 11. Implementation Roadmap

### Phase 1: Foundation (Immediate, during current UI development)

**Actions:**
- Create `docs/design/` folder hierarchy
- Write `UI-SPEC-001_DesignSystem.md` with token definitions
- Create `CoreTokens.xaml`, `SemanticTokens.xaml`, `ComponentTokens.xaml`
- Ensure all existing XAML uses `DynamicResource` (not `StaticResource`)
- Document current View-ViewModel mappings

**Gate:** Theme can be switched at runtime via MahApps.Metro ThemeManager

### Phase 2: Contract Layer (Before new View development)

**Actions:**
- Create `HnVue.UI.Contracts` project
- Define `INavigationService`, `IDialogService`, `IThemeService`
- Define `IViewModelBase` and per-feature ViewModel interfaces
- Define typed Messenger messages
- Register contracts in DI

**Gate:** All ViewModels implement contract interfaces

### Phase 3: Separation (After Phase 2 Views are implemented)

**Actions:**
- Create `HnVue.UI.ViewModels` project
- Move ViewModel classes from `HnVue.UI` to `HnVue.UI.ViewModels`
- Remove WPF type dependencies from ViewModels (abstract `BitmapSource` etc.)
- Create `ViewMappings.xaml` for DataTemplate-based View resolution
- Update `HnVue.App` DI registration

**Gate:** `HnVue.UI` project has zero ViewModel classes, only XAML Views

### Phase 4: Validation (When all screens are implemented)

**Actions:**
- Create minimal `HnVue.UI.Stub` project with empty Views
- Verify application starts and navigates with stub Views
- Run usability tests with current UI
- Document results in `docs/design/usability/`

**Gate:** Alternative UI assembly loads successfully without any module code changes

---

## 12. References

### Academic & Standards

- IEC 62366-1:2015 - Medical devices - Usability engineering
- IEC 62304:2006/AMD1:2015 - Medical device software lifecycle
- ISO 9241-210:2019 - Human-centred design for interactive systems

### Industry Sources

- [WPF Best Practices 2024 - PostSharp Blog](https://blog.postsharp.net/wpf-best-practices-2024)
- [MVVM Pattern for WPF - Microsoft Learn](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/patterns-wpf-apps-with-the-model-view-viewmodel-design-pattern)
- [CommunityToolkit.Mvvm - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [WPF Complete Guide to Themes and Skins - Michael's Coding Spot](https://michaelscodingspot.com/wpf-complete-guide-themes-skins/)
- [Prism Composite Applications - Microsoft Learn](https://learn.microsoft.com/en-us/archive/msdn-magazine/2008/september/prism-patterns-for-building-composite-applications-with-wpf)
- [WPF UI (Fluent) - GitHub](https://github.com/lepoco/wpfui)
- [MVVM Best Practices - Rico Suter](https://blog.rsuter.com/recommendations-best-practices-implementing-mvvm-xaml-net-applications/)
- [Dependency Injection in WPF - Medium](https://medium.com/@shanto462/dependency-injection-in-wpf-a-complete-implementation-guide-468abcf95337)
- [WPF and Blazor for SCADA HMI - SviluppatoreMigliore](https://sviluppatoremigliore.com/en/blog/wpf-blazor-hmi-scada-industrial-interfaces)
- [HMI with C# and WPF - Mesta Automation](https://www.mesta-automation.com/how-to-write-an-hmi-with-c-and-wpf-part-1-of-x/)
- [Splitting WPF Apps into Multiple Projects - CODE Framework](https://docs.codeframework.io/Splitting-WPF-Apps-into-Multiple-Projects)
- [DataTemplate View Switching in WPF MVVM](https://www.technical-recipes.com/2016/switching-between-wpf-xaml-views-using-mvvm-datatemplate/)
- [Visual Studio Shell Architecture - Microsoft Learn](https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/visual-studio-shell)

### Open Source References

- [WPF UI (lepoco/wpfui)](https://github.com/lepoco/wpfui) - 8,000+ stars
- [ModernWpf (Kinnara)](https://github.com/Kinnara/ModernWpf) - Light/Dark theme reference
- [Material Design In XAML Toolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) - Comprehensive skin system
- [Adonis UI](https://benruehl.github.io/adonis-ui/) - Token-based design approach
- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro) - Already in project

---

## Appendix A: Glossary

| Term | Definition |
|------|-----------|
| Design Token | Named variable representing a design decision (color, spacing, font size) |
| DataTemplate | WPF mechanism that automatically selects a View for a given ViewModel type |
| DynamicResource | WPF resource reference that updates when the source ResourceDictionary changes |
| Region | Named placeholder in Shell where Views are injected at runtime |
| Shell | Root application window that hosts navigation and content regions |
| Composition Root | Single location where all DI registrations are configured (HnVue.App) |
| Loose XAML | XAML files loaded at runtime from disk (not compiled into assembly) |
| GUI Replaceability | Ability to swap the entire View layer without changing business logic |

---

Document End.
