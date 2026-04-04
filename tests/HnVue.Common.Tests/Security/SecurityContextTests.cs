using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Enums;
using HnVue.Common.Extensions;
using HnVue.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Common.Tests.Security;

public sealed class SecurityContextTests
{
    private static ISecurityContext CreateContext()
    {
        var services = new ServiceCollection();
        services.AddHnVueCommon();
        return services.BuildServiceProvider().GetRequiredService<ISecurityContext>();
    }

    [Fact]
    public void NewContext_IsNotAuthenticated()
    {
        var context = CreateContext();

        context.IsAuthenticated.Should().BeFalse();
        context.CurrentUserId.Should().BeNull();
        context.CurrentUsername.Should().BeNull();
        context.CurrentRole.Should().BeNull();
    }

    [Fact]
    public void SetCurrentUser_MakesContextAuthenticated()
    {
        var context = CreateContext();
        var user = new AuthenticatedUser("usr-1", "alice", UserRole.Radiographer);

        context.SetCurrentUser(user);

        context.IsAuthenticated.Should().BeTrue();
        context.CurrentUserId.Should().Be("usr-1");
        context.CurrentUsername.Should().Be("alice");
        context.CurrentRole.Should().Be(UserRole.Radiographer);
    }

    [Fact]
    public void ClearCurrentUser_ResetsToUnauthenticated()
    {
        var context = CreateContext();
        context.SetCurrentUser(new AuthenticatedUser("usr-2", "bob", UserRole.Admin));

        context.ClearCurrentUser();

        context.IsAuthenticated.Should().BeFalse();
        context.CurrentUserId.Should().BeNull();
        context.CurrentUsername.Should().BeNull();
        context.CurrentRole.Should().BeNull();
    }

    [Fact]
    public void HasRole_WhenUserHasMatchingRole_ReturnsTrue()
    {
        var context = CreateContext();
        context.SetCurrentUser(new AuthenticatedUser("usr-3", "carol", UserRole.Radiologist));

        context.HasRole(UserRole.Radiologist).Should().BeTrue();
    }

    [Fact]
    public void HasRole_WhenUserHasDifferentRole_ReturnsFalse()
    {
        var context = CreateContext();
        context.SetCurrentUser(new AuthenticatedUser("usr-4", "dave", UserRole.Service));

        context.HasRole(UserRole.Admin).Should().BeFalse();
    }

    [Fact]
    public void HasRole_WhenNotAuthenticated_ReturnsFalse()
    {
        var context = CreateContext();

        context.HasRole(UserRole.Radiographer).Should().BeFalse();
    }

    [Fact]
    public void SetCurrentUser_WithNullArgument_ThrowsArgumentNullException()
    {
        var context = CreateContext();

        var act = () => context.SetCurrentUser(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
