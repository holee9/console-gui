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
    public async Task SendInProgressAsync_NetworkUnreachable_ReturnsFailure()
    {
        var sut = CreateSut(new DicomOptions
        {
            MppsHost = "192.0.2.1", // TEST-NET-1 unreachable
            MppsPort = 19999,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
        });
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST");

        result.IsFailure.Should().BeTrue();
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
    public async Task SendCompletedAsync_NetworkUnreachable_ReturnsFailure()
    {
        var sut = CreateSut(new DicomOptions
        {
            MppsHost = "192.0.2.1",
            MppsPort = 19999,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
        });
        var result = await sut.SendCompletedAsync("1.2.3.4.5.6", completed: false);

        result.IsFailure.Should().BeTrue();
    }

    // ── TlsEnabled Configuration ────────────────────────────────────────────

    [Fact]
    public async Task SendInProgressAsync_TlsEnabled_Unreachable_ReturnsFailure()
    {
        var sut = CreateSut(new DicomOptions
        {
            MppsHost = "192.0.2.1",
            MppsPort = 19999,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
            TlsEnabled = true,
        });
        var result = await sut.SendInProgressAsync("1.2.3.4.5", "P001", "ABDOMEN");

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task SendCompletedAsync_TlsEnabled_Unreachable_ReturnsFailure()
    {
        var sut = CreateSut(new DicomOptions
        {
            MppsHost = "192.0.2.1",
            MppsPort = 19999,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
            TlsEnabled = true,
        });
        var result = await sut.SendCompletedAsync("1.2.3.4.5.6", completed: true);

        result.IsFailure.Should().BeTrue();
    }
}
