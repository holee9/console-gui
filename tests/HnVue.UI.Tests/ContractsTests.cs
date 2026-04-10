using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.Contracts.Events;
using HnVue.UI.Contracts.Navigation;
using HnVue.UI.Contracts.Theming;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for UI.Contracts types: messages, records, and enums.
/// </summary>
public sealed class ContractsTests
{
    // ── ThemeInfo record tests ───────────────────────────────────────────────

    [Fact]
    public void ThemeInfo_Constructor_SetsProperties()
    {
        var theme = new ThemeInfo("dark", "Dark Theme", true);

        theme.Id.Should().Be("dark");
        theme.DisplayName.Should().Be("Dark Theme");
        theme.IsDark.Should().BeTrue();
    }

    [Fact]
    public void ThemeInfo_LightTheme_IsDarkIsFalse()
    {
        var theme = new ThemeInfo("light", "Light Theme", false);

        theme.IsDark.Should().BeFalse();
    }

    [Fact]
    public void ThemeInfo_Equality_SameValues_AreEqual()
    {
        var a = new ThemeInfo("dark", "Dark Theme", true);
        var b = new ThemeInfo("dark", "Dark Theme", true);

        a.Should().Be(b);
    }

    [Fact]
    public void ThemeInfo_Equality_DifferentId_AreNotEqual()
    {
        var a = new ThemeInfo("dark", "Dark Theme", true);
        var b = new ThemeInfo("light", "Dark Theme", true);

        a.Should().NotBe(b);
    }

    [Fact]
    public void ThemeInfo_ToString_ContainsDisplayName()
    {
        var theme = new ThemeInfo("hc", "High Contrast", false);

        theme.ToString().Should().Contain("High Contrast");
    }

    [Fact]
    public void ThemeInfo_With_CreatesModifiedCopy()
    {
        var original = new ThemeInfo("dark", "Dark", true);
        var modified = original with { DisplayName = "Dark V2" };

        modified.DisplayName.Should().Be("Dark V2");
        modified.Id.Should().Be("dark");
        original.DisplayName.Should().Be("Dark");
    }

    // ── PatientSelectedMessage tests ─────────────────────────────────────────

    [Fact]
    public void PatientSelectedMessage_Constructor_SetsPatientId()
    {
        var msg = new PatientSelectedMessage("P-001");

        msg.Value.Should().Be("P-001");
    }

    [Fact]
    public void PatientSelectedMessage_EmptyId_ValueIsEmpty()
    {
        var msg = new PatientSelectedMessage(string.Empty);

        msg.Value.Should().BeEmpty();
    }

    // ── NavigationRequestedMessage tests ─────────────────────────────────────

    [Fact]
    public void NavigationRequestedMessage_Constructor_SetsToken()
    {
        var msg = new NavigationRequestedMessage(NavigationToken.Workflow);

        msg.Value.Should().Be(NavigationToken.Workflow);
    }

    [Fact]
    public void NavigationRequestedMessage_WithParameter_SetsParameter()
    {
        var param = new object();
        var msg = new NavigationRequestedMessage(NavigationToken.ImageViewer, param);

        msg.Value.Should().Be(NavigationToken.ImageViewer);
        msg.Parameter.Should().BeSameAs(param);
    }

    [Fact]
    public void NavigationRequestedMessage_WithoutParameter_ParameterIsNull()
    {
        var msg = new NavigationRequestedMessage(NavigationToken.PatientList);

        msg.Parameter.Should().BeNull();
    }

    [Theory]
    [InlineData(NavigationToken.Login)]
    [InlineData(NavigationToken.PatientList)]
    [InlineData(NavigationToken.Workflow)]
    [InlineData(NavigationToken.ImageViewer)]
    [InlineData(NavigationToken.DoseDisplay)]
    [InlineData(NavigationToken.CDBurn)]
    [InlineData(NavigationToken.SystemAdmin)]
    [InlineData(NavigationToken.QuickPinLock)]
    [InlineData(NavigationToken.Emergency)]
    [InlineData(NavigationToken.Studylist)]
    [InlineData(NavigationToken.Merge)]
    [InlineData(NavigationToken.Settings)]
    public void NavigationRequestedMessage_AllTokens_PreserveValue(NavigationToken token)
    {
        var msg = new NavigationRequestedMessage(token);

        msg.Value.Should().Be(token);
    }

    // ── SessionTimeoutMessage tests ─────────────────────────────────────────

    [Fact]
    public void SessionTimeoutMessage_Constructor_SetsSecondsRemaining()
    {
        var msg = new SessionTimeoutMessage(120);

        msg.Value.Should().Be(120);
    }

    [Fact]
    public void SessionTimeoutMessage_ZeroSeconds_ValueIsZero()
    {
        var msg = new SessionTimeoutMessage(0);

        msg.Value.Should().Be(0);
    }

    [Fact]
    public void SessionTimeoutMessage_LargeValue_Preserved()
    {
        var msg = new SessionTimeoutMessage(3600);

        msg.Value.Should().Be(3600);
    }

    // ── LoginSuccessEventArgs additional tests ──────────────────────────────

    [Fact]
    public void LoginSuccessEventArgs_Token_PreservesAllFields()
    {
        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        var token = new AuthenticationToken(
            UserId: "u42",
            Username: "testadmin",
            Role: UserRole.Admin,
            Token: "jwt.payload.sig",
            ExpiresAt: expiresAt,
            Jti: "jti-456");

        var args = new LoginSuccessEventArgs(token);

        args.Token.UserId.Should().Be("u42");
        args.Token.Username.Should().Be("testadmin");
        args.Token.Role.Should().Be(UserRole.Admin);
        args.Token.Token.Should().Be("jwt.payload.sig");
        args.Token.ExpiresAt.Should().Be(expiresAt);
        args.Token.Jti.Should().Be("jti-456");
    }

    [Fact]
    public void LoginSuccessEventArgs_InheritsFromEventArgs()
    {
        var token = new AuthenticationToken(
            UserId: "u1",
            Username: "admin",
            Role: UserRole.Admin,
            Token: "jwt",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1),
            Jti: "jti");

        var args = new LoginSuccessEventArgs(token);

        args.Should().BeAssignableTo<EventArgs>();
    }

    // ── NavigationToken enum coverage ────────────────────────────────────────

    [Fact]
    public void NavigationToken_AllValues_AreDefined()
    {
        var values = Enum.GetValues<NavigationToken>();

        // Verify all 12 expected tokens exist
        values.Should().HaveCount(12);
        values.Should().Contain(NavigationToken.Login);
        values.Should().Contain(NavigationToken.PatientList);
        values.Should().Contain(NavigationToken.Workflow);
        values.Should().Contain(NavigationToken.ImageViewer);
        values.Should().Contain(NavigationToken.DoseDisplay);
        values.Should().Contain(NavigationToken.CDBurn);
        values.Should().Contain(NavigationToken.SystemAdmin);
        values.Should().Contain(NavigationToken.QuickPinLock);
        values.Should().Contain(NavigationToken.Emergency);
        values.Should().Contain(NavigationToken.Studylist);
        values.Should().Contain(NavigationToken.Merge);
        values.Should().Contain(NavigationToken.Settings);
    }

    // ── ThemeInfo record edge cases ─────────────────────────────────────────

    [Fact]
    public void ThemeInfo_GetHashCode_SameForEqualRecords()
    {
        var a = new ThemeInfo("hc", "High Contrast", false);
        var b = new ThemeInfo("hc", "High Contrast", false);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ThemeInfo_Deconstruct_WorksCorrectly()
    {
        var theme = new ThemeInfo("dark", "Dark", true);
        var (id, name, isDark) = theme;

        id.Should().Be("dark");
        name.Should().Be("Dark");
        isDark.Should().BeTrue();
    }
}
