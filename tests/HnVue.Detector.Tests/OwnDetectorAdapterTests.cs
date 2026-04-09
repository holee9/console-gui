using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Tests for OwnDetectorAdapter — SDK skeleton that throws NotImplementedException.
/// These tests validate the adapter contract (constructor, disposal, null guards)
/// and document expected behaviour once the SDK is integrated.
/// SWR-WF-031: OwnDetectorAdapter must implement IDetectorInterface.
/// </summary>
[Trait("SWR", "SWR-WF-031")]
public sealed class OwnDetectorAdapterTests : IDisposable
{
    private readonly OwnDetectorConfig _config = new("192.168.1.100");
    private readonly OwnDetectorAdapter _sut;

    public OwnDetectorAdapterTests()
    {
        _sut = new OwnDetectorAdapter(_config);
    }

    public void Dispose() => _sut.Dispose();

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithValidConfig_SetsInitialStateToDisconnected()
    {
        _sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new OwnDetectorAdapter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    // ── ConnectAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_BeforeSdkImplemented_ThrowsNotImplementedException()
    {
        // The adapter is a placeholder; all SDK calls throw until integrated.
        var act = async () => await _sut.ConnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task ConnectAsync_WithCancellationToken_ThrowsNotImplementedException()
    {
        using var cts = new CancellationTokenSource();

        var act = async () => await _sut.ConnectAsync(cts.Token);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── DisconnectAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_BeforeSdkImplemented_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.DisconnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── ArmAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task ArmAsync_DefaultTriggerMode_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.ArmAsync();

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task ArmAsync_FreeRunTriggerMode_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.ArmAsync(DetectorTriggerMode.FreeRun);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task ArmAsync_SyncTriggerMode_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.ArmAsync(DetectorTriggerMode.Sync);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── AbortAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_BeforeSdkImplemented_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.AbortAsync();

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── GetStatusAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_BeforeSdkImplemented_ThrowsNotImplementedException()
    {
        var act = async () => await _sut.GetStatusAsync();

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── IDisposable ───────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_CalledOnce_DoesNotThrow()
    {
        using var adapter = new OwnDetectorAdapter(_config);

        var act = () => adapter.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_IsIdempotent()
    {
        var adapter = new OwnDetectorAdapter(_config);

        adapter.Dispose();
        var act = () => adapter.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public async Task ConnectAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var adapter = new OwnDetectorAdapter(_config);
        adapter.Dispose();

        var act = async () => await adapter.ConnectAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    [Fact]
    public async Task ArmAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var adapter = new OwnDetectorAdapter(_config);
        adapter.Dispose();

        var act = async () => await adapter.ArmAsync();

        await act.Should().ThrowAsync<ObjectDisposedException>();
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public void StateChanged_EventCanBeSubscribed_WithoutThrowing()
    {
        var act = () =>
        {
            _sut.StateChanged += (_, _) => { };
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void ImageAcquired_EventCanBeSubscribed_WithoutThrowing()
    {
        var act = () =>
        {
            _sut.ImageAcquired += (_, _) => { };
        };

        act.Should().NotThrow();
    }
}
