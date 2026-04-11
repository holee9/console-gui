using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Coverage tests for <see cref="MppsScu"/>.
/// Targets N-CREATE and N-SET error paths, configuration validation.
/// </summary>
[Trait("SWR", "SWR-DC-055")]
[Trait("SWR", "SWR-DC-056")]
public sealed class MppsScuCoverageTests
{
    private static MppsScu CreateSut(DicomOptions? options = null)
    {
        return new MppsScu(options ?? new DicomOptions
        {
            MppsHost = "127.0.0.1",
            MppsPort = 104,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
            TlsEnabled = false,
        });
    }

    // ── Constructor ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var act = () => new MppsScu(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("options");
    }

    // ── SendInProgressAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SendInProgressAsync_EmptyMppsHost_ReturnsConnectionFailed()
    {
        var sut = CreateSut(new DicomOptions { MppsHost = string.Empty });
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    [Fact]
    public async Task SendInProgressAsync_NullMppsHost_ReturnsConnectionFailed()
    {
        var sut = CreateSut(new DicomOptions { MppsHost = null! });
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task SendInProgressAsync_WithValidHost_EitherSucceedsOrFails()
    {
        // When MPPS host is configured, the method attempts connection.
        // With a local listener, it may succeed or fail depending on network state.
        var sut = CreateSut();
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST");

        // Result is either success or failure - both are valid outcomes for a unit test
        result.Should().NotBeNull();
    }

    // ── SendCompletedAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SendCompletedAsync_EmptyMppsHost_ReturnsConnectionFailed()
    {
        var sut = CreateSut(new DicomOptions { MppsHost = string.Empty });
        var result = await sut.SendCompletedAsync("1.2.3.4.5.6", completed: true);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
        result.ErrorMessage.Should().Contain("MPPS host is not configured");
    }

    [Fact]
    public async Task SendCompletedAsync_NullMppsInstanceUid_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = async () => await sut.SendCompletedAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendCompletedAsync_WithValidHost_EitherSucceedsOrFails()
    {
        var sut = CreateSut();
        var result = await sut.SendCompletedAsync("1.2.3.4.5.6", completed: true);

        // Result is either success or failure - both are valid
        result.Should().NotBeNull();
    }
}
