using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-WL-010")]
public sealed class WorklistServiceCoverageTests
{
    private readonly IWorklistRepository _worklistRepository;
    private readonly IPatientService _patientService;
    private readonly WorklistService _sut;

    public WorklistServiceCoverageTests()
    {
        _worklistRepository = Substitute.For<IWorklistRepository>();
        _patientService = Substitute.For<IPatientService>();
        _sut = new WorklistService(_worklistRepository, _patientService);
    }

    // ── Constructor guards ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullWorklistRepository_ThrowsArgumentNullException()
    {
        var act = () => new WorklistService(null!, _patientService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("worklistRepository");
    }

    [Fact]
    public void Constructor_NullPatientService_ThrowsArgumentNullException()
    {
        var act = () => new WorklistService(_worklistRepository, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("patientService");
    }

    // ── PollAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task PollAsync_DelegatesToRepository()
    {
        var items = new List<WorklistItem>();
        _worklistRepository.QueryTodayAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<WorklistItem>>(items));

        var result = await _sut.PollAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PollAsync_RepositoryFails_ReturnsFailure()
    {
        _worklistRepository.QueryTodayAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<WorklistItem>>(ErrorCode.DicomConnectionFailed, "MWL failed"));

        var result = await _sut.PollAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomConnectionFailed);
    }

    // ── ImportFromMwlAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ImportFromMwlAsync_NullItem_ThrowsArgumentNullException()
    {
        var act = () => _sut.ImportFromMwlAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("item");
    }

    [Fact]
    public async Task ImportFromMwlAsync_PatientAlreadyExists_ReturnsExisting()
    {
        var existingPatient = new PatientRecord(
            "P001", "Doe^John", null, null, false, DateTimeOffset.UtcNow, "test");
        _patientService.GetByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.Success<PatientRecord?>(existingPatient));

        var item = new WorklistItem("ACC-001", "P001", "Doe^John", null, "CHEST", null);
        var result = await _sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task ImportFromMwlAsync_PatientNotFound_RegistersNew()
    {
        _patientService.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord?>(ErrorCode.NotFound, "Not found"));

        var newPatient = new PatientRecord(
            "P002", "Smith^Jane", null, null, false, DateTimeOffset.UtcNow, "WORKLIST");
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newPatient));

        var item = new WorklistItem("ACC-002", "P002", "Smith^Jane", null, "ABDOMEN", null);
        var result = await _sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        await _patientService.Received(1).RegisterAsync(
            Arg.Is<PatientRecord>(p => p.PatientId == "P002" && !p.IsEmergency),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportFromMwlAsync_GetByIdFails_RegistersNew()
    {
        _patientService.GetByIdAsync("P003", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord?>(ErrorCode.DatabaseError, "DB error"));

        var newPatient = new PatientRecord(
            "P003", "Test^Patient", null, null, false, DateTimeOffset.UtcNow, "WORKLIST");
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newPatient));

        var item = new WorklistItem("ACC-003", "P003", "Test^Patient", null, "CHEST", null);
        var result = await _sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ImportFromMwlAsync_RegistrationFails_ReturnsFailure()
    {
        _patientService.GetByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord?>(ErrorCode.NotFound, "Not found"));
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "Registration failed"));

        var item = new WorklistItem("ACC-004", "P004", "Fail^Patient", null, "CHEST", null);
        var result = await _sut.ImportFromMwlAsync(item);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── CreateEmergencyPatientAsync ────────────────────────────────────────────

    [Fact]
    public async Task CreateEmergencyPatient_NullOperatorId_ThrowsArgumentNullException()
    {
        var act = () => _sut.CreateEmergencyPatientAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("operatorId");
    }

    [Fact]
    public async Task CreateEmergencyPatient_RegistersWithCorrectProperties()
    {
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.CreateEmergencyPatientAsync("op-001");

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().StartWith("EMRG-");
        result.Value.Name.Should().Be("Emergency^Patient");
        result.Value.IsEmergency.Should().BeTrue();
        result.Value.CreatedBy.Should().Be("op-001");
    }

    [Fact]
    public async Task CreateEmergencyPatient_GeneratesTimestampBasedId()
    {
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.CreateEmergencyPatientAsync("op-001");

        result.Value.PatientId.Should().StartWith("EMRG-");
        result.Value.PatientId.Length.Should().BeGreaterThan("EMRG-".Length);
    }

    [Fact]
    public async Task CreateEmergencyPatient_RegistrationFails_ReturnsFailure()
    {
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "DB error"));

        var result = await _sut.CreateEmergencyPatientAsync("op-002");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }
}
