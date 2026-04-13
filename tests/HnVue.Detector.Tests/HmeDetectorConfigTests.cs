using FluentAssertions;
using HnVue.Detector.ThirdParty.Hme;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-DT-061")]
public sealed class HmeDetectorConfigTests
{
    // ── Default values ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var config = new HmeDetectorConfig("192.168.1.100");

        config.Host.Should().Be("192.168.1.100");
        config.Port.Should().Be(8888);
        config.ReadoutTimeoutMs.Should().Be(5000);
        config.ArmTimeoutMs.Should().Be(2000);
        config.ParamFilePath.Should().BeNull();
        config.Model.Should().Be("S4335-WA");
    }

    // ── Custom values ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithCustomValues_SetsCorrectly()
    {
        var config = new HmeDetectorConfig(
            Host: "10.0.0.50",
            Port: 9999,
            ReadoutTimeoutMs: 10000,
            ArmTimeoutMs: 5000,
            ParamFilePath: @"C:\params\S4335-WA.par",
            Model: "S4343-WA");

        config.Host.Should().Be("10.0.0.50");
        config.Port.Should().Be(9999);
        config.ReadoutTimeoutMs.Should().Be(10000);
        config.ArmTimeoutMs.Should().Be(5000);
        config.ParamFilePath.Should().Be(@"C:\params\S4335-WA.par");
        config.Model.Should().Be("S4343-WA");
    }

    [Fact]
    public void Constructor_WithNullParamFilePath_AcceptsNull()
    {
        var config = new HmeDetectorConfig("192.168.1.1", ParamFilePath: null);

        config.ParamFilePath.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyParamFilePath_AcceptsEmpty()
    {
        var config = new HmeDetectorConfig("192.168.1.1", ParamFilePath: "");

        config.ParamFilePath.Should().BeEmpty();
    }

    // ── Record equality ────────────────────────────────────────────────────────

    [Fact]
    public void Equality_SameValues_AreEqual()
    {
        var config1 = new HmeDetectorConfig("192.168.1.100");
        var config2 = new HmeDetectorConfig("192.168.1.100");

        config1.Should().Be(config2);
        config1.GetHashCode().Should().Be(config2.GetHashCode());
    }

    [Fact]
    public void Equality_DifferentHost_AreNotEqual()
    {
        var config1 = new HmeDetectorConfig("192.168.1.100");
        var config2 = new HmeDetectorConfig("192.168.1.200");

        config1.Should().NotBe(config2);
    }

    [Fact]
    public void Equality_DifferentModel_AreNotEqual()
    {
        var config1 = new HmeDetectorConfig("192.168.1.100", Model: "S4335-WA");
        var config2 = new HmeDetectorConfig("192.168.1.100", Model: "S4343-WA");

        config1.Should().NotBe(config2);
    }

    [Fact]
    public void Equality_DifferentParamFile_AreNotEqual()
    {
        var config1 = new HmeDetectorConfig("192.168.1.100", ParamFilePath: "a.par");
        var config2 = new HmeDetectorConfig("192.168.1.100", ParamFilePath: "b.par");

        config1.Should().NotBe(config2);
    }

    // ── Inheritance ────────────────────────────────────────────────────────────

    [Fact]
    public void HmeDetectorConfig_IsDetectorConfig()
    {
        var config = new HmeDetectorConfig("192.168.1.100");

        config.Should().BeAssignableTo<DetectorConfig>();
    }

    [Fact]
    public void HmeDetectorConfig_BasePropertiesMatch()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Port: 7777);

        config.Host.Should().Be("10.0.0.1");
        config.Port.Should().Be(7777);
    }

    // ── Supported models ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("S4335-WA")]
    [InlineData("S4335-WF")]
    [InlineData("S4343-WA")]
    public void Constructor_AcceptsSupportedModels(string model)
    {
        var config = new HmeDetectorConfig("192.168.1.100", Model: model);

        config.Model.Should().Be(model);
    }

    // ── ToString ───────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsHostAndModel()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Model: "S4343-WA");

        var str = config.ToString();

        str.Should().Contain("10.0.0.1");
    }
}
