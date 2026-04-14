using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector;
using HnVue.Detector.ThirdParty;
using HnVue.Detector.ThirdParty.Hme;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Additional coverage tests for Detector module edge cases.
/// Targets state transitions, dispose patterns, and config validation.
/// </summary>
[Trait("SWR", "SWR-DET-010")]
public sealed class DetectorCoverageAdditionalTests
{
    // ── HmeDetectorAdapter: dispose then operations ─────────────────────────────

    [Fact]
    public async Task Hme_DisconnectAsync_WhenNotConnected_StillSucceeds()
    {
        using var sut = new HmeDetectorAdapter(new HmeDetectorConfig("10.0.0.1"));

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task Hme_FullCycleWithGetStatusAtEachStep()
    {
        using var sut = new HmeDetectorAdapter(new HmeDetectorConfig("192.168.1.1"));

        // Initial state
        var status0 = await sut.GetStatusAsync();
        status0.Value.State.Should().Be(DetectorState.Disconnected);
        status0.Value.IsReadyToArm.Should().BeFalse();

        // After connect
        await sut.ConnectAsync();
        var status1 = await sut.GetStatusAsync();
        status1.Value.State.Should().Be(DetectorState.Idle);
        status1.Value.IsReadyToArm.Should().BeTrue();

        // After arm
        await sut.ArmAsync();
        var status2 = await sut.GetStatusAsync();
        status2.Value.State.Should().Be(DetectorState.Armed);
        status2.Value.IsReadyToArm.Should().BeFalse();

        // After abort
        await sut.AbortAsync();
        var status3 = await sut.GetStatusAsync();
        status3.Value.State.Should().Be(DetectorState.Idle);
        status3.Value.IsReadyToArm.Should().BeTrue();
    }

    [Fact]
    public async Task Hme_AbortAsync_FromDisconnected_TransitionsToIdle()
    {
        using var sut = new HmeDetectorAdapter(new HmeDetectorConfig("10.0.0.1"));

        var result = await sut.AbortAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task Hme_GetStatus_ReturnsConfigModelAsSerialNumber()
    {
        var config = new HmeDetectorConfig("10.0.0.1", Model: "S4335-WF");
        using var sut = new HmeDetectorAdapter(config);

        var status = await sut.GetStatusAsync();

        status.Value.SerialNumber.Should().Be("S4335-WF");
    }

    [Fact]
    public void Hme_Dispose_MultipleTimes_DoesNotThrow()
    {
        var sut = new HmeDetectorAdapter(new HmeDetectorConfig("10.0.0.1"));
        sut.Dispose();
        sut.Dispose();
        sut.Dispose();
    }

    [Fact]
    public async Task Hme_CurrentState_AfterDispose_StillReturnsState()
    {
        var sut = new HmeDetectorAdapter(new HmeDetectorConfig("10.0.0.1"));
        await sut.ConnectAsync();
        sut.Dispose();

        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ── VendorAdapterTemplate: additional coverage ──────────────────────────────

    [Fact]
    public async Task Vendor_FullLifecycle_ConnectArmAbortDisconnect()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));

        await sut.ConnectAsync();
        sut.CurrentState.Should().Be(DetectorState.Idle);

        await sut.ArmAsync();
        sut.CurrentState.Should().Be(DetectorState.Armed);

        await sut.AbortAsync();
        sut.CurrentState.Should().Be(DetectorState.Idle);

        await sut.DisconnectAsync();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task Vendor_GetStatus_ReturnsCurrentState()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));

        var status = await sut.GetStatusAsync();

        status.IsSuccess.Should().BeTrue();
        status.Value.State.Should().Be(DetectorState.Disconnected);
        status.Value.IsReadyToArm.Should().BeFalse();
        status.Value.TemperatureCelsius.Should().Be(0.0);
        status.Value.SerialNumber.Should().BeNull();
        status.Value.FirmwareVersion.Should().BeNull();
    }

    [Fact]
    public async Task Vendor_GetStatus_AfterConnect_IsReadyToArm()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));
        await sut.ConnectAsync();

        var status = await sut.GetStatusAsync();

        status.Value.State.Should().Be(DetectorState.Idle);
        status.Value.IsReadyToArm.Should().BeTrue();
    }

    [Fact]
    public async Task Vendor_StateChanged_Events_FireOnTransitions()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.AbortAsync();
        await sut.DisconnectAsync();

        events.Should().HaveCount(4);
        events[0].NewState.Should().Be(DetectorState.Idle);
        events[1].NewState.Should().Be(DetectorState.Armed);
        events[2].NewState.Should().Be(DetectorState.Idle);
        events[3].NewState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task Vendor_SameStateTransition_DoesNotFireEvent()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync();
        await sut.ConnectAsync(); // Same state (Idle)

        events.Should().HaveCount(1);
    }

    [Fact]
    public async Task Vendor_AbortReason_InStateChangedEvent()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.1"));
        await sut.ConnectAsync();
        await sut.ArmAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].Reason.Should().Be("Abort requested");
    }

    [Fact]
    public void Vendor_Dispose_MultipleTimes_DoesNotThrow()
    {
        var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        sut.Dispose();
        sut.Dispose();
    }

    [Fact]
    public void Vendor_Constructor_NullConfig_Throws()
    {
        var act = () => new VendorAdapterTemplate(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public async Task Vendor_ImageAcquired_CanBeSubscribed()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        var received = new List<ImageAcquiredEventArgs>();
        sut.ImageAcquired += (_, e) => received.Add(e);

        // Event handler attached but template does not raise ImageAcquired
        received.Should().BeEmpty();
    }

    // ── DetectorSimulator: additional edge cases ────────────────────────────────

    [Fact]
    public async Task Simulator_ArmAsync_CancelledDuringReadout_ThrowsOperationCanceledException()
    {
        var sim = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 5000,
        };
        await sim.ConnectAsync();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        var act = async () => await sim.ArmAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Simulator_ImageData_Is16BitPerPixel()
    {
        var sim = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 0,
            SimulatedImageWidth = 4,
            SimulatedImageHeight = 4,
        };
        await sim.ConnectAsync();
        RawDetectorImage? image = null;
        sim.ImageAcquired += (_, e) => image = e.Image;

        await sim.ArmAsync();

        image.Should().NotBeNull();
        image!.BitsPerPixel.Should().Be(12);
        image.PixelData.Should().HaveCount(4 * 4 * 2); // 16 pixels * 2 bytes each
    }

    [Fact]
    public async Task Simulator_GetStatus_IncludesSimulatorSerialAndFirmware()
    {
        var sim = new DetectorSimulator();
        await sim.ConnectAsync();

        var status = await sim.GetStatusAsync();

        status.Value.SerialNumber.Should().Be("SIM-001");
        status.Value.FirmwareVersion.Should().Be("1.0.0-sim");
    }

    // ── DetectorConfig: record equality ─────────────────────────────────────────

    [Fact]
    public void DetectorConfig_Equality_WorksCorrectly()
    {
        var config1 = new DetectorConfig("192.168.1.1");
        var config2 = new DetectorConfig("192.168.1.1");
        var config3 = new DetectorConfig("192.168.1.2");

        config1.Should().Be(config2);
        config1.Should().NotBe(config3);
        config1.GetHashCode().Should().Be(config2.GetHashCode());
    }

    [Fact]
    public void DetectorConfig_WithExpression_CreatesModifiedCopy()
    {
        var original = new DetectorConfig("192.168.1.1", Port: 8888);
        var modified = original with { Port = 9999 };

        modified.Port.Should().Be(9999);
        modified.Host.Should().Be("192.168.1.1");
        original.Port.Should().Be(8888);
    }

    [Fact]
    public void HmeDetectorConfig_InheritsBaseConfig()
    {
        var config = new HmeDetectorConfig("10.0.0.1");

        config.Host.Should().Be("10.0.0.1");
        config.Port.Should().Be(8888);
        config.ReadoutTimeoutMs.Should().Be(5000);
        config.ArmTimeoutMs.Should().Be(2000);
        config.ParamFilePath.Should().BeNull();
        config.Model.Should().Be("S4335-WA");
    }

    [Fact]
    public void HmeDetectorConfig_WithCustomValues()
    {
        var config = new HmeDetectorConfig(
            "10.0.0.1", Port: 9999, ReadoutTimeoutMs: 10000,
            ArmTimeoutMs: 5000, ParamFilePath: @"C:\param\S4335.par",
            Model: "S4343-WA");

        config.Port.Should().Be(9999);
        config.ReadoutTimeoutMs.Should().Be(10000);
        config.ParamFilePath.Should().Be(@"C:\param\S4335.par");
        config.Model.Should().Be("S4343-WA");
    }
}
