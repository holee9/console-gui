using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// DICOM module integration tests that verify IDicomService interface contract,
/// DI registration, and error handling scenarios.
/// SWR-DICOM-015: IDicomService resolves from DI container.
/// SWR-DICOM-016: DicomService implements all IDicomService methods.
/// SWR-DICOM-017: Error handling for unreachable PACS/MWL/Printer.
/// </summary>
public sealed class DicomIntegrationTests
{
    /// <summary>
    /// Integration test: IDicomService resolves from DI container with DicomOptions.
    /// SWR-DICOM-015: DI registration for IDicomService is complete and functional.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-015")]
    public void DI_ResolveIDicomService_WithConfiguration_Succeeds()
    {
        // Arrange — build DI container similar to App.xaml.cs
        var services = new ServiceCollection();

        // Register DicomOptions with test values (mirrors App.xaml.cs:214-220)
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        services.AddSingleton(options);

        // Register ILogger (required by DicomService)
        services.AddSingleton<ILogger<DicomService>>(NullLogger<DicomService>.Instance);

        // Register IDicomService (mirrors App.xaml.cs:221)
        services.AddSingleton<IDicomService, DicomService>();

        var provider = services.BuildServiceProvider();

        // Act — resolve IDicomService
        var dicomService = provider.GetService<IDicomService>();

        // Assert — service resolved successfully
        dicomService.Should().NotBeNull("IDicomService must be resolvable from DI container");
        dicomService.Should().BeOfType<DicomService>("Resolved service should be DicomService implementation");
    }

    /// <summary>
    /// Integration test: IDicomService interface has all required methods for C-STORE, C-FIND, Print.
    /// SWR-DICOM-016: Interface contract verification for DICOM operations.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-016")]
    public void Interface_IDicomService_ContainsAllRequiredMethods()
    {
        // Arrange — create DicomService instance
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);

        // Assert — verify interface implementation
        dicomService.Should().BeAssignableTo<IDicomService>("DicomService must implement IDicomService");

        // Assert — verify all methods exist via interface
        var dicomInterface = dicomService as IDicomService;
        dicomInterface.Should().NotBeNull();

        // Method signatures (compile-time verification)
        Action<string, string, CancellationToken> storeAction =
            (file, ae, ct) => _ = dicomInterface!.StoreAsync(file, ae, ct);

        Action<WorklistQuery, CancellationToken> queryAction =
            (query, ct) => _ = dicomInterface!.QueryWorklistAsync(query, ct);

        Action<string, string, CancellationToken> printAction =
            (file, ae, ct) => _ = dicomInterface!.PrintAsync(file, ae, ct);

        Action<string, string, string, CancellationToken> commitAction =
            (uid, instance, ae, ct) => _ = dicomInterface!.RequestStorageCommitmentAsync(uid, instance, ae, ct);

        Action<string, string, CancellationToken> statusAction =
            (uid, ae, ct) => _ = dicomInterface!.GetPrintJobStatusAsync(uid, ae, ct);

        Action<DoseRecord, RdsrPatientInfo, RdsrStudyInfo, string, RdsrExposureParams?, CancellationToken> rdsrAction =
            (dose, patient, study, ae, exposure, ct) => _ = dicomInterface!.SendRdsrAsync(dose, patient, study, ae, exposure, ct);

        // If we reach here, all methods exist on the interface
        true.Should().BeTrue("All IDicomService methods are present");
    }

    /// <summary>
    /// Integration test: DicomService.StoreAsync fails gracefully for non-existent file.
    /// SWR-DICOM-017: C-STORE error handling for missing files.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task StoreAsync_NonExistentFile_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);
        var nonExistentFile = "/tmp/non_existent_dicom_file_12345.dcm";

        // Act
        var result = await dicomService.StoreAsync(nonExistentFile, "PACS_TEST");

        // Assert
        result.IsFailure.Should().BeTrue("StoreAsync should fail for non-existent file");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: DicomService.QueryWorklistAsync fails gracefully for unreachable MWL SCP.
    /// SWR-DICOM-017: C-FIND error handling for network failures.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task QueryWorklistAsync_UnreachableMwlScp_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "unreachable-host",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);
        var query = new WorklistQuery(
            PatientId: "P-001",
            DateFrom: DateOnly.FromDateTime(DateTime.Today),
            DateTo: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            AeTitle: "HNVUE_TEST");

        // Act
        var result = await dicomService.QueryWorklistAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue("QueryWorklistAsync should fail for unreachable MWL SCP");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: DicomService.PrintAsync fails gracefully for unreachable printer.
    /// SWR-DICOM-017: Print error handling for network failures.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task PrintAsync_UnreachablePrinter_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);
        var testFile = "/tmp/test_print_file.dcm";

        // Act
        var result = await dicomService.PrintAsync(testFile, "PRINTER_TEST");

        // Assert
        result.IsFailure.Should().BeTrue("PrintAsync should fail for unreachable printer");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: DicomService.RequestStorageCommitmentAsync fails gracefully for unreachable PACS.
    /// SWR-DICOM-017: N-ACTION error handling for network failures.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task RequestStorageCommitmentAsync_UnreachablePacs_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "unreachable-pacs",
            PacsPort = 11112,
            PacsAeTitle = "PACS_TEST",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);

        // Act
        var result = await dicomService.RequestStorageCommitmentAsync(
            "1.2.840.10008.5.1.4.1.1.2",  // SOP Class UID
            "1.2.3.4.5.6.7.8.9",          // SOP Instance UID
            "PACS_TEST");

        // Assert
        result.IsFailure.Should().BeTrue("RequestStorageCommitmentAsync should fail for unreachable PACS");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: DicomService.GetPrintJobStatusAsync fails gracefully for unreachable printer.
    /// SWR-DICOM-017: N-GET error handling for network failures.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task GetPrintJobStatusAsync_UnreachablePrinter_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "localhost",
            PacsPort = 11112,
            PacsAeTitle = "DCMRCV",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);

        // Act
        var result = await dicomService.GetPrintJobStatusAsync(
            "1.2.840.10008.5.1.1.40.1",  // Film Session UID
            "PRINTER_TEST");

        // Assert
        result.IsFailure.Should().BeTrue("GetPrintJobStatusAsync should fail for unreachable printer");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: DicomService.SendRdsrAsync fails gracefully for unreachable PACS.
    /// SWR-DICOM-017: RDSR C-STORE error handling for network failures.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-017")]
    public async Task SendRdsrAsync_UnreachablePacs_ReturnsFailure()
    {
        // Arrange
        var options = Options.Create(new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "unreachable-pacs",
            PacsPort = 11112,
            PacsAeTitle = "PACS_TEST",
            MwlHost = "localhost",
            MwlPort = 11113
        });
        var dicomService = new DicomService(options, NullLogger<DicomService>.Instance);

        var doseRecord = new DoseRecord(
            DoseId: Guid.NewGuid().ToString(),
            StudyInstanceUid: "1.2.3.4.5",
            Dap: 1.5,
            Ei: 200.0,
            EffectiveDose: 0.03,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "P-001");

        var patientInfo = new RdsrPatientInfo(
            PatientId: "P-001",
            PatientName: "Test^Patient",
            PatientBirthDate: "19900101",
            PatientSex: "M");

        var studyInfo = new RdsrStudyInfo(
            StudyInstanceUid: "1.2.3.4.5",
            StudyDate: "20260120",
            StudyTime: "120000",
            AccessionNumber: "ACC-001");

        // Act
        var result = await dicomService.SendRdsrAsync(doseRecord, patientInfo, studyInfo, "PACS_TEST");

        // Assert
        result.IsFailure.Should().BeTrue("SendRdsrAsync should fail for unreachable PACS");
        result.Error.Should().NotBeNull("Error should have a value");
    }

    /// <summary>
    /// Integration test: WorklistQuery with null parameters creates valid query object.
    /// SWR-DICOM-018: WorklistQuery handles optional parameters correctly.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-018")]
    public void WorklistQuery_NullParameters_CreatesValidQuery()
    {
        // Arrange & Act
        var query = new WorklistQuery(
            PatientId: null,
            DateFrom: null,
            DateTo: null,
            AeTitle: "HNVUE_TEST");

        // Assert
        query.Should().NotBeNull();
        query.PatientId.Should().BeNull("PatientId should be null");
        query.DateFrom.Should().BeNull("DateFrom should be null");
        query.DateTo.Should().BeNull("DateTo should be null");
        query.AeTitle.Should().Be("HNVUE_TEST", "AeTitle should be set");
    }

    /// <summary>
    /// Integration test: WorklistQuery with date range creates valid query object.
    /// SWR-DICOM-018: WorklistQuery handles date range correctly.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-018")]
    public void WorklistQuery_DateRange_CreatesValidQuery()
    {
        // Arrange & Act
        var today = DateOnly.FromDateTime(DateTime.Today);
        var query = new WorklistQuery(
            PatientId: "P-001",
            DateFrom: today.AddDays(-7),
            DateTo: today,
            AeTitle: "HNVUE_TEST");

        // Assert
        query.Should().NotBeNull();
        query.PatientId.Should().Be("P-001");
        query.DateFrom.Should().Be(today.AddDays(-7));
        query.DateTo.Should().Be(today);
        query.AeTitle.Should().Be("HNVUE_TEST");
    }

    /// <summary>
    /// Integration test: DicomOptions properties are accessible through DI.
    /// SWR-DICOM-019: DicomOptions provides all required network configuration.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-019")]
    public void DicomOptions_Properties_AreAccessible()
    {
        // Arrange & Act
        var options = new DicomOptions
        {
            LocalAeTitle = "HNVUE_SCU",
            PacsHost = "192.168.1.100",
            PacsPort = 11112,
            PacsAeTitle = "PACS_AE",
            MwlHost = "192.168.1.101",
            MwlPort = 11113
        };

        // Assert
        options.PacsHost.Should().Be("192.168.1.100");
        options.PacsPort.Should().Be(11112);
        options.PacsAeTitle.Should().Be("PACS_AE");
        options.LocalAeTitle.Should().Be("HNVUE_SCU");
        options.MwlHost.Should().Be("192.168.1.101");
        options.MwlPort.Should().Be(11113);
    }

    /// <summary>
    /// Integration test: RdsrPatientInfo with all parameters creates valid object.
    /// SWR-DICOM-020: RdsrPatientInfo handles all patient demographic fields.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public void RdsrPatientInfo_AllParameters_CreatesValidObject()
    {
        // Arrange & Act
        var patientInfo = new RdsrPatientInfo(
            PatientId: "P-001",
            PatientName: "Hong^Gildong",
            PatientBirthDate: "19850315",
            PatientSex: "M");

        // Assert
        patientInfo.Should().NotBeNull();
        patientInfo.PatientId.Should().Be("P-001");
        patientInfo.PatientName.Should().Be("Hong^Gildong");
        patientInfo.PatientBirthDate.Should().Be("19850315");
        patientInfo.PatientSex.Should().Be("M");
    }

    /// <summary>
    /// Integration test: RdsrStudyInfo with all parameters creates valid object.
    /// SWR-DICOM-021: RdsrStudyInfo handles all study-level fields.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-021")]
    public void RdsrStudyInfo_AllParameters_CreatesValidObject()
    {
        // Arrange & Act
        var studyInfo = new RdsrStudyInfo(
            StudyInstanceUid: "1.2.3.4.5.999",
            StudyDate: "20260120",
            StudyTime: "120000",
            AccessionNumber: "ACC-001",
            StudyId: "ST-001",
            ReferringPhysicianName: "Referring^Physician",
            RetrieveAeTitle: "HNVUE");

        // Assert
        studyInfo.Should().NotBeNull();
        studyInfo.StudyInstanceUid.Should().Be("1.2.3.4.5.999");
        studyInfo.StudyDate.Should().Be("20260120");
        studyInfo.StudyTime.Should().Be("120000");
        studyInfo.AccessionNumber.Should().Be("ACC-001");
        studyInfo.StudyId.Should().Be("ST-001");
        studyInfo.ReferringPhysicianName.Should().Be("Referring^Physician");
        studyInfo.RetrieveAeTitle.Should().Be("HNVUE");
    }

    /// <summary>
    /// Integration test: RdsrExposureParams with all parameters creates valid object.
    /// SWR-DICOM-022: RdsrExposureParams handles exposure technique parameters.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-022")]
    public void RdsrExposureParams_AllParameters_CreatesValidObject()
    {
        // Arrange & Act
        var exposureParams = new RdsrExposureParams(
            Kvp: 120.0,
            Mas: 100.0,
            ExposureTimeMs: 200);

        // Assert
        exposureParams.Should().NotBeNull();
        exposureParams.Kvp.Should().Be(120.0);
        exposureParams.Mas.Should().Be(100.0);
        exposureParams.ExposureTimeMs.Should().Be(200);
    }

    /// <summary>
    /// Integration test: DoseRecord with minimal required parameters creates valid object.
    /// SWR-DICOM-023: DoseRecord handles required dose tracking fields.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-023")]
    public void DoseRecord_MinimalParameters_CreatesValidObject()
    {
        // Arrange & Act
        var doseRecord = new DoseRecord(
            DoseId: Guid.NewGuid().ToString(),
            StudyInstanceUid: "1.2.3.4.5",
            Dap: 1.5,
            Ei: 200.0,
            EffectiveDose: 0.03,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow);

        // Assert
        doseRecord.Should().NotBeNull();
        doseRecord.DoseId.Should().NotBeNullOrEmpty();
        doseRecord.StudyInstanceUid.Should().Be("1.2.3.4.5");
        doseRecord.Dap.Should().Be(1.5);
        doseRecord.Ei.Should().Be(200.0);
        doseRecord.EffectiveDose.Should().Be(0.03);
        doseRecord.BodyPart.Should().Be("CHEST");
        doseRecord.PatientId.Should().BeNull("PatientId should be null when not specified");
    }
}
