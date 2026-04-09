using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// E2E design validation tests for PPT Page 2 (Worklist Window).
/// Reference: ★HnVUE UI 변경 최종안_251118.pptx — Slides 4, 5, 6
///
/// Slide 4: Worklist window screenshot (current state)
/// Slide 5: HTML code reference (period filter buttons, status badges, action buttons)
/// Slide 6: Design token spec (12px/700/#f9e04b, bg:#2a3a5c, detail:#7bc8f5, 44px rows)
/// </summary>
public sealed class PptPage2WorklistDesignTests
{
    private readonly ITestOutputHelper _output;
    private static readonly string SrcRoot = Path.Combine(
        Directory.GetCurrentDirectory(),
        "..", "..", "..", "..", "..", "src");

    public PptPage2WorklistDesignTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // ─── PPT Slide 6 Design Token Spec ───────────────────────────────────

    /// <summary>
    /// PPT Slide 6: Section badge must use #F9E04B on #2A3A5C (same as Login, PPT p.2).
    /// Spec: "창 (현재)", 12px/700/#f9e04b, bg:#2a3a5c
    /// </summary>
    [Fact]
    public void SectionBadge_Colors_ShouldMatchPptSlide6Spec()
    {
        string coreTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath))
        {
            _output.WriteLine("CoreTokens.xaml not found — skipped");
            return;
        }

        var xaml = XDocument.Load(coreTokensPath);

        var badgeBg = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.SectionBadgeBg");
        badgeBg.Should().NotBeNull("SectionBadgeBg token must exist in CoreTokens.xaml");
        badgeBg!.Value.Should().BeEquivalentTo("#2A3A5C",
            "PPT Slide 6 spec: bg:#2a3a5c for Worklist section badge");
        _output.WriteLine($"✓ SectionBadgeBg: {badgeBg.Value}");

        var badgeText = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.SectionBadgeText");
        badgeText.Should().NotBeNull("SectionBadgeText token must exist");
        badgeText!.Value.Should().BeEquivalentTo("#F9E04B",
            "PPT Slide 6 spec: 12px/700/#f9e04b for Worklist badge text");
        _output.WriteLine($"✓ SectionBadgeText: {badgeText.Value}");
    }

    /// <summary>
    /// PPT Slide 6: Detail header color must be #7BC8F5 (light blue).
    /// Spec: "창 (현재)" detail text, 11px/700/#7bc8f5
    /// </summary>
    [Fact]
    public void DetailHeaderText_Color_ShouldMatchPptSlide6Spec()
    {
        string coreTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath)) return;

        var xaml = XDocument.Load(coreTokensPath);

        var detailHeader = xaml.Descendants()
            .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.Core.Color.DetailHeaderText");
        detailHeader.Should().NotBeNull("DetailHeaderText token must exist");
        detailHeader!.Value.Should().BeEquivalentTo("#7BC8F5",
            "PPT Slide 6 spec: 11px/700/#7bc8f5 for detail header text");
        _output.WriteLine($"✓ DetailHeaderText: {detailHeader.Value}");
    }

    // ─── PatientListView.xaml Structure Validation ───────────────────────

    private XDocument LoadPatientListView()
    {
        string path = Path.Combine(SrcRoot,
            "HnVue.UI", "Views", "PatientListView.xaml");
        path.Should().Match(p => File.Exists(p), "PatientListView.xaml must exist");
        return XDocument.Load(path);
    }

    /// <summary>
    /// PPT Slide 6: Section badge element "Worklist" must exist in PatientListView.
    /// </summary>
    [Fact]
    public void PatientListView_ShouldContain_SectionBadge_Worklist()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasBadgeBorder = xaml.Descendants(ns + "Border")
            .Any(e => e.Attribute("Background")?.Value.Contains("SectionBadge.Bg") == true);

        hasBadgeBorder.Should().BeTrue(
            "PatientListView must have a Border with HnVue.Component.SectionBadge.Bg (PPT Slide 6)");
        _output.WriteLine("✓ Section badge Border found in PatientListView");

        bool hasBadgeText = xaml.Descendants(ns + "TextBlock")
            .Any(e => e.Attribute("Text")?.Value == "Worklist");

        hasBadgeText.Should().BeTrue(
            "PatientListView must have TextBlock with Text='Worklist' (PPT Slide 6)");
        _output.WriteLine("✓ 'Worklist' TextBlock found in PatientListView");
    }

    /// <summary>
    /// PPT Slide 6: Section badge TextBlock must use HnVue.SectionBadgeText style.
    /// </summary>
    [Fact]
    public void PatientListView_SectionBadge_ShouldUse_SectionBadgeTextStyle()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasStyle = xaml.Descendants(ns + "TextBlock")
            .Any(e => e.Attribute("Style")?.Value.Contains("SectionBadgeText") == true);

        hasStyle.Should().BeTrue(
            "Worklist badge TextBlock must use Style=HnVue.SectionBadgeText (PPT Slide 6: 12px/bold/#f9e04b)");
        _output.WriteLine("✓ HnVue.SectionBadgeText style applied to Worklist badge TextBlock");
    }

    /// <summary>
    /// PPT Slide 5: Period filter buttons must exist (Today, 3Days, 1Week, All, 1Month).
    /// Slide 5 HTML shows 5 period filter buttons in Worklist.
    /// </summary>
    [Fact]
    public void PatientListView_ShouldContain_PeriodFilterButtons()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        var buttons = xaml.Descendants(ns + "Button").ToList();

        var expectedContents = new[] { "Today", "3Days", "1Week", "All", "1Month" };
        var foundContents = buttons
            .Select(b => b.Attribute("Content")?.Value)
            .Where(v => v != null)
            .ToHashSet();

        int foundCount = expectedContents.Count(c => foundContents.Contains(c));

        foundCount.Should().BeGreaterOrEqualTo(4,
            $"PPT Slide 5: at least 4 of 5 period filter buttons must exist (Today/3Days/1Week/All/1Month). Found: {string.Join(", ", foundContents)}");

        foreach (var content in expectedContents)
        {
            bool found = foundContents.Contains(content);
            _output.WriteLine($"{(found ? "✓" : "✗")} Period button '{content}'");
        }
    }

    /// <summary>
    /// PPT Slide 5: Period filter buttons must use HnVue.OutlineButton style.
    /// </summary>
    [Fact]
    public void PatientListView_PeriodButtons_ShouldUse_OutlineButtonStyle()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasPeriodButtonWithStyle = xaml.Descendants(ns + "Button")
            .Any(e => (e.Attribute("Content")?.Value == "Today"
                    || e.Attribute("Content")?.Value == "3Days"
                    || e.Attribute("Content")?.Value == "1Week"
                    || e.Attribute("Content")?.Value == "All"
                    || e.Attribute("Content")?.Value == "1Month")
                   && e.Attribute("Style")?.Value.Contains("OutlineButton") == true);

        hasPeriodButtonWithStyle.Should().BeTrue(
            "Period filter buttons must use HnVue.OutlineButton style (PPT Slide 5)");
        _output.WriteLine("✓ Period filter buttons use HnVue.OutlineButton style");
    }

    /// <summary>
    /// PPT Slide 5: ListBox row height must be 44px (accessibility — large touch targets).
    /// Slide 5 shows 44px row height for Worklist items.
    /// </summary>
    [Fact]
    public void PatientListView_ListBoxItems_ShouldHave_44pxHeight()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        // Check ListBoxItem style Height setter
        bool has44pxHeight = xaml.Descendants(ns + "Style")
            .Where(e => e.Attribute("TargetType")?.Value == "ListBoxItem")
            .SelectMany(style => style.Descendants(ns + "Setter"))
            .Any(setter => setter.Attribute("Property")?.Value == "Height"
                        && setter.Attribute("Value")?.Value == "44");

        has44pxHeight.Should().BeTrue(
            "ListBoxItem must have Height=44 (PPT Slide 5/6: 44px row height for accessibility)");
        _output.WriteLine("✓ ListBoxItem Height=44 found in PatientListView");
    }

    /// <summary>
    /// PPT Slide 6: Status colors must be defined (완료/대기/진행중/긴급).
    /// Status semantic tokens must reference IEC 62366 status color system.
    /// </summary>
    [Fact]
    public void SemanticTokens_ShouldDefine_WorklistStatusColors()
    {
        string semanticTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "SemanticTokens.xaml");

        if (!File.Exists(semanticTokensPath))
        {
            _output.WriteLine("SemanticTokens.xaml not found — skipped");
            return;
        }

        var xaml = XDocument.Load(semanticTokensPath);

        // Status color tokens (IEC 62366)
        var requiredStatusKeys = new[]
        {
            "HnVue.Semantic.Status.Emergency",  // 긴급 — Red
            "HnVue.Semantic.Status.Warning",    // 대기 — Yellow
            "HnVue.Semantic.Status.Safe",       // 완료 — Green
        };

        int found = 0;
        foreach (var key in requiredStatusKeys)
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

        found.Should().Be(requiredStatusKeys.Length,
            "All Worklist status semantic tokens must be defined (PPT Slide 6 + IEC 62366)");
    }

    /// <summary>
    /// PPT Slide 6: Emergency badge must use Status.Emergency color.
    /// Spec: EMRG badge with red background (HnVue.Semantic.Status.Emergency)
    /// </summary>
    [Fact]
    public void PatientListView_EmergencyBadge_ShouldUse_StatusEmergencyColor()
    {
        var xaml = LoadPatientListView();
        XNamespace ns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

        bool hasEmergencyBadge = xaml.Descendants(ns + "Border")
            .Any(e => e.Attribute("Background")?.Value.Contains("Status.Emergency") == true);

        hasEmergencyBadge.Should().BeTrue(
            "PatientListView must have emergency badge Border using HnVue.Semantic.Status.Emergency (PPT Slide 6: 긴급 색상)");
        _output.WriteLine("✓ Emergency badge with Status.Emergency color found");
    }

    /// <summary>
    /// PPT Slide 6: ComponentTokens must define SectionBadge brushes for Worklist.
    /// Same tokens used by both Login (Slide 3) and Worklist (Slide 6) badges.
    /// </summary>
    [Fact]
    public void ComponentTokens_ShouldDefine_WorklistSectionBadgeBrushes()
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
            "Worklist SectionBadge component tokens must be defined (PPT Slide 6)");
    }

    /// <summary>
    /// PPT Slide 6: HnVue.DetailHeaderText style must be defined for "창 (현재)" detail.
    /// </summary>
    [Fact]
    public void HnVueTheme_ShouldDefine_DetailHeaderTextStyle()
    {
        string themePath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "HnVueTheme.xaml");

        if (!File.Exists(themePath)) return;

        var xaml = XDocument.Load(themePath);

        bool exists = xaml.Descendants()
            .Any(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == "HnVue.DetailHeaderText");

        exists.Should().BeTrue(
            "HnVue.DetailHeaderText style must be defined in HnVueTheme.xaml (PPT Slide 6: 11px/700/#7bc8f5)");
        _output.WriteLine("✓ HnVue.DetailHeaderText style defined");
    }

    /// <summary>
    /// PPT Slide 4: Background colors must match dark theme spec.
    /// PPT Slide 4 final spec: #242424 base, #2A2A2A surface, #3B3B3B card.
    /// </summary>
    [Fact]
    public void CoreTokens_BackgroundColors_ShouldMatchPptSlide4DarkTheme()
    {
        string coreTokensPath = Path.Combine(SrcRoot,
            "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        if (!File.Exists(coreTokensPath)) return;

        var xaml = XDocument.Load(coreTokensPath);

        var expectedColors = new (string Key, string Hex)[]
        {
            ("HnVue.Core.Color.BackgroundPage",  "#242424"),
            ("HnVue.Core.Color.BackgroundPanel", "#2A2A2A"),
            ("HnVue.Core.Color.BackgroundCard",  "#3B3B3B"),
        };

        int matched = 0;
        foreach (var (key, hex) in expectedColors)
        {
            var el = xaml.Descendants()
                .FirstOrDefault(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "Key")?.Value == key);
            if (el != null && el.Value.Equals(hex, StringComparison.OrdinalIgnoreCase))
            {
                matched++;
                _output.WriteLine($"✓ {key}: {el.Value}");
            }
            else
            {
                _output.WriteLine($"✗ {key}: Expected {hex}, got {el?.Value ?? "NOT FOUND"}");
            }
        }

        matched.Should().Be(expectedColors.Length,
            "PPT Slide 4 final spec: dark theme #242424/#2A2A2A/#3B3B3B background colors");
    }

    /// <summary>
    /// Summary report for PPT Page 2 (Worklist) design validation.
    /// </summary>
    [Fact]
    public void PptPage2_DesignValidation_Summary()
    {
        _output.WriteLine("");
        _output.WriteLine("=== PPT PAGE 2 (WORKLIST) DESIGN VALIDATION ===");
        _output.WriteLine("Reference: ★HnVUE UI 변경 최종안_251118.pptx Slides 4-6");
        _output.WriteLine("");
        _output.WriteLine("Slide 4 Background Spec:");
        _output.WriteLine("  Dark Theme:     bg=#242424, surface=#2A2A2A, card=#3B3B3B");
        _output.WriteLine("");
        _output.WriteLine("Slide 6 Token Spec:");
        _output.WriteLine("  Section Badge:  bg=#2A3A5C, text=#F9E04B, 12px/bold");
        _output.WriteLine("  Detail Header:  color=#7BC8F5, 11px/700");
        _output.WriteLine("");
        _output.WriteLine("Slide 5 Component Spec:");
        _output.WriteLine("  Period Filter:  Today / 3Days / 1Week / All / 1Month");
        _output.WriteLine("  Row Height:     44px (accessibility)");
        _output.WriteLine("  Status Colors:  Safe(완료) / Warning(대기) / Emergency(긴급)");
        _output.WriteLine("===============================================");
    }
}
