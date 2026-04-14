using FluentAssertions;
using HnVue.Security;
using Xunit;

namespace HnVue.Security.Tests;

[Trait("SWR", "SWR-SEC-010")]
[Collection("Security-Sequential")]
public sealed class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ValidInput_ReturnsNonEmptyHash()
    {
        var hash = PasswordHasher.HashPassword("Password1");

        hash.Should().NotBeNullOrEmpty();
        hash.Should().StartWith("$2");  // BCrypt identifier
    }

    [Fact]
    public void HashPassword_SameInput_ProducesDifferentHashes()
    {
        // BCrypt generates random salt per call
        var hash1 = PasswordHasher.HashPassword("Password1");
        var hash2 = PasswordHasher.HashPassword("Password1");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void HashPassword_NullInput_ThrowsArgumentNullException()
    {
        var act = () => PasswordHasher.HashPassword(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsSuccess()
    {
        var hash = PasswordHasher.HashPassword("CorrectPass1");

        var result = PasswordHasher.Verify("CorrectPass1", hash);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFailure()
    {
        var hash = PasswordHasher.HashPassword("CorrectPass1");

        var result = PasswordHasher.Verify("WrongPassword", hash);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(HnVue.Common.Results.ErrorCode.AuthenticationFailed);
    }

    [Fact]
    public void Verify_MalformedHash_ReturnsFailure()
    {
        var result = PasswordHasher.Verify("SomePassword", "not-a-valid-hash");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Verify_NullPassword_ThrowsArgumentNullException()
    {
        var act = () => PasswordHasher.Verify(null!, "someHash");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_NullHash_ThrowsArgumentNullException()
    {
        var act = () => PasswordHasher.Verify("Password1", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void NeedsRehash_LowCostHash_ReturnsTrue()
    {
        // BCrypt with cost factor 4 (lowest allowed)
        var lowCostHash = BCrypt.Net.BCrypt.HashPassword("Password1", 4);

        var result = PasswordHasher.NeedsRehash(lowCostHash);

        result.Should().BeTrue();
    }

    [Fact]
    public void NeedsRehash_CurrentCostHash_ReturnsFalse()
    {
        // HashPassword uses cost factor 12 (current)
        var hash = PasswordHasher.HashPassword("Password1");

        var result = PasswordHasher.NeedsRehash(hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void NeedsRehash_MalformedHash_ReturnsTrue()
    {
        var result = PasswordHasher.NeedsRehash("malformed");

        result.Should().BeTrue();
    }
}
