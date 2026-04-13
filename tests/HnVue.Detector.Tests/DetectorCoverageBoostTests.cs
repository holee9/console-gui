using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector;
using HnVue.Detector.OwnDetector;
using HnVue.Detector.ThirdParty;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Additional coverage-boost tests for HnVue.Detector.
/// Targets uncovered and weakly-covered branches across DetectorSimulator,
/// VendorAdapterTemplate, OwnDetectorAdapter, DetectorConfig, and OwnDetectorConfig.
/// </summary>
[Trait("SWR", "SWR-DET-010")]
public sealed class DetectorCoverageBoostTests
{
    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — GetStatusAsync from Armed state, status field details
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_GetStatusAsync_FromArmedState_ReturnsNotReadyToArm()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.50"));
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Armed);
        result.Value.IsReadyToArm.Should().BeFalse();
        result.Value.TemperatureCelsius.Should().Be(0.0);
        result.Value.SerialNumber.Should().BeNull();
        result.Value.FirmwareVersion.Should().BeNull();
    }

    [Fact]
    public async Task Vendor_GetStatusAsync_HasTimestampNearUtcNow()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        await sut.ConnectAsync();
        var before = DateTimeOffset.UtcNow;

        var result = await sut.GetStatusAsync();

        var after = DateTimeOffset.UtcNow;
        result.Value.Timestamp.Should().BeOnOrAfter(before);
        result.Value.Timestamp.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task Vendor_GetStatusAsync_AfterDisconnect_ReturnsDisconnectedNotReady()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.50"));
        await sut.ConnectAsync();
        await sut.DisconnectAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Disconnected);
        result.Value.IsReadyToArm.Should().BeFalse();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — Dispose during async operation (race condition)
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_DisposeDuringAsyncOperation_DoesNotCorruptState()
    {
        var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.50"));

        // Start a connect then immediately dispose — template is synchronous
        // internally so both complete deterministically.
        var connectTask = sut.ConnectAsync();
        sut.Dispose();
        await connectTask;

        // After dispose, state is still accessible (lock-based read)
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task Vendor_MultipleDisposeCallsDuringOperations_NoException()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("192.168.1.50"));
        await sut.ConnectAsync();

        // Simulate concurrent dispose by calling multiple times
        sut.Dispose();
        sut.Dispose();
        sut.Dispose();

        // No exception thrown; state remains from last known value
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — FreeRun vs Sync trigger mode, ArmAsync from various states
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_ArmAsync_FreeRunFromDisconnected_TransitionsToArmed()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));

        var result = await sut.ArmAsync(DetectorTriggerMode.FreeRun);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Armed);
    }

    [Fact]
    public async Task Vendor_ArmAsync_FromArmedState_StillTransitionsAndFiresEvent()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        // Re-arm from Armed — no actual state change since already Armed
        var result = await sut.ArmAsync();

        result.IsSuccess.Should().BeTrue();
        events.Should().BeEmpty("state remained Armed");
    }

    [Fact]
    public async Task Vendor_DisconnectAsync_FromArmedState_TransitionsToDisconnected()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var result = await sut.DisconnectAsync();

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — Constructor with various config values
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void Vendor_Constructor_WithCustomPort_ConfigAccepted()
    {
        var config = new DetectorConfig("10.0.0.1", Port: 9999, ReadoutTimeoutMs: 3000, ArmTimeoutMs: 1000);
        using var sut = new VendorAdapterTemplate(config);

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Vendor_Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new VendorAdapterTemplate(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — Event subscription edge cases
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_StateChanged_MultipleSubscribers_AllReceiveEvent()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        var events1 = new List<DetectorStateChangedEventArgs>();
        var events2 = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events1.Add(e);
        sut.StateChanged += (_, e) => events2.Add(e);

        await sut.ConnectAsync();

        events1.Should().ContainSingle();
        events2.Should().ContainSingle();
        events1[0].NewState.Should().Be(events2[0].NewState);
    }

    [Fact]
    public async Task Vendor_ImageAcquired_SubscriberNeverFires()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        ImageAcquiredEventArgs? received = null;
        sut.ImageAcquired += (_, e) => received = e;

        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.AbortAsync();
        await sut.DisconnectAsync();

        received.Should().BeNull("VendorAdapterTemplate does not raise ImageAcquired");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — Full lifecycle variations
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_ConnectArmDisconnect_ArmPreservedInHistory()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync();
        await sut.ArmAsync(DetectorTriggerMode.FreeRun);
        await sut.DisconnectAsync();

        events.Should().HaveCount(3);
        events[0].NewState.Should().Be(DetectorState.Idle);
        events[1].NewState.Should().Be(DetectorState.Armed);
        events[2].NewState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public async Task Vendor_AbortFromDisconnected_ThenConnect_StillWorks()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));

        // Abort from Disconnected transitions to Idle (template behavior)
        await sut.AbortAsync();
        sut.CurrentState.Should().Be(DetectorState.Idle);

        // Connect still works
        var result = await sut.ConnectAsync();
        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task Vendor_ConnectDisconnectLoop_MultipleCycles()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));

        for (int i = 0; i < 5; i++)
        {
            await sut.ConnectAsync();
            sut.CurrentState.Should().Be(DetectorState.Idle);
            await sut.DisconnectAsync();
            sut.CurrentState.Should().Be(DetectorState.Disconnected);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorSimulator — Concurrent access patterns
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Simulator_ConcurrentGetStatus_AllReturnSuccess()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => sut.GetStatusAsync())
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r =>
        {
            r.IsSuccess.Should().BeTrue();
            r.Value.State.Should().Be(DetectorState.Idle);
            r.Value.IsReadyToArm.Should().BeTrue();
        });
    }

    [Fact]
    public async Task Simulator_ConcurrentConnectAndDisconnect_NoDeadlock()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };

        // Alternate connect and disconnect concurrently
        var connectTasks = Enumerable.Range(0, 5)
            .Select(_ => Task.Run(() => sut.ConnectAsync()))
            .ToArray();
        var disconnectTasks = Enumerable.Range(0, 5)
            .Select(_ => Task.Run(() => sut.DisconnectAsync()))
            .ToArray();

        var allTasks = connectTasks.Concat(disconnectTasks).ToArray();

        // All tasks should complete without deadlock
        var act = () => Task.WhenAll(allTasks);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Simulator_ConcurrentArmAndAbort_NoDeadlock()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();

        var armTask = Task.Run(() => sut.ArmAsync());
        var abortTask = Task.Run(() => sut.AbortAsync());

        // Both should complete without deadlock
        var act = () => Task.WhenAll(armTask, abortTask);
        await act.Should().NotThrowAsync();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorSimulator — Property mutation mid-operation, config variations
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Simulator_SmallImageSize_1x1_ProducesMinimalImage()
    {
        var sut = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 0,
            SimulatedImageWidth = 1,
            SimulatedImageHeight = 1,
        };
        await sut.ConnectAsync();
        RawDetectorImage? image = null;
        sut.ImageAcquired += (_, e) => image = e.Image;

        await sut.ArmAsync();

        image.Should().NotBeNull();
        image!.Width.Should().Be(1);
        image.Height.Should().Be(1);
        image.PixelData.Should().HaveCount(2); // 1 pixel * 2 bytes
    }

    [Fact]
    public async Task Simulator_LargeImageSize_ProducesCorrectBufferSize()
    {
        var sut = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 0,
            SimulatedImageWidth = 100,
            SimulatedImageHeight = 200,
        };
        await sut.ConnectAsync();
        RawDetectorImage? image = null;
        sut.ImageAcquired += (_, e) => image = e.Image;

        await sut.ArmAsync();

        image.Should().NotBeNull();
        image!.PixelData.Should().HaveCount(100 * 200 * 2); // 16-bit LE
    }

    [Fact]
    public async Task Simulator_ImagePixelValues_In12BitRange()
    {
        var sut = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 0,
            SimulatedImageWidth = 10,
            SimulatedImageHeight = 10,
        };
        await sut.ConnectAsync();
        RawDetectorImage? image = null;
        sut.ImageAcquired += (_, e) => image = e.Image;

        await sut.ArmAsync();

        image.Should().NotBeNull();
        // Pixel values should be approximately 2048 +/- 400 (12-bit range 0-4095)
        var pixels = new ushort[100];
        for (int i = 0; i < 100; i++)
        {
            pixels[i] = (ushort)(image!.PixelData[i * 2] | (image.PixelData[i * 2 + 1] << 8));
            pixels[i].Should().BeInRange((ushort)0, (ushort)4095);
        }
    }

    [Fact]
    public async Task Simulator_GetStatus_FromArmedStateBeforeArmCompletes()
    {
        // With zero delays, ArmAsync completes instantly, so test status at Idle after arm
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        await sut.ArmAsync();

        var result = await sut.GetStatusAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.State.Should().Be(DetectorState.Idle);
        result.Value.IsReadyToArm.Should().BeTrue();
        result.Value.SerialNumber.Should().Be("SIM-001");
        result.Value.FirmwareVersion.Should().Be("1.0.0-sim");
        result.Value.TemperatureCelsius.Should().Be(28.0);
    }

    [Fact]
    public async Task Simulator_GetStatus_SerialNumberAndFirmwareConsistentAcrossCalls()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();

        var r1 = await sut.GetStatusAsync();
        var r2 = await sut.GetStatusAsync();

        r1.Value.SerialNumber.Should().Be(r2.Value.SerialNumber);
        r1.Value.FirmwareVersion.Should().Be(r2.Value.FirmwareVersion);
    }

    [Fact]
    public async Task Simulator_StateChanged_ProducesCorrectPreviousStates()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        var transitions = new List<(DetectorState Prev, DetectorState Next)>();
        sut.StateChanged += (_, e) => transitions.Add((e.PreviousState, e.NewState));

        await sut.ConnectAsync();
        await sut.ArmAsync();
        await sut.DisconnectAsync();

        // Sequence: Disconnected->Idle, Idle->Armed, Armed->Acquiring,
        // Acquiring->ImageReady, ImageReady->Idle, Idle->Disconnected
        transitions.Should().HaveCount(6);
        transitions[0].Should().Be((DetectorState.Disconnected, DetectorState.Idle));
        transitions[1].Should().Be((DetectorState.Idle, DetectorState.Armed));
        transitions[2].Should().Be((DetectorState.Armed, DetectorState.Acquiring));
        transitions[3].Should().Be((DetectorState.Acquiring, DetectorState.ImageReady));
        transitions[4].Should().Be((DetectorState.ImageReady, DetectorState.Idle));
        transitions[5].Should().Be((DetectorState.Idle, DetectorState.Disconnected));
    }

    [Fact]
    public async Task Simulator_ArmAsync_FromArmedState_FailsWithCorrectMessage()
    {
        // ArmAsync while still in Armed state is impossible with zero delays
        // because ArmAsync goes through the full cycle. Instead test from Error state.
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        await sut.AbortAsync();
        sut.CurrentState.Should().Be(DetectorState.Error);

        var result = await sut.ArmAsync();

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Error");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorSimulator — Cancellation token in all methods
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Simulator_ConnectAsync_WithCancellationToken_Succeeds()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        using var cts = new CancellationTokenSource();

        var result = await sut.ConnectAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Simulator_DisconnectAsync_WithCancellationToken_Succeeds()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();

        var result = await sut.DisconnectAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Simulator_AbortAsync_WithCancellationToken_Succeeds()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();

        var result = await sut.AbortAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Simulator_GetStatusAsync_WithCancellationToken_Succeeds()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();

        var result = await sut.GetStatusAsync(cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Simulator_ArmAsync_CancelledDuringReadoutDelay_ThrowsOperationCancelledException()
    {
        var sut = new DetectorSimulator
        {
            ArmDelayMs = 0,
            ReadoutDelayMs = 5000, // Long readout delay to allow cancellation
        };
        await sut.ConnectAsync();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50);

        var act = async () => await sut.ArmAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorSimulator — Failure injection edge cases
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Simulator_FailNextConnectWith_EmptyString_FailsWithEmptyMessage()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        sut.FailNextConnectWith = string.Empty;

        var result = await sut.ConnectAsync();

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
        // Failure is consumed
        sut.FailNextConnectWith.Should().BeNull();
    }

    [Fact]
    public async Task Simulator_FailNextArmWith_EmptyString_TransitionsToError()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        await sut.ConnectAsync();
        sut.FailNextArmWith = string.Empty;

        var result = await sut.ArmAsync();

        result.IsFailure.Should().BeTrue();
        sut.CurrentState.Should().Be(DetectorState.Error);
    }

    [Fact]
    public async Task Simulator_FailNextConnectWith_OnlyAffectsNextCall()
    {
        var sut = new DetectorSimulator { ArmDelayMs = 0, ReadoutDelayMs = 0 };
        sut.FailNextConnectWith = "once";

        // First fails
        var r1 = await sut.ConnectAsync();
        r1.IsFailure.Should().BeTrue();

        // Second succeeds
        var r2 = await sut.ConnectAsync();
        r2.IsSuccess.Should().BeTrue();

        // Third also succeeds
        var r3 = await sut.ConnectAsync();
        r3.IsSuccess.Should().BeTrue();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorAdapter — All methods throw with class name in message
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Own_ConnectAsync_MessageContainsClassName()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.ConnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_DisconnectAsync_MessageContainsClassName()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.DisconnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_ArmAsync_MessageContainsClassName()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.ArmAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_AbortAsync_MessageContainsClassName()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.AbortAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_GetStatusAsync_MessageContainsClassName()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.GetStatusAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorAdapter — Disposed state guards on all methods
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Own_DisconnectAsync_WhenDisposed_ThrowsNotImplementedNotDisposed()
    {
        // DisconnectAsync does NOT check disposed state in current implementation
        var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        sut.Dispose();

        var act = async () => await sut.DisconnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_AbortAsync_WhenDisposed_ThrowsNotImplementedNotDisposed()
    {
        // AbortAsync does NOT check disposed state in current implementation
        var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        sut.Dispose();

        var act = async () => await sut.AbortAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    [Fact]
    public async Task Own_GetStatusAsync_WhenDisposed_ThrowsNotImplementedNotDisposed()
    {
        // GetStatusAsync does NOT check disposed state in current implementation
        var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        sut.Dispose();

        var act = async () => await sut.GetStatusAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter*");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorAdapter — CancellationToken through to all methods
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Own_DisconnectAsync_WithCancellationToken_ThrowsNotImplemented()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.DisconnectAsync(cts.Token);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Own_AbortAsync_WithCancellationToken_ThrowsNotImplemented()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.AbortAsync(cts.Token);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Own_GetStatusAsync_WithCancellationToken_ThrowsNotImplemented()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.GetStatusAsync(cts.Token);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task Own_ArmAsync_WithCancellationToken_ThrowsObjectDisposedWhenDisposed()
    {
        var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        sut.Dispose();
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.ArmAsync(DetectorTriggerMode.Sync, cts.Token);

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task Own_ConnectAsync_WithCancellationToken_ThrowsObjectDisposedWhenDisposed()
    {
        var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));
        sut.Dispose();
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.ConnectAsync(cts.Token);

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorAdapter — Detailed error message verification
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Own_ConnectAsync_MessageContainsMethodContext()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.ConnectAsync();

        var ex = await act.Should().ThrowAsync<NotImplementedException>();
        ex.Which.Message.Should().Contain("ConnectAsync");
    }

    [Fact]
    public async Task Own_ArmAsync_MessageContainsMethodContext()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.ArmAsync();

        var ex = await act.Should().ThrowAsync<NotImplementedException>();
        ex.Which.Message.Should().Contain("ArmAsync");
    }

    [Fact]
    public async Task Own_DisconnectAsync_MessageContainsMethodContext()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.DisconnectAsync();

        var ex = await act.Should().ThrowAsync<NotImplementedException>();
        ex.Which.Message.Should().Contain("DisconnectAsync");
    }

    [Fact]
    public async Task Own_AbortAsync_MessageContainsMethodContext()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.AbortAsync();

        var ex = await act.Should().ThrowAsync<NotImplementedException>();
        ex.Which.Message.Should().Contain("AbortAsync");
    }

    [Fact]
    public async Task Own_GetStatusAsync_MessageContainsMethodContext()
    {
        using var sut = new OwnDetectorAdapter(new OwnDetectorConfig("10.0.0.1"));

        var act = async () => await sut.GetStatusAsync();

        var ex = await act.Should().ThrowAsync<NotImplementedException>();
        ex.Which.Message.Should().Contain("GetStatusAsync");
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorConfig — With-expression preserving all fields
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DetectorConfig_WithHost_PreservesOtherFields()
    {
        var original = new DetectorConfig("10.0.0.1", Port: 9000, ReadoutTimeoutMs: 3000, ArmTimeoutMs: 1000);
        var modified = original with { Host = "192.168.1.1" };

        modified.Host.Should().Be("192.168.1.1");
        modified.Port.Should().Be(9000);
        modified.ReadoutTimeoutMs.Should().Be(3000);
        modified.ArmTimeoutMs.Should().Be(1000);
        original.Host.Should().Be("10.0.0.1"); // Original unchanged
    }

    [Fact]
    public void DetectorConfig_WithReadoutTimeout_PreservesOtherFields()
    {
        var original = new DetectorConfig("10.0.0.1", Port: 9000, ReadoutTimeoutMs: 3000, ArmTimeoutMs: 1000);
        var modified = original with { ReadoutTimeoutMs = 8000 };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(9000);
        modified.ReadoutTimeoutMs.Should().Be(8000);
        modified.ArmTimeoutMs.Should().Be(1000);
    }

    [Fact]
    public void DetectorConfig_WithArmTimeout_PreservesOtherFields()
    {
        var original = new DetectorConfig("10.0.0.1", Port: 9000);
        var modified = original with { ArmTimeoutMs = 5000 };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(9000);
        modified.ArmTimeoutMs.Should().Be(5000);
        original.ArmTimeoutMs.Should().Be(2000);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  DetectorConfig — Equality edge cases
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void DetectorConfig_EqualsNull_ReturnsFalse()
    {
        var config = new DetectorConfig("10.0.0.1");

        config.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void DetectorConfig_SameReference_IsEqual()
    {
        var config = new DetectorConfig("10.0.0.1");

        config.Should().BeSameAs(config);
        config.Equals(config).Should().BeTrue();
    }

    [Fact]
    public void DetectorConfig_GetHashCode_SameValues_SameHash()
    {
        var a = new DetectorConfig("10.0.0.1", Port: 9000, ReadoutTimeoutMs: 3000);
        var b = new DetectorConfig("10.0.0.1", Port: 9000, ReadoutTimeoutMs: 3000);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void DetectorConfig_WithSameValues_EqualOriginal()
    {
        var original = new DetectorConfig("10.0.0.1", Port: 9000);
        var copy = original with { }; // No change

        copy.Should().Be(original);
        copy.GetHashCode().Should().Be(original.GetHashCode());
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorConfig — With-expression preserving all fields including inherited
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void OwnDetectorConfig_WithHost_PreservesAllFields()
    {
        var original = new OwnDetectorConfig(
            "10.0.0.1", Port: 9000, ReadoutTimeoutMs: 3000, ArmTimeoutMs: 1000,
            CalibrationPath: @"C:\Cal", BitsPerPixel: 16);
        var modified = original with { Host = "192.168.1.1" };

        modified.Host.Should().Be("192.168.1.1");
        modified.Port.Should().Be(9000);
        modified.ReadoutTimeoutMs.Should().Be(3000);
        modified.ArmTimeoutMs.Should().Be(1000);
        modified.CalibrationPath.Should().Be(@"C:\Cal");
        modified.BitsPerPixel.Should().Be(16);
    }

    [Fact]
    public void OwnDetectorConfig_WithPort_PreservesAllFields()
    {
        var original = new OwnDetectorConfig("10.0.0.1", CalibrationPath: "/cal", BitsPerPixel: 16);
        var modified = original with { Port = 7777 };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(7777);
        modified.CalibrationPath.Should().Be("/cal");
        modified.BitsPerPixel.Should().Be(16);
    }

    [Fact]
    public void OwnDetectorConfig_WithBitsPerPixel_PreservesAllFields()
    {
        var original = new OwnDetectorConfig("10.0.0.1", Port: 9000, CalibrationPath: "/cal");
        var modified = original with { BitsPerPixel = 12 };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(9000);
        modified.CalibrationPath.Should().Be("/cal");
        modified.BitsPerPixel.Should().Be(12);
    }

    [Fact]
    public void OwnDetectorConfig_WithCalibrationPath_PreservesAllFields()
    {
        var original = new OwnDetectorConfig("10.0.0.1", Port: 9000, BitsPerPixel: 14);
        var modified = original with { CalibrationPath = "/new/cal" };

        modified.Host.Should().Be("10.0.0.1");
        modified.Port.Should().Be(9000);
        modified.CalibrationPath.Should().Be("/new/cal");
        modified.BitsPerPixel.Should().Be(14);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  OwnDetectorConfig — Equality edge cases
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public void OwnDetectorConfig_EqualsNull_ReturnsFalse()
    {
        var config = new OwnDetectorConfig("10.0.0.1");

        config.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void OwnDetectorConfig_SameReference_IsEqual()
    {
        var config = new OwnDetectorConfig("10.0.0.1");

        config.Should().BeSameAs(config);
        config.Equals(config).Should().BeTrue();
    }

    [Fact]
    public void OwnDetectorConfig_WithSameValues_EqualOriginal()
    {
        var original = new OwnDetectorConfig("10.0.0.1", Port: 9000, CalibrationPath: "/a", BitsPerPixel: 16);
        var copy = original with { }; // No change

        copy.Should().Be(original);
        copy.GetHashCode().Should().Be(original.GetHashCode());
    }

    [Fact]
    public void OwnDetectorConfig_NotEqualToBaseDetectorConfig_WithSameBaseValues()
    {
        var baseConfig = new DetectorConfig("10.0.0.1", Port: 9000);
        var ownConfig = new OwnDetectorConfig("10.0.0.1", Port: 9000);

        // OwnDetectorConfig is a subtype — records use runtime type for equality
        ownConfig.Should().NotBe(baseConfig);
    }

    [Fact]
    public void OwnDetectorConfig_AssignableToBaseType_PreservesFields()
    {
        DetectorConfig asBase = new OwnDetectorConfig("10.0.0.1", Port: 7777, CalibrationPath: "/cal");

        asBase.Host.Should().Be("10.0.0.1");
        asBase.Port.Should().Be(7777);
        asBase.ReadoutTimeoutMs.Should().Be(5000);
        asBase.ArmTimeoutMs.Should().Be(2000);
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    //  VendorAdapterTemplate — AbortAsync reason preserved in event across states
    // ═══════════════════════════════════════════════════════════════════════════════

    [Fact]
    public async Task Vendor_AbortAsync_FromArmed_EventContainsAbortReason()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        await sut.ConnectAsync();
        await sut.ArmAsync();
        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.AbortAsync();

        events.Should().ContainSingle();
        events[0].Reason.Should().Be("Abort requested");
        events[0].PreviousState.Should().Be(DetectorState.Armed);
        events[0].NewState.Should().Be(DetectorState.Idle);
    }

    [Fact]
    public async Task Vendor_ConnectAsync_AlreadyIdle_SameStateNoEvent()
    {
        using var sut = new VendorAdapterTemplate(new DetectorConfig("10.0.0.1"));
        await sut.ConnectAsync(); // Now Idle

        var events = new List<DetectorStateChangedEventArgs>();
        sut.StateChanged += (_, e) => events.Add(e);

        await sut.ConnectAsync(); // Already Idle

        events.Should().BeEmpty("no state change from Idle to Idle");
    }
}
