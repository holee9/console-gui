using FluentAssertions;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.Common.Tests.Results;

/// <summary>
/// Tests for network and communication error codes (REQ-COMMON-002).
/// Verifies that all required error codes exist and have correct values.
/// </summary>
public sealed class ErrorCodeNetworkTests
{
    [Fact]
    public void NetworkTimeout_ShouldExist()
    {
        // Arrange & Act
        var errorCode = ErrorCode.NetworkTimeout;

        // Assert
        errorCode.Should().Be((ErrorCode)1005);
    }

    [Fact]
    public void CommunicationFailure_ShouldExist()
    {
        // Arrange & Act
        var errorCode = ErrorCode.CommunicationFailure;

        // Assert
        errorCode.Should().Be((ErrorCode)1006);
    }

    [Fact]
    public void HardwareNoResponse_ShouldExist()
    {
        // Arrange & Act
        var errorCode = ErrorCode.HardwareNoResponse;

        // Assert
        errorCode.Should().Be((ErrorCode)1007);
    }

    [Fact]
    public void ConnectionRefused_ShouldExist()
    {
        // Arrange & Act
        var errorCode = ErrorCode.ConnectionRefused;

        // Assert
        errorCode.Should().Be((ErrorCode)1008);
    }

    [Fact]
    public void SslHandshakeFailed_ShouldExist()
    {
        // Arrange & Act
        var errorCode = ErrorCode.SslHandshakeFailed;

        // Assert
        errorCode.Should().Be((ErrorCode)1009);
    }
}
