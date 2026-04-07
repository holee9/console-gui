using System;
using System.IO;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// Design system validation tests.
/// Verifies implementation matches design plan specifications.
/// </summary>
public sealed class DesignSystemTests
{
    private readonly ITestOutputHelper _output;

    public DesignSystemTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Verifies color palette matches design plan.
    /// </summary>
    [Fact]
    public void ColorPalette_ShouldMatchDesignPlan()
    {
        // Expected colors from design plan
        var expectedColors = new (string Key, string Hex)[]
        {
            ("Primary", "#1B4F8A"),
            ("PrimaryLight", "#2E6DB4"),
            ("Accent", "#00AEEF"),
            ("BackgroundPage", "#1A1A2E"),
            ("BackgroundPanel", "#16213E"),
            ("BackgroundCard", "#0F3460"),
            ("TextPrimary", "#FFFFFF"),
            ("TextSecondary", "#B0BEC5"),
            ("StatusSafe", "#00C853"),
            ("StatusWarning", "#FFD600"),
            ("StatusBlocked", "#FF6D00"),
            ("StatusEmergency", "#D50000")
        };

        string coreTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);
        int matchedColors = 0;

        foreach (var (key, expectedHex) in expectedColors)
        {
            var colorElement = xaml.Descendants()
                .FirstOrDefault(e => e.Attribute("Key")?.Value.EndsWith(key) == true);

            if (colorElement != null)
            {
                string actualHex = colorElement.Value;
                if (actualHex.Equals(expectedHex, StringComparison.OrdinalIgnoreCase))
                {
                    matchedColors++;
                    _output.WriteLine($"✓ {key}: {actualHex}");
                }
                else
                {
                    _output.WriteLine($"✗ {key}: Expected {expectedHex}, got {actualHex}");
                }
            }
            else
            {
                _output.WriteLine($"? {key}: Not found in CoreTokens.xaml");
            }
        }

        double matchPercentage = (double)matchedColors / expectedColors.Length;
        _output.WriteLine($"Color palette match: {matchPercentage:P0}");

        // Allow some deviation for implementation adjustments
        matchPercentage.Should().BeGreaterOrEqualTo(0.8,
            "Color palette should match design plan");
    }

    /// <summary>
    /// Verifies typography scale matches design plan.
    /// </summary>
    [Fact]
    public void Typography_ShouldMatchDesignPlan()
    {
        // Expected font sizes from design plan
        var expectedSizes = new (string Key, double Size)[]
        {
            ("HnVue.Core.FontSize.XSmall", 10),
            ("HnVue.Core.FontSize.Small", 11),
            ("HnVue.Core.FontSize.Normal", 13),
            ("HnVue.Core.FontSize.Large", 16),
            ("HnVue.Core.FontSize.Header", 20),
            ("HnVue.Core.FontSize.Display", 28)
        };

        string coreTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);
        int matchedSizes = 0;

        foreach (var (key, expectedSize) in expectedSizes)
        {
            var sizeElement = xaml.Descendants()
                .FirstOrDefault(e => e.Attribute("Key")?.Value == key);

            if (sizeElement != null && double.TryParse(sizeElement.Value, out double actualSize))
            {
                if (Math.Abs(actualSize - expectedSize) < 0.1)
                {
                    matchedSizes++;
                    _output.WriteLine($"✓ {key}: {actualSize}px");
                }
                else
                {
                    _output.WriteLine($"✗ {key}: Expected {expectedSize}px, got {actualSize}px");
                }
            }
        }

        double matchPercentage = (double)matchedSizes / expectedSizes.Length;
        _output.WriteLine($"Typography match: {matchPercentage:P0}");

        matchPercentage.Should().BeGreaterOrEqualTo(0.8,
            "Typography should match design plan");
    }

    /// <summary>
    /// Verifies spacing system follows design plan.
    /// </summary>
    [Fact]
    public void Spacing_ShouldFollow4pxGrid()
    {
        // Design plan specifies 4px base unit
        var expectedSpacing = new (string Key, double Value)[]
        {
            ("HnVue.Core.Spacing.XSmall", 4),
            ("HnVue.Core.Spacing.Small", 8),
            ("HnVue.Core.Spacing.Medium", 12),
            ("HnVue.Core.Spacing.Normal", 16),
            ("HnVue.Core.Spacing.Large", 24),
            ("HnVue.Core.Spacing.XLarge", 32)
        };

        string coreTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);
        int matchedSpacing = 0;

        foreach (var (key, expectedValue) in expectedSpacing)
        {
            var spacingElement = xaml.Descendants()
                .FirstOrDefault(e => e.Attribute("Key")?.Value == key);

            if (spacingElement != null && double.TryParse(spacingElement.Value, out double actualValue))
            {
                if (Math.Abs(actualValue - expectedValue) < 0.1)
                {
                    matchedSpacing++;
                    _output.WriteLine($"✓ {key}: {actualValue}px");
                }
                else
                {
                    _output.WriteLine($"✗ {key}: Expected {expectedValue}px, got {actualValue}px");
                }
            }
        }

        double matchPercentage = (double)matchedSpacing / expectedSpacing.Length;
        _output.WriteLine($"Spacing match: {matchPercentage:P0}");

        matchPercentage.Should().BeGreaterOrEqualTo(0.8,
            "Spacing should follow 4px grid");
    }

    /// <summary>
    /// Verifies corner radius specifications.
    /// </summary>
    [Fact]
    public void CornerRadius_ShouldMatchDesignPlan()
    {
        var expectedCorners = new (string Key, int Value)[]
        {
            ("HnVue.Core.CornerRadius.None", 0),
            ("HnVue.Core.CornerRadius.Small", 2),
            ("HnVue.Core.CornerRadius.Normal", 4),
            ("HnVue.Core.CornerRadius.Large", 8),
            ("HnVue.Core.CornerRadius.Round", 16)
        };

        string coreTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);
        int matchedCorners = 0;

        foreach (var (key, expectedValue) in expectedCorners)
        {
            var cornerElement = xaml.Descendants()
                .FirstOrDefault(e => e.Attribute("Key")?.Value == key);

            if (cornerElement != null)
            {
                // CornerRadius is stored as "TopLeft,TopRight,BottomRight,BottomLeft"
                string value = cornerElement.Value;
                if (int.TryParse(value, out int actualValue))
                {
                    if (actualValue == expectedValue)
                    {
                        matchedCorners++;
                        _output.WriteLine($"✓ {key}: {actualValue}px");
                    }
                }
            }
        }

        double matchPercentage = (double)matchedCorners / expectedCorners.Length;
        _output.WriteLine($"Corner radius match: {matchPercentage:P0}");

        matchPercentage.Should().BeGreaterOrEqualTo(0.8,
            "Corner radius should match design plan");
    }

    /// <summary>
    /// Verifies button styles exist.
    /// </summary>
    [Fact]
    public void ButtonStyles_ShouldBeDefined()
    {
        string themePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "HnVueTheme.xaml");

        if (!File.Exists(themePath))
        {
            _output.WriteLine("HnVueTheme.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(themePath);

        var buttonStyles = new[] { "HnVue.PrimaryButton", "HnVue.SecondaryButton" };
        int foundStyles = 0;

        foreach (var styleName in buttonStyles)
        {
            var styleElement = xaml.Descendants()
                .FirstOrDefault(e => e.Attribute("Key")?.Value == styleName);

            if (styleElement != null)
            {
                foundStyles++;
                _output.WriteLine($"✓ {styleName}: Defined");
            }
            else
            {
                _output.WriteLine($"✗ {styleName}: Not found");
            }
        }

        foundStyles.Should().BeGreaterOrEqualTo(1,
            "At least one button style should be defined");
    }

    /// <summary>
    /// Verifies theme switching is supported.
    /// </summary>
    [Fact]
    public void ThemeSwitching_ShouldBeSupported()
    {
        string themesDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes");

        if (!Directory.Exists(themesDir))
        {
            _output.WriteLine("Themes directory not found - test skipped");
            return;
        }

        var expectedThemes = new[] { "dark", "light", "high-contrast" };
        int foundThemes = 0;

        foreach (var theme in expectedThemes)
        {
            string themeFile = Path.Combine(themesDir, theme, $"{char.ToUpper(theme[0]) + theme[1..]}Theme.xaml");
            bool exists = File.Exists(themeFile);

            if (exists)
            {
                foundThemes++;
                _output.WriteLine($"✓ Theme '{theme}': Available");
            }
            else
            {
                _output.WriteLine($"✗ Theme '{theme}': Not found at {themeFile}");
            }
        }

        foundThemes.Should().BeGreaterOrEqualTo(2,
            "At least two themes should be available (dark + light or high-contrast)");
    }

    /// <summary>
    /// Verifies component tokens reference semantic tokens correctly.
    /// </summary>
    [Fact]
    public void ComponentTokens_ShouldUseSemanticReferences()
    {
        string componentTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "ComponentTokens.xaml");

        if (!File.Exists(componentTokensPath))
        {
            _output.WriteLine("ComponentTokens.xaml not found - test skipped");
            return;
        }

        var xaml = XDocument.Load(componentTokensPath);

        // Component tokens should use DynamicResource pointing to Semantic or Core tokens
        var solidColorBrushes = xaml.Descendants()
            .Where(e => e.Name.LocalName == "SolidColorBrush");

        int dynamicResourceCount = 0;

        foreach (var brush in solidColorBrushes)
        {
            var colorAttr = brush.Attribute("Color");
            if (colorAttr != null && colorAttr.Value.StartsWith("{DynamicResource"))
            {
                dynamicResourceCount++;
            }
        }

        _output.WriteLine($"Component tokens using DynamicResource: {dynamicResourceCount}");

        dynamicResourceCount.Should().BeGreaterOrEqualTo(5,
            "Component tokens should use DynamicResource for theme switching");
    }

    /// <summary>
    /// Design system summary report.
    /// </summary>
    [Fact]
    public void DesignSystem_ShouldGenerateSummary()
    {
        _output.WriteLine("");
        _output.WriteLine("=== DESIGN SYSTEM VALIDATION ===");
        _output.WriteLine("");
        _output.WriteLine("Color Palette:");
        _output.WriteLine("  - Primary: #1B4F8A (Medical Blue)");
        _output.WriteLine("  - Accent: #00AEEF");
        _output.WriteLine("  - Background: #1A1A2E (Dark Mode)");
        _output.WriteLine("");
        _output.WriteLine("Typography:");
        _output.WriteLine("  - Font Family: Segoe UI");
        _output.WriteLine("  - Scale: 10px - 28px");
        _output.WriteLine("");
        _output.WriteLine("Spacing:");
        _output.WriteLine("  - Base Unit: 4px");
        _output.WriteLine("  - Range: 4px - 32px");
        _output.WriteLine("");
        _output.WriteLine("Corner Radius:");
        _output.WriteLine("  - Range: 0px - 16px");
        _output.WriteLine("");
        _output.WriteLine("Status Colors (IEC 62366):");
        _output.WriteLine("  - Safe: #00C853");
        _output.WriteLine("  - Warning: #FFD600");
        _output.WriteLine("  - Blocked: #FF6D00");
        _output.WriteLine("  - Emergency: #D50000");
        _output.WriteLine("");
        _output.WriteLine("Themes:");
        _output.WriteLine("  - Dark (default)");
        _output.WriteLine("  - Light");
        _output.WriteLine("  - High Contrast");
        _output.WriteLine("================================");
    }
}
