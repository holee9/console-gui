using FluentAssertions;
using HnVue.Detector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Tests for <see cref="DetectorConfig"/> record.
/// Covers default values, equality, and record semantics.
/// </summary>
[Trait("SWR", "SWR-DET-010")]
public sealed class DetectorConfigTests
{
    // ── Default values ───────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithHostOnly_SetsDefaultValues()
    {
        var config = new DetectorConfig("192.168.1.100");

        config.Host.Should().Be("192.168.1.100");
        config.Port.Should().Be(8888);
        config.ReadoutTimeoutMs.Should().Be(5000);
        config.ArmTimeoutMs.Should().Be(2000);
    }

    [Fact]
    public void Constructor_WithAllParameters_SetsCorrectValues()
    {
        var config = new DetectorConfig(
            Host: "10.0.0.1",
            Port: 9000,
            ReadoutTimeoutMs: 3000,
            ArmTimeoutMs: 1500);

        config.Host.Should().Be("10.0.0.1");
        config.Port.Should().Be(9000);
        config.ReadoutTimeoutMs.Should().Be(3000);
        config.ArmTimeoutMs.Should().Be(1500);
    }

    [Fact]
    public void Constructor_NullHost_AcceptsNull()
    {
        var config = new DetectorConfig(null!);

        config.Host.Should().BeNull();
    }

    // ── Record equality ──────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var a = new DetectorConfig("192.168.1.100");
        var b = new DetectorConfig("192.168.1.100");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentHost_AreNotEqual()
    {
        var a = new DetectorConfig("192.168.1.100");
        var b = new DetectorConfig("192.168.1.200");

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentPort_AreNotEqual()
    {
        var a = new DetectorConfig("192.168.1.100", Port: 8888);
        var b = new DetectorConfig("192.168.1.100", Port: 9999);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentReadoutTimeout_AreNotEqual()
    {
        var a = new DetectorConfig("192.168.1.100", ReadoutTimeoutMs: 5000);
        var b = new DetectorConfig("192.168.1.100", ReadoutTimeoutMs: 3000);

        a.Should().NotBe(b);
    }

    [Fact]
    public void Equality_DifferentArmTimeout_AreNotEqual()
    {
        var a = new DetectorConfig("192.168.1.100", ArmTimeoutMs: 2000);
        var b = new DetectorConfig("192.168.1.100", ArmTimeoutMs: 1500);

        a.Should().NotBe(b);
    }

    // ── Record with expression ───────────────────────────────────────────────

    [Fact]
    public void With_ChangingPort_CreatesNewRecord()
    {
        var original = new DetectorConfig("192.168.1.100");
        var modified = original with { Port = 9999 };

        modified.Port.Should().Be(9999);
        modified.Host.Should().Be("192.168.1.100");
        original.Port.Should().Be(8888);
    }

    // ── ToString ─────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsHost()
    {
        var config = new DetectorConfig("10.0.0.5");

        config.ToString().Should().Contain("10.0.0.5");
    }
}
