using System.Globalization;
using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Coverage gap tests for HnVue.Dicom to reach 85%+ target.
/// Targets remaining uncovered branches in DicomService and MppsScu.
/// S10-R4 Task 1: Dicom 83.7% → 85%+
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
[Trait("SWR", "SWR-DC-055")]
[Trait("SWR", "SWR-DC-056")]
public sealed class DicomCoverageGapTests
{
    private static DicomService CreateService(DicomOptions? options = null)
    {
        var opts = Options.Create(options ?? new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "192.0.2.1",
            PacsPort = 19999,
            MwlHost = "192.0.2.1",
            MwlPort = 19999,
            PrinterHost = "192.0.2.1",
            PrinterPort = 19999,
        });
        return new DicomService(opts, NullLogger<DicomService>.Instance);
    }

    private static MppsScu CreateMppsScu(DicomOptions? options = null)
    {
        return new MppsScu(options ?? new DicomOptions
        {
            MppsHost = "192.0.2.1",
            MppsPort = 19999,
            LocalAeTitle = "HNVUE",
            MppsAeTitle = "MPPS_SCP",
        });
    }

    private static async Task<string> CreateValidDicomFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "TEST001" },
            { DicomTag.PatientName, "Test^Patient" },
        };
        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), "dicom_gap_" + Guid.NewGuid().ToString("N") + ".dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    [Fact]
    public async Task StoreAsync_InvalidDicomFileWithEmptyDataset_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, new byte[] { 0x00, 0x01, 0x02, 0x03 });
            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOneOf(
                ErrorCode.DicomStoreFailed,
                ErrorCode.DicomConnectionFailed);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task QueryWorklistAsync_WithQueryParameters_HandlesNetworkError()
    {
        var svc = CreateService();
        var query = new WorklistQuery(
            PatientId: "PAT123",
            DateFrom: new DateOnly(2026, 1, 1),
            DateTo: new DateOnly(2026, 12, 31),
            AeTitle: "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOneOf(
            ErrorCode.DicomConnectionFailed,
            ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task PrintAsync_WithValidFile_UnreachablePrinter_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomFileAsync();
        try
        {
            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().BeOneOf(
                ErrorCode.DicomConnectionFailed,
                ErrorCode.DicomPrintFailed);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task MppsScu_SendInProgressAsync_UnreachableHost_ReturnsConnectionFailed()
    {
        var scu = CreateMppsScu();
        var result = await scu.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task MppsScu_SendInProgressAsync_WithCancellation_CancelsGracefully()
    {
        var scu = CreateMppsScu();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await scu.SendInProgressAsync("1.2.3.4.5", "P001", "CHEST", cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOneOf(
            ErrorCode.OperationCancelled,
            ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task MppsScu_SendCompletedAsync_Completed_UnreachableHost_ReturnsConnectionFailed()
    {
        var scu = CreateMppsScu();
        var result = await scu.SendCompletedAsync("1.2.3.4.5.6", completed: true, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task MppsScu_SendCompletedAsync_Discontinued_UnreachableHost_ReturnsConnectionFailed()
    {
        var scu = CreateMppsScu();
        var result = await scu.SendCompletedAsync("1.2.3.4.5.6", completed: false, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task MppsScu_SendCompletedAsync_WithCancellation_CancelsGracefully()
    {
        var scu = CreateMppsScu();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var result = await scu.SendCompletedAsync("1.2.3.4.5.6", completed: true, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOneOf(
            ErrorCode.OperationCancelled,
            ErrorCode.DicomConnectionFailed);
    }
}
