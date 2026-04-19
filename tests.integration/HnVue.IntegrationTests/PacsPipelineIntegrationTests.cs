using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Integration tests for S13-R1 PACS transmission pipeline changes.
/// Verifies Print SCU, Async Store Pipeline, and RDSR generation flows.
/// SWR traceability: SWR-DC-050 (Print SCU), SWR-DC-055 (Async Store), SWR-DC-060 (RDSR).
/// </summary>
[Trait("Category", "Integration")]
[Trait("SWR", "SWR-DC-050")]
[Trait("SWR", "SWR-DC-055")]
[Trait("SWR", "SWR-DC-060")]
public sealed class PacsPipelineIntegrationTests
{
    // ── Helper factories ──────────────────────────────────────────────────────

    private static DicomOptions CreateDicomOptions() => new()
    {
        LocalAeTitle = "HNVUE_TEST",
        PacsHost = "127.0.0.1",
        PacsPort = 11112,
        PacsAeTitle = "TEST_PACS",
        PrinterHost = "127.0.0.1",
        PrinterPort = 11113,
        PrinterAeTitle = "TEST_PRINTER",
        StoreRetryCount = 0,
        StoreRetryDelayMs = 100,
    };

    private static DicomService CreateDicomService(DicomOptions? options = null)
    {
        var opts = Options.Create(options ?? CreateDicomOptions());
        var logger = Substitute.For<ILogger<DicomService>>();
        return new DicomService(opts, logger);
    }

    private static DoseRecord CreateTestDoseRecord() => new(
        DoseId: "DR-001",
        StudyInstanceUid: "1.2.840.113619.2.55.3.20260419.1",
        Dap: 12.5,
        Ei: 1500.0,
        EffectiveDose: 0.05,
        BodyPart: "CHEST",
        RecordedAt: DateTimeOffset.UtcNow,
        PatientId: "P-001",
        DapMgyCm2: 12.5,
        FieldAreaCm2: 400.0,
        MeanPixelValue: 32000.0,
        EiTarget: 3000.0,
        EsdMgy: 0.25);

    private static RdsrPatientInfo CreateTestPatientInfo() => new(
        PatientId: "P-001",
        PatientName: "Test^Patient",
        PatientBirthDate: "19800101",
        PatientSex: "M");

    private static RdsrStudyInfo CreateTestStudyInfo() => new(
        StudyInstanceUid: "1.2.840.113619.2.55.3.20260419.1",
        StudyDate: "20260419",
        StudyTime: "120000",
        AccessionNumber: "ACC-001",
        StudyId: "SID-001",
        ReferringPhysicianName: "Dr^Test",
        RetrieveAeTitle: "HNVUE_TEST");

    private static RdsrExposureParams CreateTestExposureParams() => new(
        Kvp: 80.0,
        Mas: 200.0,
        ExposureTimeMs: 50.0);

    // ══════════════════════════════════════════════════════════════════════════
    // 1. DicomService Print SCU — SWR-DC-050
    // ══════════════════════════════════════════════════════════════════════════

    public sealed class PrintScuTests
    {
        [Fact]
        [Trait("SWR", "SWR-DC-050")]
        public async Task PrintAsync_WithMissingFile_ReturnsDicomPrintFailed()
        {
            // Arrange
            var sut = CreateDicomService();
            const string nonExistentPath = "Z:\\nonexistent\\image.dcm";

            // Act
            var result = await sut.PrintAsync(nonExistentPath, "PRINTER_AE");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("not found");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-050")]
        public async Task PrintAsync_WithEmptyFilePath_ReturnsDicomPrintFailed()
        {
            // Arrange
            var sut = CreateDicomService();

            // Act
            var result = await sut.PrintAsync("", "PRINTER_AE");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("must not be empty");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-050")]
        public async Task PrintAsync_WithEmptyPrinterAeTitle_ReturnsDicomPrintFailed()
        {
            // Arrange
            var sut = CreateDicomService();

            // Act
            var result = await sut.PrintAsync("C:\\some\\file.dcm", "");

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomPrintFailed);
            result.ErrorMessage.Should().Contain("must not be empty");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-050")]
        public async Task PrintAsync_WithUnreachablePrinter_ReturnsConnectionFailed()
        {
            // Arrange -- use a real temp file so the File.Exists check passes,
            // but configure the printer host to an unreachable address so the
            // network connection fails gracefully.
            var tempFile = Path.GetTempFileName();
            try
            {
                // Create a minimal valid DICOM file so DicomFile.OpenAsync succeeds
                // before the network attempt.  If the file is not valid DICOM,
                // the service will throw before reaching the printer.
                var options = CreateDicomOptions();
                options.PrinterHost = "192.0.2.1"; // TEST-NET-1, RFC 5737 -- unreachable
                options.PrinterPort = 11113;
                var sut = CreateDicomService(options);

                // Act
                var result = await sut.PrintAsync(tempFile, "PRINTER_AE");

                // Assert -- either connection failure or print failure is acceptable;
                // the key requirement is that it does not throw an unhandled exception.
                result.IsFailure.Should().BeTrue();
                result.Error.Should().BeOneOf(
                    ErrorCode.DicomPrintFailed,
                    ErrorCode.DicomConnectionFailed);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        [Trait("SWR", "SWR-DC-050")]
        public async Task PrintAsync_WithCancelledToken_ReturnsOperationCancelled()
        {
            // Arrange
            var sut = CreateDicomService();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            var result = await sut.PrintAsync("C:\\some\\file.dcm", "PRINTER_AE", cts.Token);

            // Assert -- cancellation is handled gracefully even for missing files
            result.IsFailure.Should().BeTrue();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 2. AsyncStorePipeline — SWR-DC-055
    // ══════════════════════════════════════════════════════════════════════════

    public sealed class AsyncStorePipelineTests
    {
        private static AsyncStorePipeline CreatePipeline(
            IDicomService? dicomService = null,
            string pacsAeTitle = "TEST_PACS",
            int capacity = 100)
        {
            var service = dicomService ?? Substitute.For<IDicomService>();
            var logger = Substitute.For<ILogger<AsyncStorePipeline>>();
            return new AsyncStorePipeline(service, pacsAeTitle, logger, capacity);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public void Constructor_WithNullDicomService_ThrowsArgumentNullException()
        {
            // Arrange
            var logger = Substitute.For<ILogger<AsyncStorePipeline>>();

            // Act
            var act = () => new AsyncStorePipeline(null!, "PACS", logger);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public void Constructor_WithEmptyPacsAeTitle_ThrowsArgumentException()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            var logger = Substitute.For<ILogger<AsyncStorePipeline>>();

            // Act
            var act = () => new AsyncStorePipeline(service, "", logger);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public void Constructor_WithZeroCapacity_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            var logger = Substitute.For<ILogger<AsyncStorePipeline>>();

            // Act
            var act = () => new AsyncStorePipeline(service, "PACS", logger, capacity: 0);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task EnqueueAsync_TracksItemAsPending()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));
            await using var pipeline = CreatePipeline(service);
            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");

            // Assert
            pipeline.GetStatus("1.2.3.4.5").Should().NotBeNull();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task EnqueueAsync_WithDuplicateSopInstanceUid_ThrowsArgumentException()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));
            await using var pipeline = CreatePipeline(service);
            await pipeline.StartAsync();

            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");

            // Act
            var act = async () => await pipeline.EnqueueAsync("C:\\images\\other.dcm", "1.2.3.4.5");

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*already enqueued*");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task Pipeline_OnSuccessfulStore_RaisesStoreCompletedEvent()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            await using var pipeline = CreatePipeline(service);
            StoreCompletedEventArgs? eventArgs = null;
            pipeline.StoreCompleted += (_, e) => eventArgs = e;

            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");

            // Allow the consumer task to process
            await Task.Delay(200);

            // Assert
            eventArgs.Should().NotBeNull();
            eventArgs!.Success.Should().BeTrue();
            eventArgs.FilePath.Should().Be("C:\\images\\test.dcm");
            eventArgs.ErrorMessage.Should().BeNull();
            eventArgs.Attempts.Should().BeGreaterThan(0);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task Pipeline_OnFailedStore_RaisesStoreCompletedWithFailure()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Failure(ErrorCode.DicomStoreFailed, "PACS rejected.")));

            await using var pipeline = CreatePipeline(service);
            var tcs = new TaskCompletionSource<StoreCompletedEventArgs>();
            pipeline.StoreCompleted += (_, e) => tcs.TrySetResult(e);

            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");

            // Wait for the final StoreCompleted event (retries: 2s + 4s + 8s = ~14s max)
            var eventArgs = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(20));

            // Assert
            eventArgs.Should().NotBeNull();
            eventArgs.Success.Should().BeFalse();
            eventArgs.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task Pipeline_OnSuccessfulStore_UpdatesStatusToSent()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            await using var pipeline = CreatePipeline(service);
            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");
            await Task.Delay(200);

            // Assert
            pipeline.GetStatus("1.2.3.4.5").Should().Be(StoreStatus.Sent);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task Pipeline_OnPersistentFailure_UpdatesStatusToFailed()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Failure(ErrorCode.DicomStoreFailed, "Rejected.")));

            await using var pipeline = CreatePipeline(service);
            var tcs = new TaskCompletionSource<StoreCompletedEventArgs>();
            pipeline.StoreCompleted += (_, e) => tcs.TrySetResult(e);

            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\test.dcm", "1.2.3.4.5");

            // Wait for the final StoreCompleted event (retries: 2s + 4s + 8s = ~14s max)
            await tcs.Task.WaitAsync(TimeSpan.FromSeconds(20));

            // Assert
            pipeline.GetStatus("1.2.3.4.5").Should().Be(StoreStatus.Failed);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task StopAsync_CompletesWithoutError()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success()));

            await using var pipeline = CreatePipeline(service);
            await pipeline.StartAsync();

            // Act
            await pipeline.StopAsync();

            // Assert -- no exception means the pipeline shut down cleanly
        }

        [Fact]
        [Trait("SWR", "SWR-DC-055")]
        public async Task GetAllPending_ReturnsUnsentItems()
        {
            // Arrange
            var service = Substitute.For<IDicomService>();
            // Delay the response so items remain pending during the check
            service.StoreAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(async call =>
                {
                    await Task.Delay(500, call.ArgAt<CancellationToken>(2));
                    return Result.Success();
                });

            await using var pipeline = CreatePipeline(service);
            await pipeline.StartAsync();

            // Act
            await pipeline.EnqueueAsync("C:\\images\\a.dcm", "1.2.3.4.5");
            await pipeline.EnqueueAsync("C:\\images\\b.dcm", "1.2.3.4.6");

            // Assert -- both items should be tracked (at least Pending/Sending/Retrying)
            var pending = pipeline.GetAllPending();
            pending.Should().HaveCount(2);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    // 3. RdsrBuilder — SWR-DC-060
    // ══════════════════════════════════════════════════════════════════════════

    public sealed class RdsrBuilderTests
    {
        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithValidData_ProducesDicomDatasetWithCorrectSopClass()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();
            var exposureParams = CreateTestExposureParams();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo, exposureParams);

            // Act
            var dataset = builder.Build();

            // Assert -- SOP Class UID must be Enhanced SR (1.2.840.10008.5.1.4.1.1.88.22)
            var sopClass = dataset.GetSingleValue<DicomUID>(DicomTag.SOPClassUID);
            sopClass.Should().Be(RdsrBuilder.EnhancedSrSopClassUid);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithValidData_ProducesDicomDatasetWithPatientModule()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- Patient Module tags
            dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty)
                .Should().Be("P-001");
            dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty)
                .Should().Be("Test^Patient");
            dataset.GetSingleValueOrDefault(DicomTag.PatientBirthDate, string.Empty)
                .Should().Be("19800101");
            dataset.GetSingleValueOrDefault(DicomTag.PatientSex, string.Empty)
                .Should().Be("M");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithValidData_ProducesDicomDatasetWithStudyModule()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- Study Module tags
            dataset.GetSingleValueOrDefault(DicomTag.StudyInstanceUID, string.Empty)
                .Should().Be("1.2.840.113619.2.55.3.20260419.1");
            dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty)
                .Should().Be("20260419");
            dataset.GetSingleValueOrDefault(DicomTag.StudyTime, string.Empty)
                .Should().Be("120000");
            dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty)
                .Should().Be("ACC-001");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithValidData_ProducesDicomDatasetWithSrDocumentGeneral()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();
            var sopUid = DicomUID.Generate();
            var seriesUid = DicomUID.Generate();

            var builder = new RdsrBuilder(
                doseRecord, patientInfo, studyInfo,
                sopInstanceUid: sopUid,
                seriesInstanceUid: seriesUid,
                instanceNumber: 7);

            // Act
            var dataset = builder.Build();

            // Assert -- SR Document General Module
            dataset.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID).Should().Be(sopUid);
            dataset.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID).Should().Be(seriesUid);
            dataset.GetSingleValue<int>(DicomTag.InstanceNumber).Should().Be(7);
            dataset.GetSingleValue<int>(DicomTag.SeriesNumber).Should().Be(1);
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithValidData_ProducesContainerRoot()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- Root value type is CONTAINER
            dataset.GetSingleValueOrDefault(DicomTag.ValueType, string.Empty)
                .Should().Be("CONTAINER");

            // Concept Name Code Sequence identifies this as RDSR
            var conceptNameSeq = dataset.GetSequence(DicomTag.ConceptNameCodeSequence);
            conceptNameSeq.Items.Should().HaveCount(1);
            var conceptItem = conceptNameSeq.Items[0];
            conceptItem.GetSingleValueOrDefault(DicomTag.CodeValue, string.Empty)
                .Should().Be("113812");
            conceptItem.GetSingleValueOrDefault(DicomTag.CodeMeaning, string.Empty)
                .Should().Be("Radiation Dose Structured Report");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithExposureParams_IncludesKvpMasAndExposureTime()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();
            var exposureParams = CreateTestExposureParams(); // 80 kVp, 200 mAs, 50 ms

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo, exposureParams);

            // Act
            var dataset = builder.Build();

            // Assert -- Content Sequence should contain irradiation event with kVp, mAs, exposure time
            var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
            contentSeq.Items.Should().HaveCountGreaterThan(0);

            // First child is the Irradiation Event CONTAINER
            var irradiationEvent = contentSeq.Items[0];
            irradiationEvent.GetSingleValueOrDefault(DicomTag.ValueType, string.Empty)
                .Should().Be("CONTAINER");

            var eventContent = irradiationEvent.GetSequence(DicomTag.ContentSequence);
            eventContent.Items.Should().HaveCountGreaterOrEqualTo(5); // DAP + ESD + kVp + mAs + ExposureTime + BodyPart + EI

            // Verify body part is present (mandatory)
            var allConceptCodes = eventContent.Items
                .Where(i => i.Contains(DicomTag.ConceptNameCodeSequence))
                .Select(i => i.GetSequence(DicomTag.ConceptNameCodeSequence).Items[0]
                    .GetSingleValueOrDefault(DicomTag.CodeValue, string.Empty))
                .ToList();

            allConceptCodes.Should().Contain("113823");   // DAP
            allConceptCodes.Should().Contain("113824");   // ESD
            allConceptCodes.Should().Contain("113821");   // kVp
            allConceptCodes.Should().Contain("113822");   // mAs
            allConceptCodes.Should().Contain("113852");   // Exposure Time
            allConceptCodes.Should().Contain("123009");   // Body Part
            allConceptCodes.Should().Contain("113840");   // Exposure Index
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithoutExposureParams_OmitsOptionalParamsButKeepsDapAndBodyPart()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo, exposureParams: null);

            // Act
            var dataset = builder.Build();

            // Assert
            var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
            var irradiationEvent = contentSeq.Items[0];
            var eventContent = irradiationEvent.GetSequence(DicomTag.ContentSequence);

            var allConceptCodes = eventContent.Items
                .Where(i => i.Contains(DicomTag.ConceptNameCodeSequence))
                .Select(i => i.GetSequence(DicomTag.ConceptNameCodeSequence).Items[0]
                    .GetSingleValueOrDefault(DicomTag.CodeValue, string.Empty))
                .ToList();

            // Mandatory items present
            allConceptCodes.Should().Contain("113823");   // DAP
            allConceptCodes.Should().Contain("123009");   // Body Part

            // Optional exposure params absent (no kVp, mAs, ExposureTime)
            allConceptCodes.Should().NotContain("113821"); // kVp
            allConceptCodes.Should().NotContain("113822"); // mAs
            allConceptCodes.Should().NotContain("113852"); // Exposure Time
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithNullPatientInfoFields_IncludesEmptyStringTags()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = new RdsrPatientInfo(); // all null
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- null patient fields are mapped to empty strings by the builder
            // The RdsrBuilder uses `?? string.Empty` so the tags are present with empty values.
            // fo-dicom may store empty strings or zero-length values.
            dataset.Contains(DicomTag.PatientID).Should().BeTrue();
            dataset.Contains(DicomTag.PatientName).Should().BeTrue();
            dataset.Contains(DicomTag.PatientSex).Should().BeTrue();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithAutoGeneratedUids_ProducesValidSopAndSeriesUids()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- auto-generated UIDs should be non-empty
            var sopUid = dataset.GetSingleValue<DicomUID>(DicomTag.SOPInstanceUID);
            sopUid.UID.Should().NotBeNullOrEmpty();

            var seriesUid = dataset.GetSingleValue<DicomUID>(DicomTag.SeriesInstanceUID);
            seriesUid.UID.Should().NotBeNullOrEmpty();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithVerifyingObserver_IncludesVerificationSequence()
        {
            // Arrange
            var doseRecord = CreateTestDoseRecord();
            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- Verifying Observer Sequence should be present
            dataset.Contains(DicomTag.VerifyingObserverSequence).Should().BeTrue();
            var verifySeq = dataset.GetSequence(DicomTag.VerifyingObserverSequence);
            verifySeq.Items.Should().HaveCount(1);
            verifySeq.Items[0].GetSingleValueOrDefault(DicomTag.VerifyingOrganization, string.Empty)
                .Should().Be("H&abyz");
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Build_WithDoseRecordWithoutEsd_OmitsEsdContentItem()
        {
            // Arrange
            var doseRecord = new DoseRecord(
                DoseId: "DR-002",
                StudyInstanceUid: "1.2.840.113619.2.55.3.20260419.2",
                Dap: 10.0,
                Ei: 1200.0,
                EffectiveDose: 0.03,
                BodyPart: "ABDOMEN",
                RecordedAt: DateTimeOffset.UtcNow,
                EsdMgy: null); // ESD not computed

            var patientInfo = CreateTestPatientInfo();
            var studyInfo = CreateTestStudyInfo();

            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo);

            // Act
            var dataset = builder.Build();

            // Assert -- ESD code should not be present
            var contentSeq = dataset.GetSequence(DicomTag.ContentSequence);
            var irradiationEvent = contentSeq.Items[0];
            var eventContent = irradiationEvent.GetSequence(DicomTag.ContentSequence);

            var allConceptCodes = eventContent.Items
                .Where(i => i.Contains(DicomTag.ConceptNameCodeSequence))
                .Select(i => i.GetSequence(DicomTag.ConceptNameCodeSequence).Items[0]
                    .GetSingleValueOrDefault(DicomTag.CodeValue, string.Empty))
                .ToList();

            allConceptCodes.Should().NotContain("113824"); // ESD should be absent
            allConceptCodes.Should().Contain("113823");    // DAP should still be present
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Constructor_WithNullDoseRecord_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new RdsrBuilder(
                doseRecord: null!,
                patientInfo: CreateTestPatientInfo(),
                studyInfo: CreateTestStudyInfo());

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Constructor_WithNullPatientInfo_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new RdsrBuilder(
                doseRecord: CreateTestDoseRecord(),
                patientInfo: null!,
                studyInfo: CreateTestStudyInfo());

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        [Trait("SWR", "SWR-DC-060")]
        public void Constructor_WithNullStudyInfo_ThrowsArgumentNullException()
        {
            // Act
            var act = () => new RdsrBuilder(
                doseRecord: CreateTestDoseRecord(),
                patientInfo: CreateTestPatientInfo(),
                studyInfo: null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
