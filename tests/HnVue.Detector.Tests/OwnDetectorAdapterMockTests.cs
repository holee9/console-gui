using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Additional tests for OwnDetectorAdapter to increase coverage without requiring actual SDK.
/// Focuses on edge cases, parameter validation, and state transitions.
/// </summary>
[Trait("SWR", "SWR-DET-011")]
[Trait("Coverage", "Boost")]
public sealed class OwnDetectorAdapterMockTests
{
    private readonly OwnDetectorConfig _config = new("192.168.1.100");

    // ── OwnDetectorConfig Edge Cases ────────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_NullHost_ThrowsArgumentNullException()
    {
        var act = () => new OwnDetectorConfig(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("Host");
    }

    [Fact]
    public void OwnDetectorConfig_EmptyHost_AcceptsEmptyString()
    {
        var config = new OwnDetectorConfig("");

        config.Host.Should().Be("");
        config.Port.Should().Be(8888);
    }

    [Fact]
    public void OwnDetectorConfig_DefaultValues_SetsExpectedDefaults()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.Host.Should().Be("192.168.1.100");
        config.Port.Should().Be(8888);
        config.ReadoutTimeoutMs.Should().Be(5000);
        config.ArmTimeoutMs.Should().Be(2000);
        config.BitsPerPixel.Should().Be(14);
        config.CalibrationPath.Should().BeNull();
    }

    [Fact]
    public void OwnDetectorConfig_CustomValues_OverridesDefaults()
    {
        var config = new OwnDetectorConfig(
            Host: "10.0.0.1",
            Port: 9999,
            ReadoutTimeoutMs: 10000,
            ArmTimeoutMs: 5000,
            CalibrationPath: @"C:\Calibration",
            BitsPerPixel: 16
        );

        config.Host.Should().Be("10.0.0.1");
        config.Port.Should().Be(9999);
        config.ReadoutTimeoutMs.Should().Be(10000);
        config.ArmTimeoutMs.Should().Be(5000);
        config.CalibrationPath.Should().Be(@"C:\Calibration");
        config.BitsPerPixel.Should().Be(16);
    }

    // ── Record Equality ─────────────────────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_Equality_SameValues_AreEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100", Port: 8888);
        var config2 = new OwnDetectorConfig("192.168.1.100", Port: 8888);

        config1.Should().Be(config2);
        (config1 == config2).Should().BeTrue();
    }

    [Fact]
    public void OwnDetectorConfig_Equality_DifferentHost_AreNotEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100");
        var config2 = new OwnDetectorConfig("192.168.1.101");

        config1.Should().NotBe(config2);
        (config1 != config2).Should().BeTrue();
    }

    [Fact]
    public void OwnDetectorConfig_Equality_DifferentPort_AreNotEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100", Port: 8888);
        var config2 = new OwnDetectorConfig("192.168.1.100", Port: 9999);

        config1.Should().NotBe(config2);
    }

    [Fact]
    public void OwnDetectorConfig_Equality_DifferentCalibrationPath_AreNotEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100", CalibrationPath: @"C:\Cal1");
        var config2 = new OwnDetectorConfig("192.168.1.100", CalibrationPath: @"C:\Cal2");

        config1.Should().NotBe(config2);
    }

    // ── With Pattern ────────────────────────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_With_CanModifyImmutableProperties()
    {
        var original = new OwnDetectorConfig("192.168.1.100");

        var modified = original with { Host = "10.0.0.1", Port = 9999 };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(9999);
        original.Host.Should().Be("192.168.1.100"); // Original unchanged
    }

    // ── Deconstruct ──────────────────────────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_Deconstruct_ExtractsAllProperties()
    {
        var config = new OwnDetectorConfig(
            Host: "192.168.1.100",
            Port: 9999,
            ReadoutTimeoutMs: 10000,
            ArmTimeoutMs: 5000,
            CalibrationPath: @"C:\Cal",
            BitsPerPixel: 16
        );

        var (host, port, readoutTimeout, armTimeout, calPath, bpp) = config;

        host.Should().Be("192.168.1.100");
        port.Should().Be(9999);
        readoutTimeout.Should().Be(10000);
        armTimeout.Should().Be(5000);
        calPath.Should().Be(@"C:\Cal");
        bpp.Should().Be(16);
    }

    // ── ToString ─────────────────────────────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_ToString_ContainsConfigValues()
    {
        var config = new OwnDetectorConfig("192.168.1.100", Port: 8888);

        var str = config.ToString();

        str.Should().Contain("192.168.1.100");
        str.Should().Contain("8888");
    }
}
