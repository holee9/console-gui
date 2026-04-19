// Copyright (c) H&abyz. All rights reserved.

using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
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
/// Tests for <see cref="DicomService.SendRdsrAsync"/> covering parameter validation,
/// RDSR build + C-STORE flow, and error handling.
/// SWR-DC-062~065.
/// </summary>
public sealed class RdsrTransmissionTests
{
    private static DicomService CreateService(
        DicomOptions? options = null,
        IDicomClient? mockClient = null)
    {
        var opts = Options.Create(options ?? new DicomOptions
        {
            LocalAeTitle = "HNVUE",
            PacsHost = "127.0.0.1",
            PacsPort = 104,
        });

        var service = mockClient is not null
            ? new TestableDicomService(opts, mockClient)
            : new DicomService(opts, NullLogger<DicomService>.Instance);

        return service;
    }

    private static DoseRecord CreateTestDoseRecord() =>
        new(
            DoseId: "DOSE-RDSR-001",
            StudyInstanceUid: "1.2.3.4.5.6.7.8",
            Dap: 5.0,
            Ei: 1200.0,
            EffectiveDose: 0.1,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "PAT-001",
            DapMgyCm2: 5.0,
            FieldAreaCm2: 400.0,
            EsdMgy: 0.5);

    private static RdsrPatientInfo CreateTestPatientInfo() =>
        new(PatientId: "PAT-001", PatientName: "Doe^John", PatientBirthDate: "19800101", PatientSex: "M");

    private static RdsrStudyInfo CreateTestStudyInfo() =>
        new(StudyInstanceUid: "1.2.3.4.5.6.7.8", StudyDate: "20260419", StudyTime: "120000");

    // ── Parameter Validation ─────────────────────────────────────────────────

    [Fact]
    public async Task SendRdsrAsync_NullDoseRecord_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var act = async () => await service.SendRdsrAsync(
            null!, CreateTestPatientInfo(), CreateTestStudyInfo(), "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("doseRecord");
    }

    [Fact]
    public async Task SendRdsrAsync_NullPatientInfo_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var act = async () => await service.SendRdsrAsync(
            CreateTestDoseRecord(), null!, CreateTestStudyInfo(), "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientInfo");
    }

    [Fact]
    public async Task SendRdsrAsync_NullStudyInfo_ThrowsArgumentNullException()
    {
        var service = CreateService();
        var act = async () => await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), null!, "PACS");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInfo");
    }

    [Fact]
    public async Task SendRdsrAsync_EmptyAeTitle_ReturnsStoreFailed()
    {
        var service = CreateService();
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task SendRdsrAsync_NullAeTitle_ReturnsStoreFailed()
    {
        var service = CreateService();
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── Successful transmission (mock client) ────────────────────────────────

    [Fact]
    public async Task SendRdsrAsync_WithMockClient_ReturnsSuccess()
    {
        // Arrange: mock client that simulates successful C-STORE
        var mockClient = Substitute.For<IDicomClient>();
        mockClient.When(c => c.SendAsync(Arg.Any<CancellationToken>()))
            .Do(_ => { }); // no-op: success by default

        var service = CreateService(mockClient: mockClient);
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            "PACS");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendRdsrAsync_WithExposureParams_ReturnsSuccess()
    {
        // Arrange: mock client that simulates successful C-STORE
        var mockClient = Substitute.For<IDicomClient>();
        mockClient.When(c => c.SendAsync(Arg.Any<CancellationToken>()))
            .Do(_ => { });

        var service = CreateService(mockClient: mockClient);
        var exposureParams = new RdsrExposureParams(Kvp: 80.0, Mas: 200.0, ExposureTimeMs: 150.0);
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            "PACS", exposureParams);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendRdsrAsync_WithNullExposureParams_ReturnsSuccess()
    {
        // Arrange: mock client that simulates successful C-STORE
        var mockClient = Substitute.For<IDicomClient>();
        mockClient.When(c => c.SendAsync(Arg.Any<CancellationToken>()))
            .Do(_ => { });

        var service = CreateService(mockClient: mockClient);
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            "PACS", exposureParams: null);

        result.IsSuccess.Should().BeTrue();
    }

    // ── Error handling ───────────────────────────────────────────────────────

    [Fact]
    public async Task SendRdsrAsync_NetworkException_ReturnsConnectionFailed()
    {
        var mockClient = Substitute.For<IDicomClient>();
        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new DicomNetworkException("Connection refused"));

        var service = CreateService(mockClient: mockClient);
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    [Fact]
    public async Task SendRdsrAsync_Cancelled_ReturnsOperationCancelled()
    {
        var mockClient = Substitute.For<IDicomClient>();
        mockClient.SendAsync(Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new OperationCanceledException());

        var service = CreateService(mockClient: mockClient);
        var result = await service.SendRdsrAsync(
            CreateTestDoseRecord(), CreateTestPatientInfo(), CreateTestStudyInfo(),
            "PACS");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    // ── Testable subclass for client injection ───────────────────────────────

    /// <summary>
    /// Testable subclass that allows injecting a mock DICOM client.
    /// </summary>
    private sealed class TestableDicomService : DicomService
    {
        private readonly IDicomClient _mockClient;

        public TestableDicomService(IOptions<DicomOptions> options, IDicomClient mockClient)
            : base(options, NullLogger<DicomService>.Instance)
        {
            _mockClient = mockClient;
        }

        internal override IDicomClient CreateClient(string host, int port, string callingAeTitle, string calledAeTitle)
            => _mockClient;
    }
}
