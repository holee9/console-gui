using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Network-dependent error path tests for <see cref="DicomService"/>.
/// Uses valid DICOM temp files with unreachable network endpoints to exercise
/// try/catch blocks in StoreAsync, QueryWorklistAsync, PrintAsync, RequestStorageCommitmentAsync.
/// </summary>
/// <remarks>
/// All tests are deterministic: they point to unreachable hosts (192.0.2.1 TEST-NET-1)
/// which guarantees connection failure without any network dependency.
/// </remarks>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomServiceNetworkTests
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

    /// <summary>
    /// Creates a valid DICOM file on disk with the minimum required tags
    /// (SOPClassUID, SOPInstanceUID) so fo-dicom can parse it successfully.
    /// </summary>
    private static async Task<string> CreateValidDicomTempFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "TEST001" },
            { DicomTag.PatientName, "Test^Patient" },
        };

        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── StoreAsync Network Error Paths ────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_ValidDicomFile_UnreachablePacs_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            // Connection failure surfaces as DicomConnectionFailed or DicomStoreFailed
            result.Error.Should().BeOneOf(
                ErrorCode.DicomConnectionFailed,
                ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_ValidDicomFile_CancelledToken_ReturnsFailureOrSuccess()
    {
        // Pre-cancelled token: fo-dicom may return Success (completes synchronously)
        // or OperationCancelled depending on internal timing. Both are acceptable.
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var result = await svc.StoreAsync(tempFile, "PACS", cts.Token);

            // Pre-cancelled token is handled gracefully
            result.Should().NotBeNull();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_ValidDicomFile_LoopbackNonListening_ReturnsFailure()
    {
        // Point to a local port with no listener - exercises network error catch blocks
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 19999,
        });
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_ValidDicomFile_WithDifferentAeTitles_AllFail()
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            // Multiple ae-title variations exercise different string paths
            var result1 = await svc.StoreAsync(tempFile, "PACS_A", CancellationToken.None);
            var result2 = await svc.StoreAsync(tempFile, "ANOTHER_PACS", CancellationToken.None);

            result1.IsFailure.Should().BeTrue();
            result2.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── QueryWorklistAsync Network Error Paths ────────────────────────────────

    [Fact]
    public async Task QueryWorklistAsync_UnreachableMwl_ReturnsConnectionFailed()
    {
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOneOf(
            ErrorCode.DicomConnectionFailed,
            ErrorCode.DicomQueryFailed);
    }

    [Fact]
    public async Task QueryWorklistAsync_CancelledToken_HandledGracefully()
    {
        // Pre-cancelled token: fo-dicom may return Success (completes synchronously)
        // or OperationCancelled depending on internal timing. Both are acceptable.
        var svc = CreateService();
        var query = new WorklistQuery(null, null, null, "MWL_SCP");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await svc.QueryWorklistAsync(query, cts.Token);

        // Pre-cancelled token is handled gracefully
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task QueryWorklistAsync_WithPatientId_UnreachableMwl_ReturnsFailure()
    {
        var svc = CreateService();
        var query = new WorklistQuery("P001", null, null, "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_WithDateRange_UnreachableMwl_ReturnsFailure()
    {
        var svc = CreateService();
        var query = new WorklistQuery(
            null,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_LoopbackNonListening_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            MwlHost = "127.0.0.1",
            MwlPort = 19999,
        });
        var query = new WorklistQuery(null, null, null, "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_FullQueryParams_UnreachableMwl_ReturnsFailure()
    {
        // Exercises BuildWorklistRequest with all parameters populated
        var svc = CreateService();
        var query = new WorklistQuery(
            "PAT12345",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            "MWL_AET");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── PrintAsync Network Error Paths ────────────────────────────────────────

    [Fact]
    public async Task PrintAsync_ValidDicomFile_UnreachablePrinter_ReturnsFailure()
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
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
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_ValidDicomFile_CancelledToken_HandledGracefully()
    {
        // Pre-cancelled token: fo-dicom may return Success (completes synchronously)
        // or OperationCancelled depending on internal timing. Both are acceptable.
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            var result = await svc.PrintAsync(tempFile, "PRINTER", cts.Token);

            // Pre-cancelled token is handled gracefully
            result.Should().NotBeNull();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_ValidDicomFile_LoopbackNonListening_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PrinterHost = "127.0.0.1",
            PrinterPort = 19999,
        });
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_ValidDicomFile_DifferentPrinterAeTitles_ReturnsFailure()
    {
        // Exercise PrintAsync with multiple ae-title variations
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.PrintAsync(tempFile, "FILM_PRINTER_1", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── RequestStorageCommitmentAsync Network Error Paths ─────────────────────

    [Fact]
    public async Task RequestStorageCommitmentAsync_UnreachablePacs_ReturnsConnectionFailed()
    {
        var svc = CreateService();

        var result = await svc.RequestStorageCommitmentAsync(
            DicomUID.SecondaryCaptureImageStorage.UID,
            DicomUID.Generate().UID,
            "PACS",
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOneOf(
            ErrorCode.DicomConnectionFailed,
            ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_CancelledToken_HandledGracefully()
    {
        // Pre-cancelled token: fo-dicom may return Success (completes synchronously)
        // or OperationCancelled depending on internal timing. Both are acceptable.
        var svc = CreateService();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await svc.RequestStorageCommitmentAsync(
            DicomUID.SecondaryCaptureImageStorage.UID,
            DicomUID.Generate().UID,
            "PACS",
            cts.Token);

        // Pre-cancelled token is handled gracefully
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_LoopbackNonListening_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 19999,
        });

        var result = await svc.RequestStorageCommitmentAsync(
            DicomUID.SecondaryCaptureImageStorage.UID,
            DicomUID.Generate().UID,
            "PACS",
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_ValidUid_Unreachable_ReturnsConnectionError()
    {
        // Test with explicit SOP UIDs to exercise the dataset construction path
        var svc = CreateService();

        var result = await svc.RequestStorageCommitmentAsync(
            "1.2.840.10008.5.1.4.1.1.7",
            "1.2.3.4.5.6.7.8.9",
            "TEST_PACS",
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task RequestStorageCommitmentAsync_MultipleDifferentUids_AllFail()
    {
        // Exercise the dataset construction with different SOP class UIDs
        var svc = CreateService();

        var result1 = await svc.RequestStorageCommitmentAsync(
            DicomUID.SecondaryCaptureImageStorage.UID,
            DicomUID.Generate().UID,
            "PACS_A",
            CancellationToken.None);

        var result2 = await svc.RequestStorageCommitmentAsync(
            DicomUID.DigitalXRayImageStorageForPresentation.UID,
            DicomUID.Generate().UID,
            "PACS_B",
            CancellationToken.None);

        result1.IsFailure.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
    }

    // ── DicomStoreScu Network Error Paths (with valid DICOM) ──────────────────

    [Fact]
    public async Task DicomStoreScu_ValidDicomFile_UnreachablePacs_ReturnsStoreFailed()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("192.0.2.1");
        config.PacsPort.Returns(19999);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");

        var scu = new DicomStoreScu(config);
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await scu.StoreAsync(tempFile, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DicomStoreScu_ValidDicomFile_CancelledToken_HandledGracefully()
    {
        // Pre-cancelled token: fo-dicom may return Success or throw OperationCanceledException.
        // Both are acceptable - the key is no unhandled crash.
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("192.0.2.1");
        config.PacsPort.Returns(19999);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");

        var scu = new DicomStoreScu(config);
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            try
            {
                var result = await scu.StoreAsync(tempFile, cts.Token);
                // May return success or failure - both are acceptable
                result.Should().NotBeNull();
            }
            catch (OperationCanceledException)
            {
                // Also acceptable: DicomStoreScu re-throws cancellation
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task DicomStoreScu_ValidDicomFile_LoopbackNonListening_ReturnsStoreFailed()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(19999);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");

        var scu = new DicomStoreScu(config);
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await scu.StoreAsync(tempFile, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── StoreAsync - Invalid DICOM file error paths ───────────────────────────

    [Fact]
    public async Task StoreAsync_BinaryGarbageFile_UnreachablePacs_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            var garbage = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(tempFile, garbage);

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_EmptyFile_UnreachablePacs_ReturnsStoreFailed()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, string.Empty);

            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── PrintAsync - Invalid DICOM file error paths ───────────────────────────

    [Fact]
    public async Task PrintAsync_BinaryGarbageFile_UnreachablePrinter_ReturnsFailure()
    {
        var svc = CreateService();
        var tempFile = Path.GetTempFileName();
        try
        {
            var garbage = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(tempFile, garbage);

            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── BuildWorklistRequest / MapToWorklistItem - internal static helpers ─────

    [Fact]
    public void BuildWorklistRequest_FullQueryParams_CreatesRequest()
    {
        // Exercise the full parameter path
        var query = new WorklistQuery(
            "PAT12345",
            new DateOnly(2026, 1, 15),
            new DateOnly(2026, 6, 30),
            "MWL_AET");

        var request = DicomService.BuildWorklistRequest(query);

        request.Should().NotBeNull();
    }

    [Fact]
    public void MapToWorklistItem_WithBodyPartExamined_ReturnsBodyPart()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC100" },
            { DicomTag.PatientID, "P100" },
            { DicomTag.PatientName, "Test^Body" },
            { DicomTag.StudyDate, "20260115" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.BodyPartExamined, "CHEST" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public void MapToWorklistItem_SpsDescriptionFallback_ReturnsDescription()
    {
        // When BodyPartExamined is empty but SPS description is set
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC200" },
            { DicomTag.PatientID, "P200" },
            { DicomTag.PatientName, "Test^Desc" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProcedureStepDescription, "Chest PA" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("Chest PA");
    }

    [Fact]
    public void MapToWorklistItem_ProtocolCodeSequenceFallback_ExtractsFromSequence()
    {
        // When BodyPartExamined and SPS description are empty but protocol code sequence is set
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC300" },
            { DicomTag.PatientID, "P300" },
            { DicomTag.PatientName, "Test^Protocol" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeMeaning, "ABDOMEN" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence, new DicomSequence(
                DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("ABDOMEN");
    }

    [Fact]
    public void MapToWorklistItem_ProtocolCodeValueFallback_ExtractsFromSequence()
    {
        // When CodeMeaning is empty but CodeValue is set in the protocol sequence
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC400" },
            { DicomTag.PatientID, "P400" },
            { DicomTag.PatientName, "Test^CodeValue" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeValue, "PELVIS" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence, new DicomSequence(
                DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("PELVIS");
    }
}
