using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="DicomService"/> covering parameter validation,
/// error code mapping, and worklist item construction logic.
/// </summary>
public sealed class DicomServiceTests
{
    private static DicomService CreateService(DicomOptions? options = null)
    {
        var opts = Options.Create(options ?? new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
            MwlHost = "127.0.0.1",
            MwlPort = 104,
            PrinterHost = "127.0.0.1",
            PrinterPort = 104,
        });
        return new DicomService(opts, NullLogger<DicomService>.Instance);
    }

    // ── StoreAsync – Parameter Validation ────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NullFilePath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(null!, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_EmptyFilePath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(string.Empty, "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_WhitespaceFilePath_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("   ", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_EmptyAeTitle_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync("some.dcm", string.Empty, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_NonExistentFile_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var result = await svc.StoreAsync(@"C:\nonexistent\file.dcm", "PACS", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다");
    }

    // ── QueryWorklistAsync – Parameter Validation ────────────────────────────

    [Fact]
    public async Task QueryWorklistAsync_EmptyAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, string.Empty);
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_WhitespaceAeTitle_ReturnsQueryFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, "   ");
        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    // ── PrintAsync – Parameter Validation ───────────────────────────────────

    [Fact]
    public async Task PrintAsync_EmptyFilePath_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(string.Empty, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_EmptyPrinterAeTitle_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync("some.dcm", string.Empty, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    [Fact]
    public async Task PrintAsync_NonExistentFile_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(@"C:\nonexistent\file.dcm", "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
        result.ErrorMessage.Should().Contain("not found");
    }

    // ── DicomOptions binding ──────────────────────────────────────────────────

    [Fact]
    public void DicomOptions_Defaults_AreExpected()
    {
        var opts = new DicomOptions();

        opts.LocalAeTitle.Should().Be("HNVUE");
        opts.PacsPort.Should().Be(104);
        opts.MwlPort.Should().Be(104);
        opts.PrinterPort.Should().Be(104);
        opts.TlsEnabled.Should().BeFalse();
        opts.PacsAeTitle.Should().BeEmpty();
        opts.PacsHost.Should().BeEmpty();
    }

    [Fact]
    public void DicomOptions_CanBeConfigured()
    {
        var opts = new DicomOptions
        {
            LocalAeTitle = "MY_SCU",
            PacsAeTitle = "MY_PACS",
            PacsHost = "192.168.1.100",
            PacsPort = 11112,
            TlsEnabled = true
        };

        opts.LocalAeTitle.Should().Be("MY_SCU");
        opts.PacsAeTitle.Should().Be("MY_PACS");
        opts.PacsPort.Should().Be(11112);
        opts.TlsEnabled.Should().BeTrue();
    }
}
