using FluentAssertions;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Tests for OwnDetectorConfig record — validates construction, defaults, and immutability.
/// SWR-WF-032: OwnDetectorConfig must provide sane defaults for the 자사 FPD detector.
/// </summary>
[Trait("SWR", "SWR-WF-032")]
public sealed class OwnDetectorConfigTests
{
    // ── Construction with defaults ─────────────────────────────────────────────

    [Fact]
    public void Constructor_WithHostOnly_UsesDefaultPort8888()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.Port.Should().Be(8888);
    }

    [Fact]
    public void Constructor_WithHostOnly_UsesDefaultReadoutTimeout5000()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.ReadoutTimeoutMs.Should().Be(5000);
    }

    [Fact]
    public void Constructor_WithHostOnly_UsesDefaultArmTimeout2000()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.ArmTimeoutMs.Should().Be(2000);
    }

    [Fact]
    public void Constructor_WithHostOnly_CalibrationPathIsNull()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.CalibrationPath.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithHostOnly_UsesDefaultBitsPerPixel14()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.BitsPerPixel.Should().Be(14);
    }

    // ── Construction with explicit values ─────────────────────────────────────

    [Fact]
    public void Constructor_WithAllParameters_StoresAllValues()
    {
        var config = new OwnDetectorConfig(
            Host: "10.0.0.50",
            Port: 9999,
            ReadoutTimeoutMs: 3000,
            ArmTimeoutMs: 1000,
            CalibrationPath: @"C:\HnVue\Calibration\SN12345\",
            BitsPerPixel: 12);

        config.Host.Should().Be("10.0.0.50");
        config.Port.Should().Be(9999);
        config.ReadoutTimeoutMs.Should().Be(3000);
        config.ArmTimeoutMs.Should().Be(1000);
        config.CalibrationPath.Should().Be(@"C:\HnVue\Calibration\SN12345\");
        config.BitsPerPixel.Should().Be(12);
    }

    // ── Record equality ────────────────────────────────────────────────────────

    [Fact]
    public void TwoConfigsWithSameValues_AreEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100", BitsPerPixel: 14);
        var config2 = new OwnDetectorConfig("192.168.1.100", BitsPerPixel: 14);

        config1.Should().Be(config2);
    }

    [Fact]
    public void TwoConfigsWithDifferentHosts_AreNotEqual()
    {
        var config1 = new OwnDetectorConfig("192.168.1.100");
        var config2 = new OwnDetectorConfig("192.168.1.200");

        config1.Should().NotBe(config2);
    }

    // ── Immutability (with expression) ─────────────────────────────────────────

    [Fact]
    public void WithExpression_ProducesNewRecordWithChangedField()
    {
        var original = new OwnDetectorConfig("192.168.1.100");
        var modified = original with { Port = 12345 };

        modified.Port.Should().Be(12345);
        original.Port.Should().Be(8888);
    }

    [Fact]
    public void WithExpression_CalibrationPath_UpdatesOnly()
    {
        var original = new OwnDetectorConfig("192.168.1.100");
        var modified = original with { CalibrationPath = @"C:\Cal\" };

        modified.CalibrationPath.Should().Be(@"C:\Cal\");
        original.CalibrationPath.Should().BeNull();
    }

    // ── Inheritance from DetectorConfig ───────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_IsDetectorConfig()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.Should().BeAssignableTo<DetectorConfig>();
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("192.168.0.1")]
    [InlineData("detector.local")]
    public void Constructor_VariousHosts_StoresHostCorrectly(string host)
    {
        var config = new OwnDetectorConfig(host);

        config.Host.Should().Be(host);
    }
}
