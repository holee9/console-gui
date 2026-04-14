using FluentAssertions;
using HnVue.Detector.ThirdParty.Hme;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Tests for HmeNativeMethods helper methods (IsSuccess, DescribeError).
/// P/Invoke extern methods cannot be tested without the native DLL.
/// </summary>
[Trait("SWR", "SWR-DT-062")]
public sealed class HmeNativeMethodsTests
{
    [Fact]
    public void IsSuccess_ZeroReturnCode_ReturnsTrue()
    {
        HmeNativeMethods.IsSuccess(0).Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_NonZeroReturnCode_ReturnsFalse()
    {
        HmeNativeMethods.IsSuccess(1).Should().BeFalse();
        HmeNativeMethods.IsSuccess(-1).Should().BeFalse();
        HmeNativeMethods.IsSuccess(42).Should().BeFalse();
        HmeNativeMethods.IsSuccess(int.MaxValue).Should().BeFalse();
        HmeNativeMethods.IsSuccess(int.MinValue).Should().BeFalse();
    }

    [Fact]
    public void DescribeError_ZeroReturnCode_FormatsCorrectly()
    {
        var result = HmeNativeMethods.DescribeError(0);

        result.Should().Contain("0x00000000");
        result.Should().StartWith("HME SDK error code:");
    }

    [Fact]
    public void DescribeError_PositiveReturnCode_FormatsAsHex()
    {
        var result = HmeNativeMethods.DescribeError(256);

        result.Should().Contain("0x00000100");
    }

    [Fact]
    public void DescribeError_NegativeReturnCode_FormatsAsHex()
    {
        var result = HmeNativeMethods.DescribeError(-1);

        result.Should().Contain("0xFFFFFFFF");
    }

    [Fact]
    public void DescribeError_LargeReturnCode_FormatsAsHex()
    {
        var result = HmeNativeMethods.DescribeError(int.MaxValue);

        result.Should().Contain("0x7FFFFFFF");
    }
}
