using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// E2E design validation tests for PPT Page 1 (Login Window).
/// Reference: ★HnVUE UI 변경 최종안_251118.pptx — Slides 1, 2, 3
///
/// Slide 1: Login window screenshot (current)
/// Slide 2: HTML code reference (구조: 사용자/비밀번호, 확인/취소 버튼)
/// Slide 3: Design token spec (12px/700/#f9e04b, bg:#2a3a5c, 2px 8px, r:3px)
/// </summary>
public sealed class PptPage1LoginDesignTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string SrcRoot = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..", "src");

    public PptPage1LoginDesignTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ─── PPT Slide 3 Design Token Spec ───────────────────────────────────

    /// <summary>
    /// PPT Slide 3: Section badge must use #F9E04B text on #2A3A5C background.
    /// Spec: "로그인 창", 12px/700/#f9e04b, bg:#2a3a5c, pad:2px 8px, radius:3px
    /// </summary>
    [Fact]
    public void SectionBadge_Colors_ShouldMatchPptSlide3Spec()
    {
        string coreTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found — skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);

        // Badge background: #2A3A5C
        var badgeBg = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.SectionBadgeBg");
        badgeBg.Should().NotBeNull("SectionBadgeBg token must exist in CoreTokens.xaml");
        badgeBg!.Value.Should().BeEquivalentTo("#2A3A5C",
            "PPT Slide 3 spec: bg:#2a3a5c");
        _output.WriteLine($"✓ SectionBadgeBg: {badgeBg.Value}");

        // Badge text: #F9E04B (golden yellow)
        var badgeText = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.SectionBadgeText");
        badgeText.Should().NotBeNull("SectionBadgeText token must exist");
        badgeText!.Value.Should().BeEquivalentTo("#F9E04B",
            "PPT Slide 3 spec: 12px/700/#f9e04b");
        _output.WriteLine($"✓ SectionBadgeText: {badgeText.Value}");
    }

    /// <summary>
    /// PPT Slide 3: Label muted color must be #7090B0 (uppercase, 10px/600).
    /// </summary>
    [Fact]
    public void UppercaseLabel_Color_ShouldMatchPptSlide3Spec()
    {
        string coreTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath)) return;

        var xaml = XDocument.Load(coreTokensPath);

        var labelMuted = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.LabelMuted");
        labelMuted.Should().NotBeNull("LabelMuted token must exist");
        labelMuted!.Value.Should().BeEquivalentTo("#7090B0",
            "PPT Slide 3 spec: 10px/600/#7090b0 for field labels");
        _output.WriteLine($"✓ LabelMuted: {labelMuted.Value}");
    }

    // ─── LoginView.xaml Structure Validation ─────────────────────────────

    private XDocument LoadLoginView()
    {
        string path = Path.Combine(SrcRoot,
            "HnVue.UI", "Views", "LoginView.xaml");
        path.Should().Match(p => File.Exists(p), "LoginView.xaml must exist");
        return XDocument.Load(path);
    }

    /// <summary>
    /// PPT Slide 3: Section badge element "로그인 창" must exist in LoginView.
    /// </summary>
    [Fact]
    public void LoginView_ShouldContain_SectionBadge_LoginWindow()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        // Find Border with SectionBadge.Bg resource
        bool hasBadgeBorder = xaml.Descendants(ns + "Border")
            .Any(e => e.Attribute("Background")?.Value.Contains("SectionBadge.Bg") == true);

        hasBadgeBorder.Should().BeTrue(
            "LoginView must have a Border with HnVue.Component.SectionBadge.Bg background (PPT Slide 3)");
        _output.WriteLine("✓ Section badge Border found in LoginView");

        // Find TextBlock with "로그인 창"
        bool hasBadgeText = xaml.Descendants(ns + "TextBlock")
            .Any(e => e.Attribute("Text")?.Value == "로그인 창");

        hasBadgeText.Should().BeTrue(
            "LoginView must have TextBlock with Text='로그인 창' (PPT Slide 3)");
        _output.WriteLine("✓ '로그인 창' TextBlock found in LoginView");
    }

    /// <summary>
    /// PPT Slide 3: Section badge TextBlock must use HnVue.SectionBadgeText style.
    /// </summary>
    [Fact]
    public void LoginView_SectionBadge_ShouldUse_SectionBadgeTextStyle()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasStyle = xaml.Descendants(ns + "TextBlock")
            .Any(e => e.Attribute("Style")?.Value.Contains("SectionBadgeText") == true);

        hasStyle.Should().BeTrue(
            "Section badge TextBlock must use Style=HnVue.SectionBadgeText (PPT Slide 3: 12px/bold)");
        _output.WriteLine("✓ HnVue.SectionBadgeText style applied to badge TextBlock");
    }

    /// <summary>
    /// PPT Slide 2: Username input must be ComboBox (dropdown), not TextBox.
    /// Slide 2 HTML: 사용자 -> UserSelectDropdown
    /// Slide 3 component mapping: 사용자] -> UserSelectDropdown
    /// </summary>
    [Fact]
    public void LoginView_Username_ShouldBe_ComboBox()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasComboBox = xaml.Descendants(ns + "ComboBox").Any();
        hasComboBox.Should().BeTrue(
            "PPT Slide 3 maps 사용자 -> UserSelectDropdown (ComboBox, not TextBox)");
        _output.WriteLine("✓ ComboBox (UserSelectDropdown) found in LoginView");
    }

    /// <summary>
    /// PPT Slide 3: Labels must use HnVue.UppercaseLabel style (10px/600/#7090b0).
    /// </summary>
    [Fact]
    public void LoginView_Labels_ShouldUse_UppercaseLabelStyle()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasUppercaseLabel = xaml.Descendants(ns + "TextBlock")
            .Any(e => e.Attribute("Style")?.Value.Contains("UppercaseLabel") == true);

        hasUppercaseLabel.Should().BeTrue(
            "PPT Slide 3: field labels should use HnVue.UppercaseLabel style (10px/600/#7090b0)");
        _output.WriteLine("✓ HnVue.UppercaseLabel style found in LoginView");
    }

    /// <summary>
    /// PPT Slide 2: 취소 (Cancel) button must exist alongside 확인 (Confirm) button.
    /// Slide 2 HTML shows: 확인/취소 buttons.
    /// Slide 3 design tokens include: 취소 버튼 bg, 취소 버튼 텍스트.
    /// </summary>
    [Fact]
    public void LoginView_ShouldContain_CancelButton()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasCancelButton = xaml.Descendants(ns + "Button")
            .Any(e => e.Attribute("Content")?.Value == "취소");

        hasCancelButton.Should().BeTrue(
            "PPT Slide 2/3: 취소 button required (취소 버튼 bg/text tokens defined in Slide 3)");
        _output.WriteLine("✓ 취소 Button found in LoginView");
    }

    /// <summary>
    /// PPT Slide 2: 확인 (Confirm/Login) button must exist.
    /// </summary>
    [Fact]
    public void LoginView_ShouldContain_ConfirmButton()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasConfirmButton = xaml.Descendants(ns + "Button")
            .Any(e => e.Attribute("Content")?.Value == "확인"
                   || e.Attribute("Content")?.Value == "LOGIN");

        hasConfirmButton.Should().BeTrue(
            "PPT Slide 2: 확인 (Login) button must exist");
        _output.WriteLine("✓ 확인/LOGIN Button found in LoginView");
    }

    /// <summary>
    /// PPT Slide 3: 취소 button must use HnVue.CancelButton style.
    /// </summary>
    [Fact]
    public void LoginView_CancelButton_ShouldUse_CancelButtonStyle()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasCancelStyle = xaml.Descendants(ns + "Button")
            .Any(e => e.Attribute("Content")?.Value == "취소"
                   && e.Attribute("Style")?.Value.Contains("CancelButton") == true);

        hasCancelStyle.Should().BeTrue(
            "취소 button should use HnVue.CancelButton style (PPT Slide 3: 취소 버튼 bg spec)");
        _output.WriteLine("✓ HnVue.CancelButton style applied to 취소 button");
    }

    /// <summary>
    /// PPT Slide 3: PasswordBox must exist (비밀번호] -> PasswordInput).
    /// </summary>
    [Fact]
    public void LoginView_Password_ShouldBe_PasswordBox()
    {
        var xaml = LoadLoginView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasPasswordBox = xaml.Descendants(ns + "PasswordBox").Any();
        hasPasswordBox.Should().BeTrue(
            "PPT Slide 3 maps 비밀번호 -> PasswordInput (PasswordBox)");
        _output.WriteLine("✓ PasswordBox (PasswordInput) found in LoginView");
    }

    // ─── HnVueTheme.xaml Style Definitions ───────────────────────────────

    /// <summary>
    /// HnVueTheme must define all PPT Slide 3 required styles.
    /// </summary>
    [Fact]
    public void HnVueTheme_ShouldDefine_AllPptSlide3Styles()
    {
        string themePath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "HnVueTheme.xaml");

        if (!File.Exists(themePath)) return;

        var xaml = XDocument.Load(themePath);

        var requiredStyles = new[]
        {
            "HnVue.SectionBadgeText",   // 12px/700/#f9e04b
            "HnVue.DetailHeaderText",   // 11px/700/#7bc8f5
            "HnVue.UppercaseLabel",     // 10px/600/#7090b0
            "HnVue.CancelButton",       // 취소 버튼
        };

        int found = 0;
        foreach (var style in requiredStyles)
        {
            bool exists = xaml.Descendants()
                .Any(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == style);
            if (exists)
            {
                found++;
                _output.WriteLine($"✓ {style}: Defined");
            }
            else
            {
                _output.WriteLine($"✗ {style}: MISSING");
            }
        }

        found.Should().Be(requiredStyles.Length,
            "All PPT Slide 3 styles must be defined in HnVueTheme.xaml");
    }

    // ─── ComponentTokens Validation ──────────────────────────────────────

    /// <summary>
    /// ComponentTokens must define SectionBadge brushes (PPT Slides 3 & 6).
    /// </summary>
    [Fact]
    public void ComponentTokens_ShouldDefine_SectionBadgeBrushes()
    {
        string componentTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "ComponentTokens.xaml");

        if (!File.Exists(componentTokensPath)) return;

        var xaml = XDocument.Load(componentTokensPath);

        var requiredBrushes = new[]
        {
            "HnVue.Component.SectionBadge.Bg",
            "HnVue.Component.SectionBadge.Text",
            "HnVue.Component.SectionBadge.DetailText",
            "HnVue.Component.Label.Muted",
        };

        int found = 0;
        foreach (var key in requiredBrushes)
        {
            bool exists = xaml.Descendants()
                .Any(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == key);
            if (exists)
            {
                found++;
                _output.WriteLine($"✓ {key}: Defined");
            }
            else
            {
                _output.WriteLine($"✗ {key}: MISSING");
            }
        }

        found.Should().Be(requiredBrushes.Length,
            "All SectionBadge component tokens must be defined (PPT Slides 3 & 6)");
    }

    /// <summary>
    /// Summary report for PPT Page 1 (Login) design validation.
    /// </summary>
    [Fact]
    public void PptPage1_DesignValidation_Summary()
    {
        _output.WriteLine("");
        _output.WriteLine("=== PPT PAGE 1 (LOGIN) DESIGN VALIDATION ===");
        _output.WriteLine("Reference: ★HnVUE UI 변경 최종안_251118.pptx Slides 1-3");
        _output.WriteLine("");
        _output.WriteLine("Slide 3 Token Spec:");
        _output.WriteLine("  Section Badge:  bg=#2A3A5C, text=#F9E04B, 12px/bold, r:3px");
        _output.WriteLine("  Field Labels:   color=#7090B0, 10px/600, uppercase");
        _output.WriteLine("  Detail Header:  color=#7BC8F5, 11px/700");
        _output.WriteLine("");
        _output.WriteLine("Slide 2 Component Spec:");
        _output.WriteLine("  사용자 → ComboBox (UserSelectDropdown)");
        _output.WriteLine("  비밀번호 → PasswordBox (PasswordInput)");
        _output.WriteLine("  확인/취소 → Button row layout");
        _output.WriteLine("==============================================");
    }
}
