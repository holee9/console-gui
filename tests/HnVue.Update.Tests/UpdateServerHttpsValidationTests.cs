using FluentAssertions;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for update server HTTPS validation (T-010).
/// Verifies that HTTP URLs are rejected and HTTPS URLs are required.
/// </summary>
public sealed class UpdateServerHttpsValidationTests
{
    /// <summary>
    /// GREEN Test: HTTP URL should fail validation.
    /// </summary>
    [Fact]
    public void Validate_HttpUrl_ShouldFail()
    {
        // Arrange
        var options = new UpdateOptions
        {
            UpdateServerUrl = "http://update.hnvue.com/api/v1"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must use HTTPS*");
    }

    /// <summary>
    /// GREEN Test: HTTPS URL should pass validation.
    /// </summary>
    [Fact]
    public void Validate_HttpsUrl_ShouldPass()
    {
        // Arrange
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.hnvue.com/api/v1"
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().NotThrow("HTTPS URL should be valid");
    }

    /// <summary>
    /// GREEN Test: Null/empty URL should fail validation.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrEmptyUrl_ShouldFail(string? url)
    {
        // Arrange
        var options = new UpdateOptions
        {
            UpdateServerUrl = url ?? string.Empty
        };

        // Act
        Action act = () => options.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be null, empty, or whitespace*");
    }

    /// <summary>
    /// GREEN Test: UpdateChecker should validate URL scheme in constructor.
    /// </summary>
    [Fact]
    public void UpdateChecker_Constructor_ValidatesUrlScheme()
    {
        // Arrange
        var options = new UpdateOptions
        {
            UpdateServerUrl = "http://insecure.hnvue.com/api"
        };

        // Act
        Action act = () => new UpdateChecker(options, new System.Net.Http.HttpClient());

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*must use HTTPS*");
    }
}
