---
name: hnvue-skill-coordinator
description: >
  HnVue Coordinator integration engineering skill. Encodes UI.Contracts interface gate management,
  ViewModel composition with CommunityToolkit.Mvvm [ObservableProperty]/[RelayCommand],
  DI composition root (App.xaml.cs 13-module registration), navigation service, and integration testing.
  Coordinator is the SOLE modifier of UI.Contracts interfaces.
  Loaded by hnvue-coordinator agent. Triggers on: interface, DI, ViewModel, navigation, integration, contract.
user-invocable: false
metadata:
  version: "1.0.0"
  category: "domain"
  status: "active"
  updated: "2026-04-11"
  tags: "hnvue, coordinator, di, viewmodel, contracts, integration, navigation"

# MoAI Extension: Progressive Disclosure
progressive_disclosure:
  enabled: true
  level1_tokens: 100
  level2_tokens: 4500

# MoAI Extension: Triggers
triggers:
  keywords: ["interface", "DI", "viewmodel", "navigation", "integration", "contract", "composition root", "App.xaml", "UI.Contracts", "IMainViewModel"]
  agents: ["hnvue-coordinator"]
---

# HnVue Coordinator Integration Skill

Senior-level domain knowledge for Coordinator modules (UI.Contracts, UI.ViewModels, App).

## 1. UI.Contracts Interface Gate

Coordinator is the SOLE modifier of UI.Contracts interfaces. Any interface change requires impact analysis across all consumers.

**Interface inventory (12 contracts):**
- IViewModelBase: base (IsLoading, ErrorMessage)
- IMainViewModel: shell (CurrentView, NavigationHistory, OnLoginSuccess, RefreshFromContext, ResetSessionTimer)
- ILoginViewModel: auth (Username, Password, AvailableUserIds, LoginCommand, LoginSucceeded)
- IPatientListViewModel, IStudylistViewModel, IWorkflowViewModel, IDoseDisplayViewModel
- ICDBurnViewModel, ISystemAdminViewModel, IQuickPinLockViewModel, IMergeViewModel, ISettingsViewModel
- IImageViewerViewModel

**Service contracts:**
- INavigationService: NavigateTo(NavigationToken), GoBack(), CanGoBack, Navigated event
- IThemeService: CurrentTheme, AvailableThemes, ApplyTheme(ThemeInfo), ThemeChanged event
- IDialogService: ShowConfirmAsync, ShowErrorAsync, ShowWarningAsync

**NavigationToken enum (type-safe):**
Login, PatientList, Workflow, ImageViewer, DoseDisplay, CDBurn, SystemAdmin, QuickPinLock, Emergency, Studylist, Merge, Settings

**INavigationAware:** Optional interface for ViewModels receiving navigation parameters

**Breaking change protocol:**
- Create issue with `interface-contract` label
- Notify ALL affected teams
- Use interface segregation: prefer small, focused interfaces

## 2. ViewModel Composition (CommunityToolkit.Mvvm)

**Primary constructor injection pattern:**
```csharp
public sealed partial class LoginViewModel : ObservableObject, ILoginViewModel
{
    public LoginViewModel(ISecurityService securityService, ISecurityContext securityContext)
    {
        ArgumentNullException.ThrowIfNull(securityService);
    }
}
```

**Code generation attributes:**
- `[ObservableProperty]`: generates property + PropertyChanged
- `[RelayCommand]`: generates ICommand property with CanExecute
- `partial` class required for source generators

**ICommand bridge for interface covariance:**
```csharp
ICommand ILoginViewModel.LoginCommand => LoginCommand;
```

**MainViewModel (Singleton) patterns:**
- Session timeout: System.Timers.Timer (1s tick, 15-min inactivity + 3-min warning)
- Navigation stack: Stack<NavigationToken> for back navigation
- Audit logging: fire-and-forget LogoutAsync via ISecurityService
- Korean error messages in authentication flow

**Rules:**
- ViewModels inject domain services through constructor injection only
- ViewModels MUST NOT directly reference infrastructure (Data, Security internals)
- Use interfaces from UI.Contracts for ALL ViewModel dependencies
- Complex composition: use factories or builders, not service locator

## 3. DI Composition Root (App.xaml.cs)

**13-module registration order (dependency-aware):**
1. HnVue.Common -> ISecurityContext (Singleton)
2. HnVue.Data -> HnVueDbContext, IUserRepository, IPatientRepository, IStudyRepository
3. HnVue.Security -> ISecurityService, IAuditService, JwtTokenService
4. HnVue.Workflow -> IWorkflowEngine (Scoped), Simulators (Singleton)
5. HnVue.Dose -> IDoseService (Scoped), NullDoseRepository (stub)
6. HnVue.PatientManagement -> IPatientService, IWorklistService (Scoped)
7. HnVue.Incident -> IncidentResponseService (Scoped), NullIncidentRepository (stub)
8. HnVue.Update -> ISWUpdateService, BackupService
9. HnVue.SystemAdmin -> ISystemAdminService
10. HnVue.CDBurning -> ICDDVDBurnService (Scoped), IMAPIComWrapper
11. HnVue.Dicom -> DicomStoreScu, DicomFileIO (Singleton)
12. HnVue.Imaging -> IImageProcessor (Singleton)
13. ViewModels: all VMs against interface + concrete (Transient except MainViewModel = Singleton)

**Lifetime rules:**
- Singleton: stateless services, shared state (MainViewModel, NavigationService, MainWindow)
- Scoped: per-request domain services (WorkflowEngine, DoseService)
- Transient: ViewModels (except MainViewModel)

**Phase 1d stubs:** NullDoseRepository, NullWorklistRepository — satisfy DI until EF implementations

**Missing DI registration = App startup failure.** Always verify with integration test.

## 4. Navigation Service

**NavigationService (Singleton):**
- Resolves ViewModels from IServiceProvider
- Maps NavigationToken -> ViewModel type
- Sets MainViewModel.CurrentView
- Manages navigation stack for GoBack()
- Fires Navigated event

## 5. Design Team Handoff

When Design Team reports `NEEDS_VIEWMODEL`:
1. Add required properties/commands to UI.Contracts interface
2. Implement ViewModel with CommunityToolkit.Mvvm
3. Register in App.xaml.cs DI
4. Design Team connects XAML bindings

## 6. Integration Testing

- Every cross-module interaction must have integration test coverage
- Use real services with in-memory SQLite (not mocks)
- Test naming: {Module}_{Scenario}_{ExpectedResult}
- Test project: tests.integration/HnVue.IntegrationTests/

## 7. Cross-Module Protocol

- UI.Contracts changes: issue with `interface-contract` label + notify all teams
- DI registration changes: issue with `coordinator` label
- Architecture changes: notify RA for SAD/SDS update

## 8. Quality Enforcement Protocol [HARD]

Before writing any code, read `${CLAUDE_SKILL_DIR}/references/coordinator-patterns.md` for:
- ViewModel implementation template (CommunityToolkit.Mvvm patterns)
- DI registration order verification (wrong order = startup crash)
- Interface change impact analysis procedure
- Post-implementation verification script (5 steps including integration tests)

**Implementation flow:**
1. Read references/coordinator-patterns.md Pre-Implementation Checklist
2. Check UI.Contracts for existing interfaces (never duplicate)
3. Write ViewModel using [ObservableProperty]/[RelayCommand] with partial+sealed
4. Register in App.xaml.cs in correct dependency order
5. Write integration test verifying DI resolution
6. Run Post-Implementation Verification Script (all 5 steps)
7. Only report COMPLETED with build evidence
