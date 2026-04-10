using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Security;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Security.Tests;

/// <summary>
/// Async deadlock prevention tests for JwtTokenService (REQ-SEC-002).
/// Verifies that token validation does not block on async operations.
/// </summary>
public sealed class JwtTokenDeadlockTests
{
    private const string TestSecret = "HnVue-Test-Secret-Key-Must-Be-Long-Enough-For-HS256-Algorithm!!";
    private static readonly JwtOptions TestOptions = new()
    {
        SecretKey = TestSecret,
        Issuer = "HnVue-Test",
        Audience = "HnVue-Test-Client",
        ExpiryMinutes = 15
    };

    [Fact]
    public async Task ValidateAsync_WithDenylistCheck_DoesNotDeadlock()
    {
        // Arrange
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtService = new JwtTokenService(TestOptions);

        // Create a valid token
        var (token, jti) = jwtService.Issue("user-1", "testuser", UserRole.Radiologist);

        // Setup denylist to return false (not revoked)
        tokenDenylist.IsRevokedAsync(jti, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        // Act - This should complete without deadlock
        // Run on a different thread to increase chance of deadlock if sync-over-async is used
        var validationTask = Task.Run(() =>
        {
            return jwtService.ValidateAsync(token, tokenDenylist);
        });

        // Add a small delay to ensure the task has started
        await Task.Delay(10);

        // Assert - Should complete successfully without deadlock
        var result = await validationTask;
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_WithRevokedToken_DoesNotDeadlock()
    {
        // Arrange
        var tokenDenylist = Substitute.For<ITokenDenylist>();
        var jwtService = new JwtTokenService(TestOptions);

        // Create a valid token
        var (token, jti) = jwtService.Issue("user-1", "testuser", UserRole.Radiologist);

        // Setup denylist to return true (revoked)
        tokenDenylist.IsRevokedAsync(jti, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        // Act
        var validationTask = Task.Run(() =>
        {
            return jwtService.ValidateAsync(token, tokenDenylist);
        });

        await Task.Delay(10);

        // Assert - Should complete without deadlock and return TokenRevoked error
        var result = await validationTask;
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.TokenRevoked);
    }
}
