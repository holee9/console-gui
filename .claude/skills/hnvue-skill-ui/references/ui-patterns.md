# UI Design Implementation Quality Guide

Team Design agent reads this file when implementing XAML in Views, Styles, Themes, Components.

## Pre-Implementation Checklist

Before writing any XAML:

1. Confirm PPT slide number — only implement specified pages (Issue #59 compliance)
2. List EXACT files to modify — verify against PPT scope boundary map
3. Check if ViewModel properties exist in UI.Contracts — if missing, report NEEDS_VIEWMODEL
4. Verify theme tokens exist in CoreTokens.xaml/SemanticTokens.xaml — never hardcode colors

## PPT Scope Boundary Map (HARD)

| PPT Slides | View File | Forbidden Elements |
|-----------|-----------|-------------------|
| 1 | LoginView.xaml | No patient list, no thumbnails |
| 2-4 | PatientListView.xaml | No image viewer, no thumbnails |
| 5-7 | StudylistView.xaml | No thumbnails, no acquisition panel |
| 8 | AddPatientProcedureView.xaml | No worklist elements |
| 9-11 | WorkflowView.xaml | Thumbnail strip belongs HERE only |
| 12-13 | MergeView.xaml | No settings elements |
| 14-22 | SettingsView.xaml | No patient data elements |

**After implementation: perform 1:1 visual comparison with PPT source.**

## Design Token Usage (MANDATORY)

### Correct: Using semantic tokens

```xml
<Button Background="{DynamicResource HnVue.Semantic.Button.Primary}"
        Foreground="{DynamicResource HnVue.Semantic.Text.OnPrimary}"
        Padding="{DynamicResource HnVue.Core.Spacing.md}"
        FontSize="{DynamicResource HnVue.Core.FontSize.Base}">
    Login
</Button>
```

### Anti-Pattern: Hardcoded values

```xml
<!-- WRONG — hardcoded colors break theming -->
<Button Background="#1B4F8A" Foreground="White" Padding="16" FontSize="14">
    Login
</Button>
```

### Anti-Pattern: Using Core tokens in Views

```xml
<!-- WRONG — Views should use Semantic tokens, not Core -->
<Button Background="{DynamicResource HnVue.Core.Color.Primary}">
```

### Key Resource Keys Reference

**Status Colors (IEC 62366):**
- `HnVue.Core.Color.StatusSafe` (Safe/Green)
- `HnVue.Core.Color.StatusWarning` (Warning/Yellow)
- `HnVue.Core.Color.StatusBlocked` (Blocked/Orange)
- `HnVue.Core.Color.StatusEmergency` (Emergency/Red)

**Button Styles:**
- `HnVue.Semantic.Button.Primary` — main action
- `HnVue.Semantic.Button.Secondary` — secondary action
- `HnVue.Semantic.Button.Danger` — destructive action
- `HnVue.EmergencyStopButton` — 56px min-height, always enabled

**Text:**
- `HnVue.Core.FontSize.Base` (14px)
- `HnVue.Semantic.Text.Body`, `HnVue.Semantic.Text.Heading`

## MahApps.Metro Integration Patterns

### Correct: Extending MahApps styles

```xml
<Style x:Key="HnVue.LoginButton" TargetType="Button"
       BasedOn="{StaticResource MahApps.Styles.Button.Square.Accent}">
    <Setter Property="Height" Value="44"/>
    <Setter Property="MinWidth" Value="120"/>
    <Setter Property="Margin" Value="{DynamicResource HnVue.Core.Spacing.sm}"/>
</Style>
```

### Anti-Pattern: Ignoring MahApps base styles

```xml
<!-- WRONG — not extending MahApps base -->
<Style x:Key="HnVue.LoginButton" TargetType="Button">
    <!-- Missing BasedOn — loses MahApps theming -->
</Style>
```

## Accessibility Requirements (IEC 62366 / WCAG 2.1 AA)

### Every Interactive Element Must Have:

```xml
<Button AutomationProperties.Name="Login Button"
        AutomationProperties.HelpText="Click to authenticate"
        IsTabStop="True"
        TabIndex="3"
        MinHeight="44" MinWidth="44">
```

### Checklist for Every View:

- [ ] All buttons/inputs have AutomationProperties.Name
- [ ] Tab order (TabIndex) is logical left-to-right, top-to-bottom
- [ ] Color contrast >= 4.5:1 (verify with HighContrast theme)
- [ ] All interactive elements have MinHeight/MinWidth >= 44px
- [ ] Emergency Stop button is visible and always enabled (Acquisition view)
- [ ] No information conveyed by color alone (use icons + text + color)

### Anti-Patterns

- Missing AutomationProperties on interactive controls
- Using only color to indicate state (inaccessible to color-blind users)
- Touch targets smaller than 44x44px
- Non-sequential TabIndex values
- Disabled Emergency Stop button

## DesignTime Mock Pattern

### Correct: DesignTime DataContext

```xml
<UserControl x:Class="HnVue.UI.Views.LoginView"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:dt="clr-namespace:HnVue.UI.DesignTime"
             mc:Ignorable="d"
             d:DataContext="{d:DesignInstance dt:DesignLoginViewModel, IsDesignTimeCreatable=True}"
             d:DesignHeight="800" d:DesignWidth="1280">
```

### Mock Data Requirements

- Korean names: "김영수", "박지현", "이민호" (realistic)
- Patient IDs: "P2026-0001" format
- Dates: recent dates (not 2020)
- Study descriptions: Korean medical terms ("흉부 X선", "복부 CT")

## Code-Behind Quality Gate

### Allowed Code-Behind

```csharp
public partial class LoginView : UserControl
{
    public LoginView(ILoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    // ALLOWED: WPF security constraint (PasswordBox cannot bind)
    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ILoginViewModel vm)
            vm.Password = ((PasswordBox)sender).Password;
    }
}
```

### Forbidden Code-Behind

```csharp
// WRONG — business logic in code-behind
private async void LoginButton_Click(object sender, RoutedEventArgs e)
{
    var result = await _securityService.AuthenticateAsync(username, password); // FORBIDDEN
    if (result.IsSuccess) NavigationService.Navigate(new PatientListView()); // FORBIDDEN
}
```

## Converter Quality Rules

### Design Team Owns (pure visual):

```csharp
public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var flag = value is bool b && b;
        if (Invert) flag = !flag;
        return flag ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException(); // One-way binding only
}
```

### Design Team Does NOT Own (domain logic):

- SafeStateToColorConverter — Team B (depends on SafeState enum)
- AgeFromBirthDateConverter — Team B (depends on patient data model)

## Post-Implementation Verification

```bash
# 1. Build UI module
dotnet build src/HnVue.UI/

# 2. Run UI tests
dotnet test tests/HnVue.UI.Tests/ --verbosity normal

# 3. Architecture tests (verify no forbidden dependencies)
dotnet test tests/HnVue.Architecture.Tests/ --verbosity normal

# 4. Full solution build
dotnet build HnVue.sln -c Release
```

### Visual Verification Checklist

After build succeeds:
- [ ] Each implemented element matches PPT source 1:1
- [ ] No UI elements from other PPT pages leaked in
- [ ] DynamicResource used for all colors/sizes (no hardcoded values)
- [ ] Dark theme renders correctly (switch theme, verify no invisible text)
- [ ] HighContrast theme renders correctly (safety colors visible)
- [ ] DesignTime preview works in VS2022 without running the app
