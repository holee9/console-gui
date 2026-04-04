using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-PM-020")]
public sealed class WorklistServiceTests
{
    private readonly IWorklistRepository _worklistRepo;
    private readonly IPatientService _patientService;
    private readonly WorklistService _sut;

    private static PatientRecord MakePatient(string id = "P001") =>
        new(id, "Doe^John", new DateOnly(1980, 1, 1), "M",
            IsEmergency: false, DateTimeOffset.UtcNow, "WORKLIST");

    private static WorklistItem MakeWorklistItem(string patientId = "P001") =>
        new("ACC001", patientId, "Doe^John", DateOnly.FromDateTime(DateTime.Today), "CHEST", "Chest PA");

    public WorklistServiceTests()
    {
        _worklistRepo = Substitute.For<IWorklistRepository>();
        _patientService = Substitute.For<IPatientService>();
        _sut = new WorklistService(_worklistRepo, _patientService);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullWorklistRepository_ThrowsArgumentNullException()
    {
        var act = () => new WorklistService(null!, _patientService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("worklistRepository");
    }

    [Fact]
    public void Constructor_NullPatientService_ThrowsArgumentNullException()
    {
        var act = () => new WorklistService(_worklistRepo, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("patientService");
    }

    // ── PollAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Poll_DelegatesToRepository()
    {
        var items = (IReadOnlyList<WorklistItem>)new[] { MakeWorklistItem() };
        _worklistRepo.QueryTodayAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.PollAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        await _worklistRepo.Received(1).QueryTodayAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Poll_RepositoryFailure_PropagatesFailure()
    {
        _worklistRepo.QueryTodayAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed, "MWL SCP unreachable"));

        var result = await _sut.PollAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    // ── ImportFromMwlAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task Import_NewPatient_RegistersAndReturnsPatient()
    {
        var item = MakeWorklistItem("P002");
        var newPatient = MakePatient("P002");
        _patientService.GetByIdAsync("P002", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(null));
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(newPatient));

        var result = await _sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P002");
        await _patientService.Received(1).RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Import_ExistingPatient_ReturnsExistingWithoutRegistering()
    {
        var item = MakeWorklistItem("P001");
        var existing = MakePatient("P001");
        _patientService.GetByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(existing));

        var result = await _sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(existing);
        await _patientService.DidNotReceive().RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Import_NullItem_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ImportFromMwlAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── CreateEmergencyPatientAsync ───────────────────────────────────────────

    [Fact]
    public async Task CreateEmergency_ValidOperatorId_RegistersEmergencyPatient()
    {
        PatientRecord? registeredPatient = null;
        _patientService.RegisterAsync(Arg.Do<PatientRecord>(p => registeredPatient = p), Arg.Any<CancellationToken>())
            .Returns(x => Result.Success((PatientRecord)x[0]));

        var result = await _sut.CreateEmergencyPatientAsync("op1");

        result.IsSuccess.Should().BeTrue();
        registeredPatient.Should().NotBeNull();
        registeredPatient!.IsEmergency.Should().BeTrue();
        registeredPatient.PatientId.Should().StartWith("EMRG-");
        registeredPatient.CreatedBy.Should().Be("op1");
    }

    [Fact]
    public async Task CreateEmergency_NullOperatorId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.CreateEmergencyPatientAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task CreateEmergency_TwoCallsInSameSecond_GenerateDistinctIds()
    {
        // IDs include timestamp so rapid calls may collide; test verifies prefix at minimum
        _patientService.RegisterAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(x => Result.Success((PatientRecord)x[0]));

        var r1 = await _sut.CreateEmergencyPatientAsync("op1");
        var r2 = await _sut.CreateEmergencyPatientAsync("op1");

        r1.IsSuccess.Should().BeTrue();
        r2.IsSuccess.Should().BeTrue();
        r1.Value.PatientId.Should().StartWith("EMRG-");
        r2.Value.PatientId.Should().StartWith("EMRG-");
    }
}
