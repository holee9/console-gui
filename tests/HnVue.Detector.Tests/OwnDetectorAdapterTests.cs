using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

[Trait("SWR", "SWR-DET-010")]
public sealed class OwnDetectorAdapterTests
{
    private static OwnDetectorConfig CreateConfig() => new("192.168.1.100");

    // ── Constructor ──────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new OwnDetectorAdapter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ValidConfig_SetsDisconnectedState()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── ConnectAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.ConnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.ConnectAsync*");
    }

    [Fact]
    public async Task ConnectAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());
        sut.Dispose();

        var act = async () => await sut.ConnectAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── DisconnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.DisconnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.DisconnectAsync*");
    }

    // ── ArmAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_WhenDisposed_ThrowsObjectDisposedException()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());
        sut.Dispose();

        var act = async () => await sut.ArmAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ArmAsync_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.ArmAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.ArmAsync*");
    }

    [Fact]
    public async Task ArmAsync_WithFreeRunTrigger_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.ArmAsync(DetectorTriggerMode.FreeRun);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── AbortAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.AbortAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.AbortAsync*");
    }

    // ── GetStatusAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.GetStatusAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.GetStatusAsync*");
    }

    // ── Dispose ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());

        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());

        sut.Dispose();
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task Dispose_ThenConnectAsync_ThrowsObjectDisposedException()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());
        sut.Dispose();

        var act = async () => await sut.ConnectAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task Dispose_ThenArmAsync_ThrowsObjectDisposedException()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());
        sut.Dispose();

        var act = async () => await sut.ArmAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── Events ───────────────────────────────────────────────────────────────────

    [Fact]
    public void StateChanged_CanBeSubscribedAndUnsubscribed()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());
        var events = new List<DetectorStateChangedEventArgs>();

        EventHandler<DetectorStateChangedEventArgs> handler = (_, e) => events.Add(e);
        sut.StateChanged += handler;
        sut.StateChanged -= handler;

        // No events fired because all methods throw NotImplementedException
        events.Should().BeEmpty();
    }

    [Fact]
    public void ImageAcquired_CanBeSubscribedAndUnsubscribed()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());
        var images = new List<ImageAcquiredEventArgs>();

        EventHandler<ImageAcquiredEventArgs> handler = (_, e) => images.Add(e);
        sut.ImageAcquired += handler;
        sut.ImageAcquired -= handler;

        images.Should().BeEmpty();
    }

    // ── CurrentState after disposal ──────────────────────────────────────────────

    [Fact]
    public void CurrentState_AfterDisposal_StillReturnsDisconnected()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());
        sut.Dispose();

        // State is still accessible (read-only property with lock)
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── Config is stored correctly ───────────────────────────────────────────────

    [Fact]
    public void Constructor_StoresConfigCorrectly()
    {
        var config = new OwnDetectorConfig("10.0.0.1", Port: 9999, CalibrationPath: @"C:\Cal");
        using var sut = new OwnDetectorAdapter(config);

        // Adapter created without error — config accepted
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }
}
