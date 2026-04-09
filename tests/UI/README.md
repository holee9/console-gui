# HnVue UI QA Testing Framework

Comprehensive UI testing framework for HnVue medical device Console UI.

## Overview

This testing framework verifies visual consistency, accessibility compliance, performance targets, and Windows DPI scaling for the HnVue UI application.

## Current Status (2026-04-08)

- Reproduced command: `dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --configuration Debug --nologo --no-restore -v minimal`
- Result: **65 total / 50 passed / 15 failed**
- Current failures are concentrated in `DesignSystemTests`, `PptPage1LoginDesignTests`, and `PptPage2WorklistDesignTests`
- Representative mismatches: dark theme background tokens, Login 화면 section badge/button style, Worklist period filter/button row height

## Test Categories

### 1. Visual Regression Tests (`VisualRegressionTests.cs`)

Verifies 95%+ visual consistency across components and screens.

**Tests:**
- Login screen baseline comparison
- Main dashboard baseline comparison
- Workflow screen baseline comparison
- Image viewer baseline comparison
- Cross-DPI consistency (1920x1080, 2560x1440, 3840x2160)

**Screenshot Directories:**
- `Screenshots/Baseline/` - Reference images for comparison
- `Screenshots/Actual/` - Current screenshots for testing
- `Screenshots/Diff/` - Difference images (auto-generated)

**Usage:**
```bash
# Run visual regression tests
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~VisualRegressionTests"

# Update baselines (use with caution)
Copy-Item -Path .\\Screenshots\\Actual\\* -Destination .\\Screenshots\\Baseline\\ -Recurse -Force
```

### 2. Accessibility Tests (`AccessibilityTests.cs`)

WCAG 2.2 AA compliance verification.

**Tests:**
- Primary text contrast (≥4.5:1)
- Secondary text contrast (≥4.5:1)
- Disabled text visibility (≥2.0:1)
- Status color accessibility (≥3.0:1)
- Focus indicator visibility (≥3.0:1)
- Touch target size (≥44x44px)
- Keyboard navigation support
- High contrast theme availability

**Run:**
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~AccessibilityTests"
```

### 3. Performance Tests (`PerformanceTests.cs`)

Load time and memory usage verification.

**Targets:**
- Screen load time: <1000ms
- Search response: <500ms
- Button response: <100ms
- Base memory: <500MB
- Idle CPU: <10%

**Tests:**
- XAML loading efficiency
- Theme resource loading
- Design token modularity
- Component complexity control
- DataGrid virtualization
- GPU-accelerated animations

**Run:**
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PerformanceTests"
```

### 4. Design System Tests (`DesignSystemTests.cs`)

Design system specification validation.

**Tests:**
- Color palette matching
- Typography scale matching
- Spacing system (4px grid)
- Corner radius specifications
- Button style definitions
- Theme switching support
- Component token references

**Run:**
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~DesignSystemTests"
```

### 5. PPT Design Alignment Tests

`PptPage1LoginDesignTests.cs`와 `PptPage2WorklistDesignTests.cs`는 최종 PPT 시안 기준으로
Login/Worklist 화면의 구조, 토큰, 버튼 스타일, 배지, 리스트 행 높이를 검증합니다.

**Run:**
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PptPage1LoginDesignTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PptPage2WorklistDesignTests"
```

## Tools

### ScreenshotCapture (`ScreenshotCapture.cs`)

Utility for capturing and comparing screenshots.

**Methods:**
- `CaptureElement(FrameworkElement)` - Capture WPF element
- `CaptureScreen()` - Capture entire screen
- `CreateDiffImage(Bitmap, Bitmap)` - Generate difference visualization

### QAReportGenerator (`QAReportGenerator.cs`)

Generates HTML and JSON test reports.

**Output:**
- `qa_report_YYYYMMDD_HHMMSS.html` - Interactive HTML report
- `qa_report_YYYYMMDD_HHMMSS.json` - Machine-readable JSON report

**Features:**
- Pass/fail summary
- Test results by category
- Bug tracking integration
- Quality criteria dashboard

## Running Tests

### All Tests
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --configuration Debug --nologo --no-restore -v minimal
```

### Specific Category
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~VisualRegressionTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~AccessibilityTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PerformanceTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~DesignSystemTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PptPage1LoginDesignTests"
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --filter "FullyQualifiedName~PptPage2WorklistDesignTests"
```

### With Coverage
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --collect:"XPlat Code Coverage"
```

### Generate Report
```bash
dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --logger "html;logfilename=testresults.html"
```

## CI/CD Integration

### GitHub Actions Example

```yaml
name: UI QA Tests

on: [push, pull_request]

jobs:
  ui-tests:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      - name: Run UI Tests
        run: dotnet test tests/UI/HnVue.UI.QA.Tests.csproj --logger "github"
        env:
          DOTNET_CLI_TELEMETRY_OPTOUT: true
```

## Quality Gates

### Pass Criteria
- Visual consistency: ≥95%
- WCAG 2.2 AA: All tests pass
- Performance: All targets met
- Design system: ≥80% compliance

### Bug Severity Levels
- **Critical**: Blocks release, safety impact
- **High**: Major functionality broken
- **Medium**: Workaround available
- **Low**: Cosmetic issue

## Dependencies

| Package | Purpose |
|---------|---------|
| `Microsoft.NET.Test.Sdk` | test host |
| `xunit`, `xunit.runner.visualstudio` | test framework |
| `FlaUI.UIA3`, `FlaUI.Core` | UI automation |
| `FluentAssertions` | readable assertions |
| `coverlet.collector` | coverage collection |
| `System.Drawing.Common` | image processing |

패키지 버전은 `Directory.Packages.props`에서 중앙 관리됩니다.

## Extending the Framework

### Adding New Visual Tests

1. Capture baseline screenshot:
```csharp
using var screenshot = ScreenshotCapture.CaptureElement(myElement);
ScreenshotCapture.SaveBitmap(screenshot, "Screenshots/Baseline/new_feature.png");
```

2. Add test method:
```csharp
[Fact]
public void NewFeature_ShouldMatchBaseline()
{
    string baselineFile = GetBaselinePath("new_feature.png");
    string actualFile = GetActualPath("new_feature_actual.png");
    
    // ... test implementation
}
```

### Adding Accessibility Tests

```csharp
[Fact]
public void NewComponent_ShouldMeetContrastRequirements()
{
    var (fgR, fgG, fgB) = ParseHexColor("#ForegroundHex");
    var (bgR, bgG, bgB) = ParseHexColor("#BackgroundHex");
    
    double contrast = GetContrastRatio(fgR, fgG, fgB, bgR, bgG, bgB);
    
    contrast.Should().BeGreaterOrEqualTo(4.5);
}
```

### Adding Performance Tests

```csharp
[Fact]
public void NewScreen_LoadTime_ShouldBeUnder1Second()
{
    var stopwatch = Stopwatch.StartNew();
    
    // Load screen
    // ...
    
    stopwatch.Stop();
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
}
```

## Troubleshooting

### Screenshot Comparison Fails

**Issue**: Pixel-perfect comparison too strict

**Solution**: Adjust tolerance in `CompareImages()`:
```csharp
const int tolerance = 20; // Increase from default 10
```

### Tests Fail on Different DPI

**Issue**: DPI scaling affects screenshots

**Solution**: Set DPI awareness in test app:
```xml
<Application x:Class="HnVue.UI.TestApp"
            xmlns:windows="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <windows:Application.Resources>
        <windows:ResourceDictionary>
            <windows:ResourceDictionary.MergedDictionaries>
                <windows:ResourceDictionary Source="/Themes/HnVueTheme.xaml"/>
            </windows:ResourceDictionary.MergedDictionaries>
        </windows:ResourceDictionary>
    </windows:Application.Resources>
</Application>
```

## References

- [WCAG 2.2 Guidelines](https://www.w3.org/WAI/WCAG22/quickref/)
- [IEC 62366-1](https://webstore.iec.ch/publication/6805)
- [FDA Human Factors](https://www.fda.gov/medical-devices/human-factors-engineering-usability)
- [HnVue Design Plan](../../docs/ui_design_plan_2026.md)
- [HnVue UI Design Master Reference](../../docs/design/UI_DESIGN_MASTER_REFERENCE.md)

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-04-06 | Initial release with visual, accessibility, performance, and design system tests |
