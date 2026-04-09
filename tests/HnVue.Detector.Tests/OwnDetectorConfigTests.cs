using FluentAssertions;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-DET-010")]
public sealed class OwnDetectorConfigTests
{
    // ── Constructor validation ───────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullHost_CreatesConfigWithNullHost()
    {
        // Records do not validate constructor parameters — null Host is accepted
        var config = new OwnDetectorConfig(null!);

        config.Host.Should().BeNull();
    }

    [Fact]
    public void Constructor_EmptyHost_CreatesConfigWithEmptyHost()
    {
        var config = new OwnDetectorConfig(string.Empty);

        config.Host.Should().BeEmpty();
    }

    // ── Property values ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithHostOnly_SetsDefaultValues()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.Host.Should().Be("192.168.1.100");
        config.Port.Should().Be(8888);
        config.ReadoutTimeoutMs.Should().Be(5000);
        config.ArmTimeoutMs.Should().Be(2000);
        config.CalibrationPath.Should().BeNull();
        config.BitsPerPixel.Should().Be(14);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsCorrectValues()
    {
        var config = new OwnDetectorConfig(
            Host: "10.0.0.1",
            Port: 9000,
            ReadoutTimeoutMs: 3000,
            ArmTimeoutMs: 1500,
            CalibrationPath: @"C:\HnVue\Calibration\SN001",
            BitsPerPixel: 16);

        config.Host.Should().Be("10.0.0.1");
        config.Port.Should().Be(9000);
        config.ReadoutTimeoutMs.Should().Be(3000);
        config.ArmTimeoutMs.Should().Be(1500);
        config.CalibrationPath.Should().Be(@"C:\HnVue\Calibration\SN001");
        config.BitsPerPixel.Should().Be(16);
    }

    [Fact]
    public void Constructor_CustomPort_SetsCorrectPort()
    {
        var config = new OwnDetectorConfig("192.168.1.100", Port: 7777);

        config.Port.Should().Be(7777);
    }

    [Fact]
    public void Constructor_WithCalibrationPath_SetsPath()
    {
        var config = new OwnDetectorConfig("192.168.1.100", CalibrationPath: "/data/cal");

        config.CalibrationPath.Should().Be("/data/cal");
    }

    // ── Record equality ──────────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new OwnDetectorConfig("192.168.1.100");
        var b = new OwnDetectorConfig("192.168.1.100");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentHost_AreNotEqual()
    {
        var a = new OwnDetectorConfig("192.168.1.100");
        var b = new OwnDetectorConfig("192.168.1.101");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentPort_AreNotEqual()
    {
        var a = new OwnDetectorConfig("192.168.1.100", Port: 8888);
        var b = new OwnDetectorConfig("192.168.1.100", Port: 9999);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentCalibrationPath_AreNotEqual()
    {
        var a = new OwnDetectorConfig("192.168.1.100", CalibrationPath: "/a");
        var b = new OwnDetectorConfig("192.168.1.100", CalibrationPath: "/b");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentBitsPerPixel_AreNotEqual()
    {
        var a = new OwnDetectorConfig("192.168.1.100", BitsPerPixel: 14);
        var b = new OwnDetectorConfig("192.168.1.100", BitsPerPixel: 16);

        a.Should().NotBe(b);
    }

    // ── Inheritance from DetectorConfig ──────────────────────────────────────────

    [Fact]
    public void OwnDetectorConfig_IsSubclassOf_DetectorConfig()
    {
        typeof(OwnDetectorConfig).IsSubclassOf(typeof(DetectorConfig))
            .Should().BeTrue();
    }

    [Fact]
    public void OwnDetectorConfig_CanBeAssignedTo_DetectorConfig()
    {
        DetectorConfig baseConfig = new OwnDetectorConfig("192.168.1.100");

        baseConfig.Host.Should().Be("192.168.1.100");
        baseConfig.Port.Should().Be(8888);
    }

    // ── Default BitsPerPixel for CsI panel ───────────────────────────────────────

    [Fact]
    public void DefaultBitsPerPixel_Is14_ForCsiPanel()
    {
        var config = new OwnDetectorConfig("192.168.1.100");

        config.BitsPerPixel.Should().Be(14);
    }
}
