using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.UI.ViewModels;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// Unit tests for <see cref="LoginSuccessEventArgs"/>.
/// </summary>
public sealed class LoginSuccessEventArgsTests
{
    [Fact]
    public void Constructor_SetsToken()
    {
        // Arrange
        var token = new AuthenticationToken(
            UserId: "u1",
            Username: "testuser",
            Role: UserRole.Radiographer,
            Token: "jwt.token.here",
            ExpiresAt: DateTimeOffset.UtcNow.AddHours(1));

        // Act
        var args = new LoginSuccessEventArgs(token);

        // Assert
        args.Token.Should().BeSameAs(token);
    }

    [Fact]
    public void Constructor_WhenTokenIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LoginSuccessEventArgs(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("token");
    }
}
