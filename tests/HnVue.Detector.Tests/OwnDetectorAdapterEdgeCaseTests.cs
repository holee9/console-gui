using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Detector.OwnDetector;
using Xunit;

namespace HnVue.Detector.Tests;

/// <summary>
/// Edge-case tests for <see cref="OwnDetectorAdapter"/> to improve branch coverage.
/// All methods throw NotImplementedException (SDK not integrated yet), but we can
/// test the disposed paths, constructor validation, and state access patterns.
/// </summary>
[Trait("SWR", "SWR-DET-010")]
public sealed class OwnDetectorAdapterEdgeCaseTests
{
    private static OwnDetectorConfig CreateConfig(
        string host = "192.168.1.100",
        int port = 8888,
        string? calibrationPath = null,
        int bitsPerPixel = 14) =>
        new(host, Port: port, CalibrationPath: calibrationPath, BitsPerPixel: bitsPerPixel);

    // ── Config with different parameters ─────────────────────────────────────

    [Fact]
    public void Constructor_WithCalibrationPath_CreatesAdapterWithoutError()
    {
        var config = CreateConfig(calibrationPath: @"C:\HnVue\Cal\SN001");
        using var sut = new OwnDetectorAdapter(config);

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Constructor_With16BitConfig_CreatesAdapterWithoutError()
    {
        var config = CreateConfig(bitsPerPixel: 16);
        using var sut = new OwnDetectorAdapter(config);

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Constructor_WithCustomPort_CreatesAdapterWithoutError()
    {
        var config = CreateConfig(port: 9000);
        using var sut = new OwnDetectorAdapter(config);

        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    // ── DisconnectAsync when disposed ────────────────────────────────────────

    [Fact]
    public async Task DisconnectAsync_NotImplementedButNotDisposed_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.DisconnectAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.DisconnectAsync*");
    }

    // ── AbortAsync when disposed ─────────────────────────────────────────────

    [Fact]
    public async Task AbortAsync_NotImplementedThrows()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.AbortAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.AbortAsync*");
    }

    // ── GetStatusAsync when disposed ─────────────────────────────────────────

    [Fact]
    public async Task GetStatusAsync_NotImplementedThrows()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.GetStatusAsync();

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("*OwnDetectorAdapter.GetStatusAsync*");
    }

    // ── CancellationToken passed through ─────────────────────────────────────

    [Fact]
    public async Task ConnectAsync_WithCancellationToken_ThrowsNotImplementedBeforeCancellation()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());
        using var cts = new CancellationTokenSource();

        var act = async () => await sut.ConnectAsync(cts.Token);

        // NotImplementedException is thrown before cancellation is checked
        await act.Should().ThrowAsync<NotImplementedException>();
    }

    [Fact]
    public async Task ArmAsync_WithSyncMode_ThrowsNotImplementedException()
    {
        using var sut = new OwnDetectorAdapter(CreateConfig());

        var act = async () => await sut.ArmAsync(DetectorTriggerMode.Sync);

        await act.Should().ThrowAsync<NotImplementedException>();
    }

    // ── Dispose sequence ─────────────────────────────────────────────────────

    [Fact]
    public void Dispose_DoesNotChangeState()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());

        sut.Dispose();

        // State is still accessible and remains Disconnected
        sut.CurrentState.Should().Be(DetectorState.Disconnected);
    }

    [Fact]
    public void Dispose_ThreeTimesInARow_DoesNotThrow()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());

        sut.Dispose();
        sut.Dispose();
        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }

    // ── Events can be subscribed before dispose ──────────────────────────────

    [Fact]
    public void Events_SubscribeAndDispose_DoesNotThrow()
    {
        var sut = new OwnDetectorAdapter(CreateConfig());

        sut.StateChanged += (_, _) => { };
        sut.ImageAcquired += (_, _) => { };

        var act = () => sut.Dispose();

        act.Should().NotThrow();
    }
}
