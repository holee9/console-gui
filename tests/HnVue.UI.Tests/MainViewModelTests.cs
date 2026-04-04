using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.UI.ViewModels;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="MainViewModel"/>.
/// </summary>
public sealed class MainViewModelTests
{
    private static MainViewModel CreateSut(ISecurityContext context) => new(context);

    // ── Constructor guard test ───────────────────────────────────────────────

    [Fact]
    public void Constructor_WhenContextIsNull_ThrowsArgumentNullException()
    {
        var act = () => new MainViewModel(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("securityContext");
    }

    // ── RefreshFromContext tests ──────────────────────────────────────────────

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsIsAuthenticatedTrue()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsCurrentUsername()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentUsername.Should().Be("john");
    }

    [Fact]
    public void RefreshFromContext_WhenAuthenticated_SetsCurrentRoleDisplay()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("john");
        context.CurrentRole.Returns(UserRole.Radiologist);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().Be(UserRole.Radiologist.ToString());
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_SetsIsAuthenticatedFalse()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.IsAuthenticated.Should().BeFalse();
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_CurrentUsernameIsNull()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentUsername.Should().BeNull();
    }

    [Fact]
    public void RefreshFromContext_WhenNotAuthenticated_CurrentRoleDisplayIsNull()
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(false);
        context.CurrentUsername.Returns((string?)null);
        context.CurrentRole.Returns((UserRole?)null);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().BeNull();
    }

    [Theory]
    [InlineData(UserRole.Radiographer)]
    [InlineData(UserRole.Radiologist)]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Service)]
    public void RefreshFromContext_AllRoles_SetsRoleDisplayCorrectly(UserRole role)
    {
        var context = Substitute.For<ISecurityContext>();
        context.IsAuthenticated.Returns(true);
        context.CurrentUsername.Returns("user");
        context.CurrentRole.Returns(role);

        var vm = CreateSut(context);
        vm.RefreshFromContext();

        vm.CurrentRoleDisplay.Should().Be(role.ToString());
    }
}
