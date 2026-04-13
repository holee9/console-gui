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
/// Targeted coverage boost tests for HnVue.Dicom.
/// Focuses on remaining gaps in DicomService async state machines,
/// PrintAsync error paths, and MapToWorklistItem edge cases.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomCoverageBoostTests
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
            MppsHost = "192.0.2.1",
            MppsPort = 19999,
        });
        return new DicomService(opts, NullLogger<DicomService>.Instance);
    }

    private static async Task<string> CreateValidDicomTempFileAsync()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "BOOST001" },
            { DicomTag.PatientName, "Boost^Test" },
        };

        var dicomFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"dicom_boost_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── StoreAsync — Additional error paths to exercise more catch blocks ──────

    [Fact]
    public async Task StoreAsync_NonDicomFile_Loopback_ReturnsStoreFailed()
    {
        // Non-DICOM file + loopback unreachable host exercises IOException catch
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 19999,
        });
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, "not dicom binary"u8.ToArray());
            var result = await svc.StoreAsync(tempFile, "PACS", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData("PACS_A")]
    [InlineData("PACS_B")]
    [InlineData("TEST_PACS")]
    public async Task StoreAsync_ValidDicomFile_DifferentAeTitles_ReturnsFailure(string aeTitle)
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, aeTitle, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_ValidDicomFile_WhitespaceAeTitle_ReturnsStoreFailed()
    {
        // Whitespace aeTitle passes the null check but is empty after trim
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.StoreAsync(tempFile, "   ", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── PrintAsync — Additional error paths to improve from 57% to 70%+ ───────

    [Fact]
    public async Task PrintAsync_NonDicomBinary_LoopbackPrinter_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PrinterHost = "127.0.0.1",
            PrinterPort = 19999,
        });
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, new byte[] { 0x00, 0x01, 0x02, 0xFF });
            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_EmptyFile_LoopbackPrinter_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PrinterHost = "127.0.0.1",
            PrinterPort = 19999,
        });
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, string.Empty);
            var result = await svc.PrintAsync(tempFile, "PRINTER", CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Theory]
    [InlineData("PRINTER_1")]
    [InlineData("FILM_PRINTER")]
    public async Task PrintAsync_ValidDicomFile_DifferentAeTitles_ReturnsFailure(string aeTitle)
    {
        var svc = CreateService();
        var tempFile = await CreateValidDicomTempFileAsync();
        try
        {
            var result = await svc.PrintAsync(tempFile, aeTitle, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task PrintAsync_NullFilePath_ExplicitNull_ReturnsPrintFailed()
    {
        var svc = CreateService();
        var result = await svc.PrintAsync(null!, "PRINTER", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomPrintFailed);
    }

    // ── QueryWorklistAsync — Additional paths ──────────────────────────────────

    [Fact]
    public async Task QueryWorklistAsync_LoopbackNonListening_WithDateRange_ReturnsFailure()
    {
        var svc = CreateService(new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            MwlHost = "127.0.0.1",
            MwlPort = 19999,
        });
        var query = new WorklistQuery(
            "PAT001",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "MWL_SCP");

        var result = await svc.QueryWorklistAsync(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task QueryWorklistAsync_CancelledToken_WithDateRange_ReturnsResult()
    {
        var svc = CreateService();
        var query = new WorklistQuery(
            null,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            "MWL_SCP");
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await svc.QueryWorklistAsync(query, cts.Token);

        result.Should().NotBeNull();
    }

    // ── RequestStorageCommitmentAsync — Additional paths ───────────────────────

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
            "1.2.840.10008.5.1.4.1.1.7",
            "1.2.3.4.5.6.7.8",
            "PACS",
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── DicomStoreScu — Additional coverage ────────────────────────────────────

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

    [Fact]
    public async Task DicomStoreScu_CancelledToken_WithValidDicom_ThrowsOrReturns()
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
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            try
            {
                var result = await scu.StoreAsync(tempFile, cts.Token);
                result.Should().NotBeNull();
            }
            catch (OperationCanceledException)
            {
                // DicomStoreScu re-throws cancellation - acceptable
            }
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── MapToWorklistItem — All body part extraction paths ─────────────────────

    [Fact]
    public void MapToWorklistItem_BodyPartExaminedTakesPriority()
    {
        // Both BodyPartExamined and SPS description set → BodyPartExamined wins
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC500" },
            { DicomTag.PatientID, "P500" },
            { DicomTag.PatientName, "Priority^Test" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.BodyPartExamined, "CHEST" },
            { DicomTag.ScheduledProcedureStepDescription, "Chest PA and Lateral" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public void MapToWorklistItem_SpsDescriptionUsedWhenNoBodyPartExamined()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC600" },
            { DicomTag.PatientID, "P600" },
            { DicomTag.PatientName, "Desc^Fallback" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProcedureStepDescription, "Abdomen AP" },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("Abdomen AP");
    }

    [Fact]
    public void MapToWorklistItem_CodeValueFallbackWhenCodeMeaningEmpty()
    {
        // CodeMeaning empty, CodeValue present → CodeValue used
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC700" },
            { DicomTag.PatientID, "P700" },
            { DicomTag.PatientName, "CodeVal^Test" },
        };

        var protocolItem = new DicomDataset
        {
            { DicomTag.CodeValue, "EXTREMITY" },
            { DicomTag.CodeMeaning, string.Empty },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence, protocolItem) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().Be("EXTREMITY");
    }

    [Fact]
    public void MapToWorklistItem_AllBodyPartSourcesEmpty_ReturnsNull()
    {
        // SPS sequence present but all fields empty
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC800" },
            { DicomTag.PatientID, "P800" },
            { DicomTag.PatientName, "Empty^Body" },
        };

        var spsItem = new DicomDataset();
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    [Fact]
    public void MapToWorklistItem_EmptyProtocolCodeSequence_ReturnsNull()
    {
        var dataset = new DicomDataset
        {
            { DicomTag.AccessionNumber, "ACC900" },
            { DicomTag.PatientID, "P900" },
        };

        var spsItem = new DicomDataset
        {
            { DicomTag.ScheduledProtocolCodeSequence,
                new DicomSequence(DicomTag.ScheduledProtocolCodeSequence) },
        };
        dataset.Add(new DicomSequence(DicomTag.ScheduledProcedureStepSequence, spsItem));

        var item = DicomService.MapToWorklistItem(dataset);

        item.BodyPart.Should().BeNull();
    }

    // ── DicomOptions — Full property coverage ──────────────────────────────────

    [Fact]
    public void DicomOptions_AllProperties_CanBeSetAndRead()
    {
        var opts = new DicomOptions
        {
            LocalAeTitle = "SCU",
            PacsAeTitle = "PACS",
            PacsHost = "10.0.0.1",
            PacsPort = 11112,
            MwlAeTitle = "MWL",
            MwlHost = "10.0.0.2",
            MwlPort = 11113,
            PrinterAeTitle = "PRINT",
            PrinterHost = "10.0.0.3",
            PrinterPort = 11114,
            MppsAeTitle = "MPPS",
            MppsHost = "10.0.0.4",
            MppsPort = 11115,
            TlsEnabled = true,
        };

        opts.LocalAeTitle.Should().Be("SCU");
        opts.PacsAeTitle.Should().Be("PACS");
        opts.PacsHost.Should().Be("10.0.0.1");
        opts.PacsPort.Should().Be(11112);
        opts.MwlAeTitle.Should().Be("MWL");
        opts.MwlHost.Should().Be("10.0.0.2");
        opts.MwlPort.Should().Be(11113);
        opts.PrinterAeTitle.Should().Be("PRINT");
        opts.PrinterHost.Should().Be("10.0.0.3");
        opts.PrinterPort.Should().Be(11114);
        opts.MppsAeTitle.Should().Be("MPPS");
        opts.MppsHost.Should().Be("10.0.0.4");
        opts.MppsPort.Should().Be(11115);
        opts.TlsEnabled.Should().BeTrue();
    }

    // ── DicomFileIO — Additional Write edge cases ──────────────────────────────

    [Fact]
    public async Task WriteAsync_WrapperWithExistingDirectory_WritesSuccessfully()
    {
        // Write to an existing directory (no creation needed)
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };
        var dicomFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dicomFile);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"dicom_existing_dir_{Guid.NewGuid():N}.dcm");

        try
        {
            var result = await DicomFileIO.WriteAsync(wrapper, outputPath);

            result.IsSuccess.Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ReadAsync_ValidDicomFile_ThenWriteAsync_ThenReadAsync_Roundtrip()
    {
        // Full round-trip: create → write → read → verify
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
            { DicomTag.PatientID, "ROUNDTRIP_PAT" },
        };
        var dicomFile = new DicomFile(dataset);
        var wrapper = new DicomFileWrapper(dicomFile);

        var writePath = Path.Combine(
            Path.GetTempPath(),
            $"dicom_rt_{Guid.NewGuid():N}.dcm");

        try
        {
            // Write
            var writeResult = await DicomFileIO.WriteAsync(wrapper, writePath);
            writeResult.IsSuccess.Should().BeTrue();

            // Read back
            var readResult = await DicomFileIO.ReadAsync(writePath);
            readResult.IsSuccess.Should().BeTrue();
            readResult.Value.SopInstanceUid.Should().NotBeNullOrWhiteSpace();

            // GetTagValue
            var tagResult = await DicomFileIO.GetTagValueAsync(writePath, "PatientID");
            tagResult.IsSuccess.Should().BeTrue();
            tagResult.Value.Should().Be("ROUNDTRIP_PAT");
        }
        finally
        {
            if (File.Exists(writePath)) File.Delete(writePath);
        }
    }
}
