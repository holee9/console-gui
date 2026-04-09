using HnVue.Security;
using HnVue.Common.Enums;
using Xunit;
using FluentAssertions;

namespace HnVue.Security.Tests;

/// <summary>
/// Tests for JWT key rotation support.
/// Verifies that tokens signed with the previous key remain valid during rotation.
/// </summary>
public sealed class JwtKeyRotationTests
{
    private const string OldKey = "old-secret-key-that-is-at-least-32-characters-long!!!";
    private const string NewKey = "new-secret-key-that-is-at-least-32-characters-long!!!";

    [Fact]
    public void Validate_TokenSignedWithCurrentKey_Succeeds()
    {
        var options = new JwtOptions { SecretKey = NewKey, PreviousSecretKey = OldKey };
        var service = new JwtTokenService(options);
        var (token, _) = service.Issue("user-1", "admin", UserRole.Admin);

        var result = service.Validate(token);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_TokenSignedWithPreviousKey_SucceedsDuringRotation()
    {
        // Issue token with old key
        var oldOptions = new JwtOptions { SecretKey = OldKey };
        var oldService = new JwtTokenService(oldOptions);
        var (token, _) = oldService.Issue("user-1", "admin", UserRole.Admin);

        // Validate with new key service that has previous key configured
        var newOptions = new JwtOptions { SecretKey = NewKey, PreviousSecretKey = OldKey };
        var newService = new JwtTokenService(newOptions);

        var result = newService.Validate(token);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_TokenSignedWithUnknownKey_Fails()
    {
        // Issue token with unknown key
        var unknownOptions = new JwtOptions { SecretKey = "unknown-key-that-is-at-least-32-characters-long!" };
        var unknownService = new JwtTokenService(unknownOptions);
        var (token, _) = unknownService.Issue("user-1", "admin", UserRole.Admin);

        // Validate with new key service
        var newOptions = new JwtOptions { SecretKey = NewKey, PreviousSecretKey = OldKey };
        var newService = new JwtTokenService(newOptions);

        var result = newService.Validate(token);
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Validate_NoPreviousKey_OldTokensFail()
    {
        // Issue token with old key
        var oldOptions = new JwtOptions { SecretKey = OldKey };
        var oldService = new JwtTokenService(oldOptions);
        var (token, _) = oldService.Issue("user-1", "admin", UserRole.Admin);

        // Validate with new key service WITHOUT previous key
        var newOptions = new JwtOptions { SecretKey = NewKey };
        var newService = new JwtTokenService(newOptions);

        var result = newService.Validate(token);
        result.IsFailure.Should().BeTrue();
    }
}
