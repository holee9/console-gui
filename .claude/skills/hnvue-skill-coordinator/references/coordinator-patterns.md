# Coordinator Implementation Quality Guide

Coordinator agent reads this file when implementing ViewModel, DI registration, interface contracts, or integration tests.

## Pre-Implementation Checklist

Before writing any code in Coordinator modules:

1. Check existing UI.Contracts interfaces — never duplicate
2. Verify DI registration order in App.xaml.cs — dependencies must be registered before consumers
3. Check for NEEDS_VIEWMODEL reports from Design Team in DISPATCH
4. Verify CommunityToolkit.Mvvm source generators are enabled (partial class required)

## ViewModel Implementation Pattern

### Correct: Full ViewModel with CommunityToolkit.Mvvm

```csharp
public sealed partial class PatientListViewModel : ObservableObject, IPatientListViewModel, INavigationAware
{
    private readonly IPatientService _patientService;
    private readonly INavigationService _navigationService;

    public PatientListViewModel(
        IPatientService patientService,
        INavigationService navigationService)
    {
        ArgumentNullException.ThrowIfNull(patientService);
        ArgumentNullException.ThrowIfNull(navigationService);
        _patientService = patientService;
        _navigationService = navigationService;
    }

    [ObservableProperty]
    private ObservableCollection<PatientRecord> _patients = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    private async Task LoadPatientsAsync(CancellationToken ct)
    {
        IsLoading = true;
        ErrorMessage = null;
        
        var result = await _patientService.SearchAsync(string.Empty, ct);
        result.Match(
            onSuccess: patients => Patients = new ObservableCollection<PatientRecord>(patients),
            onFailure: error => ErrorMessage = error.ToKoreanMessage()
        );
        
        IsLoading = false;
    }

    // ICommand bridge for interface covariance
    ICommand IPatientListViewModel.LoadPatientsCommand => LoadPatientsCommand;

    // INavigationAware
    public void OnNavigatedTo(object? parameter)
    {
        LoadPatientsCommand.Execute(null);
    }
}
```

### Anti-Patterns

- Missing `partial` keyword (source generators won't work)
- Missing `sealed` (ViewModels should be sealed — no inheritance)
- Direct infrastructure references (using HnVue.Data directly instead of interfaces)
- Missing ArgumentNullException.ThrowIfNull on constructor parameters
- Using async void instead of async Task in commands
- Missing ICommand bridge for interface properties (covariance issue)
- Forgetting to set IsLoading = false in error paths

## Interface Contract Quality Gate

### Adding a New Interface Property

1. Add to UI.Contracts interface FIRST
2. Implement in ViewModel
3. Create issue with `interface-contract` label
4. Notify ALL teams that consume the interface

### Correct: Interface design

```csharp
// Small, focused interfaces (Interface Segregation Principle)
public interface IPatientListViewModel : IViewModelBase
{
    ObservableCollection<PatientRecord> Patients { get; }
    string? SearchQuery { get; set; }
    ICommand LoadPatientsCommand { get; }
    ICommand SelectPatientCommand { get; }
}
```

### Anti-Pattern: Fat interface

```csharp
// WRONG — too many responsibilities in one interface
public interface IPatientListViewModel : IViewModelBase
{
    // Patient list
    ObservableCollection<PatientRecord> Patients { get; }
    // Study management (should be separate)
    ObservableCollection<StudyRecord> Studies { get; }
    // Dose display (should be separate)
    DoseRecord CurrentDose { get; }
    // Navigation (should use INavigationService)
    void NavigateToImageViewer();
}
```

## DI Registration Quality Gate

### Correct: Registration order (dependency-aware)

```csharp
// In App.xaml.cs OnStartup or ConfigureServices:
// 1. Foundation first
services.AddSingleton<ISecurityContext, SecurityContext>();

// 2. Data layer
services.AddDbContext<HnVueDbContext>(/* SQLite config */);
services.AddScoped<IUserRepository, UserRepository>();

// 3. Domain services (depend on data)
services.AddScoped<ISecurityService, SecurityService>();

// 4. ViewModels (depend on domain services)
services.AddTransient<ILoginViewModel, LoginViewModel>();
services.AddTransient<LoginViewModel>(); // Also register concrete for internal use

// 5. Navigation (depends on ViewModel resolution)
services.AddSingleton<INavigationService, NavigationService>();

// 6. MainViewModel (Singleton — shell)
services.AddSingleton<IMainViewModel, MainViewModel>();
services.AddSingleton<MainViewModel>();
```

### Anti-Patterns

- Registering consumer before dependency (runtime DI failure)
- Missing concrete type registration alongside interface (needed for some resolution patterns)
- Using Singleton for ViewModels that hold per-view state (use Transient)
- Using Transient for MainViewModel (must be Singleton — only one shell)
- Forgetting to register new ViewModel (App startup crash)

### DI Verification Test

```csharp
[Fact]
public void AllViewModels_CanBeResolved_FromServiceProvider()
{
    var services = new ServiceCollection();
    ConfigureAllServices(services);
    using var provider = services.BuildServiceProvider(validateScopes: true);
    
    // Verify every ViewModel interface resolves
    provider.GetRequiredService<ILoginViewModel>().Should().NotBeNull();
    provider.GetRequiredService<IPatientListViewModel>().Should().NotBeNull();
    provider.GetRequiredService<IMainViewModel>().Should().NotBeNull();
    // ... all 12 ViewModels
}
```

## Navigation Service Pattern

### Adding a New View

1. Add NavigationToken enum value
2. Add ViewModel interface to UI.Contracts
3. Implement ViewModel
4. Register in DI (App.xaml.cs)
5. Add token-to-ViewModel mapping in NavigationService
6. Create View (Design Team)
7. Add DataTemplate in App.xaml or resource dictionary

### Navigation Token Mapping

```csharp
private IViewModelBase ResolveViewModel(NavigationToken token) => token switch
{
    NavigationToken.Login => _provider.GetRequiredService<ILoginViewModel>() as IViewModelBase,
    NavigationToken.PatientList => _provider.GetRequiredService<IPatientListViewModel>() as IViewModelBase,
    NavigationToken.Studylist => _provider.GetRequiredService<IStudylistViewModel>() as IViewModelBase,
    // ... all tokens mapped
    _ => throw new ArgumentOutOfRangeException(nameof(token))
};
```

## Integration Test Patterns

### Correct: Cross-module integration test

```csharp
[Fact]
public async Task LoginViewModel_ValidLogin_NavigatesToPatientList()
{
    // Arrange — use real services with in-memory SQLite
    await using var context = CreateInMemoryDbContext();
    var securityService = new SecurityService(
        new UserRepository(context, NullLogger<UserRepository>.Instance),
        new AuditRepository(context, NullLogger<AuditRepository>.Instance),
        new JwtTokenService(Options.Create(new JwtSettings { SecretKey = "test-key-32-chars-minimum-length!" })),
        NullLogger<SecurityService>.Instance);
    
    await securityService.RegisterUserAsync("testuser", "P@ssw0rd!", ct: default);
    
    var navigationService = new MockNavigationService();
    var sut = new LoginViewModel(securityService, new SecurityContext());
    
    // Act
    sut.Username = "testuser";
    sut.Password = "P@ssw0rd!";
    await sut.LoginCommand.ExecuteAsync(null);
    
    // Assert
    sut.ErrorMessage.Should().BeNull();
    // LoginSucceeded event should have fired
}
```

### Anti-Patterns in Integration Tests

- Using mocks instead of real services (defeats integration testing purpose)
- Not using in-memory SQLite (testing against file-based DB is fragile)
- Missing CancellationToken
- Testing only happy path (must test auth failure, DI resolution failure)

## Post-Implementation Verification

```bash
# 1. Build owned modules
dotnet build src/HnVue.UI.Contracts/ src/HnVue.UI.ViewModels/ src/HnVue.App/

# 2. Run integration tests
dotnet test tests.integration/HnVue.IntegrationTests/ --verbosity normal

# 3. Run UI tests (ViewModel binding verification)
dotnet test tests/HnVue.UI.Tests/ --verbosity normal

# 4. Full solution build (catch DI registration gaps)
dotnet build HnVue.sln -c Release

# 5. Architecture tests
dotnet test tests/HnVue.Architecture.Tests/ --verbosity normal
```

## Design Team Handoff Protocol

When Design Team reports `NEEDS_VIEWMODEL: {property list}`:

1. Create interface in UI.Contracts with requested properties
2. Implement ViewModel with CommunityToolkit.Mvvm
3. Register in App.xaml.cs DI
4. Create integration test for the new ViewModel
5. Update DISPATCH status: "VIEWMODEL_READY: {interface name}"
6. Design Team can now bind XAML to the interface properties
