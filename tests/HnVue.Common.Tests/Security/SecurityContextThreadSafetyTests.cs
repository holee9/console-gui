using System.Collections.Concurrent;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Extensions;
using HnVue.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HnVue.Common.Tests.Security;

/// <summary>
/// Thread safety tests for ISecurityContext (REQ-COMMON-001).
/// Verifies that concurrent SetCurrentUser and ClearCurrentUser operations
/// do not cause data races or inconsistent state.
/// </summary>
public sealed class SecurityContextThreadSafetyTests
{
    private static ISecurityContext CreateContext()
    {
        var services = new ServiceCollection();
        services.AddHnVueCommon();
        return services.BuildServiceProvider().GetRequiredService<ISecurityContext>();
    }

    [Fact]
    public async Task Concurrent_SetAndClear_ShouldNotCorruptState()
    {
        // Arrange
        var context = CreateContext();
        var user1 = new AuthenticatedUser("user1", "User One", UserRole.Radiologist, "jti1");
        var user2 = new AuthenticatedUser("user2", "User Two", UserRole.Admin, "jti2");
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Run 100 concurrent operations
        var tasks = Enumerable.Range(0, 100).Select(i =>
            Task.Run(() =>
            {
                try
                {
                    if (i % 2 == 0)
                    {
                        context.SetCurrentUser(user1);
                        // Small delay to increase chance of race
                        Thread.Sleep(1);
                        context.ClearCurrentUser();
                    }
                    else
                    {
                        context.SetCurrentUser(user2);
                        Thread.Sleep(1);
                        context.ClearCurrentUser();
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            })
        );

        await Task.WhenAll(tasks);

        // Assert - No exceptions should have been thrown
        exceptions.Should().BeEmpty();

        // Final state should be unauthenticated
        context.IsAuthenticated.Should().BeFalse();
        context.CurrentUserId.Should().BeNull();
    }

    [Fact]
    public async Task Concurrent_SetMultipleUsers_ShouldEventuallySettle()
    {
        // Arrange
        var context = CreateContext();
        var users = Enumerable.Range(0, 10)
            .Select(i => new AuthenticatedUser($"user{i}", $"User {i}", UserRole.Radiologist, $"jti{i}"))
            .ToArray();

        // Act - Set all users concurrently
        var tasks = users.Select(user =>
            Task.Run(() => context.SetCurrentUser(user))
        );

        await Task.WhenAll(tasks);

        // Assert - Should eventually settle to some user (no crash or corruption)
        context.IsAuthenticated.Should().BeTrue();
        context.CurrentUserId.Should().NotBeNullOrEmpty();

        // The settled user should be one of the users we set
        users.Any(u => u.UserId == context.CurrentUserId).Should().BeTrue();
    }

    [Fact]
    public async Task Concurrent_ReadsDuringWrites_ShouldNotThrow()
    {
        // Arrange
        var context = CreateContext();
        var user = new AuthenticatedUser("user1", "User One", UserRole.Radiologist, "jti1");
        var exceptions = new ConcurrentBag<Exception>();

        // Act - Mix reads and writes
        var tasks = Enumerable.Range(0, 50).Select(i =>
            Task.Run(() =>
            {
                try
                {
                    if (i % 3 == 0)
                    {
                        context.SetCurrentUser(user);
                    }
                    else if (i % 3 == 1)
                    {
                        context.ClearCurrentUser();
                    }
                    else
                    {
                        // Read operations
                        _ = context.IsAuthenticated;
                        _ = context.CurrentUserId;
                        _ = context.CurrentUsername;
                        _ = context.CurrentRole;
                        _ = context.HasRole(UserRole.Radiologist);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            })
        );

        await Task.WhenAll(tasks);

        // Assert - No exceptions should have been thrown
        exceptions.Should().BeEmpty();
    }
}
