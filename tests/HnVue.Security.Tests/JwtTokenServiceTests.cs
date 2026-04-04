using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Security;
using Xunit;

namespace HnVue.Security.Tests;

public sealed class JwtTokenServiceTests
{
    private readonly JwtOptions _options = new()
    {
        SecretKey = "TestSecretKey-32CharMinimumForHs256!",
        ExpiryMinutes = 15,
        Issuer = "HnVue",
        Audience = "HnVue",
    };

    private JwtTokenService CreateSut() => new(_options);

    [Fact]
    public void Issue_ReturnsValidJwt()
    {
        var sut = CreateSut();

        var token = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void Issue_TokenContainsUserId()
    {
        var sut = CreateSut();

        var token = sut.Issue("user-42", "alice", UserRole.Admin);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Subject.Should().Be("user-42");
    }

    [Fact]
    public void Issue_TokenContainsUsername()
    {
        var sut = CreateSut();

        var token = sut.Issue("user-1", "bob", UserRole.Radiologist);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var uniqueNameClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.UniqueName);
        uniqueNameClaim.Should().NotBeNull();
        uniqueNameClaim!.Value.Should().Be("bob");
    }

    [Fact]
    public void Issue_TokenExpiresIn15Minutes()
    {
        var sut = CreateSut();
        var before = DateTime.UtcNow;

        var token = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var expiryWindow = jwt.ValidTo - before;
        expiryWindow.Should().BeCloseTo(TimeSpan.FromMinutes(15), precision: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Issue_TokenContainsRoleClaim()
    {
        var sut = CreateSut();

        var token = sut.Issue("user-1", "testuser", UserRole.Service);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var roleClaim = jwt.Claims.FirstOrDefault(c =>
            c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
        roleClaim.Should().NotBeNull();
        roleClaim!.Value.Should().Be("Service");
    }

    [Fact]
    public void Issue_TokenHasCorrectIssuerAndAudience()
    {
        var sut = CreateSut();

        var token = sut.Issue("user-1", "testuser", UserRole.Admin);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Issuer.Should().Be("HnVue");
        jwt.Audiences.Should().Contain("HnVue");
    }

    [Fact]
    public void Validate_ValidToken_ReturnsSuccessWithPrincipal()
    {
        var sut = CreateSut();
        var token = sut.Issue("user-1", "testuser", UserRole.Radiographer);

        var result = sut.Validate(token);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Identity.Should().NotBeNull();
        result.Value.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void Validate_InvalidToken_ReturnsTokenInvalidFailure()
    {
        var sut = CreateSut();

        var result = sut.Validate("this.is.not.a.valid.token");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HnVue.Common.Results.ErrorCode.TokenInvalid);
    }

    [Fact]
    public void Validate_TokenSignedWithDifferentKey_ReturnsTokenInvalidFailure()
    {
        var otherOptions = new JwtOptions
        {
            SecretKey = "OtherSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = 15,
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var otherSut = new JwtTokenService(otherOptions);
        var tokenFromOtherKey = otherSut.Issue("user-1", "testuser", UserRole.Admin);

        var sut = CreateSut();
        var result = sut.Validate(tokenFromOtherKey);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HnVue.Common.Results.ErrorCode.TokenInvalid);
    }

    [Fact]
    public void Validate_ExpiredToken_ReturnsTokenExpiredFailure()
    {
        var expiredOptions = new JwtOptions
        {
            SecretKey = "TestSecretKey-32CharMinimumForHs256!",
            ExpiryMinutes = -1, // Already expired
            Issuer = "HnVue",
            Audience = "HnVue",
        };
        var expiredSut = new JwtTokenService(expiredOptions);
        var expiredToken = expiredSut.Issue("user-1", "testuser", UserRole.Radiographer);

        var sut = CreateSut();
        var result = sut.Validate(expiredToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HnVue.Common.Results.ErrorCode.TokenExpired);
    }
}
