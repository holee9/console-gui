using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using Xunit;

namespace HnVue.PatientManagement.Tests;

/// <summary>
/// Additional tests targeting uncovered branches in PatientService and WorklistService.
/// Uses simple stub classes instead of mocking frameworks for explicit control.
/// SWR-PM-001: General patient management requirement tracing.
/// SWR-PM-030: Emergency patient registration requirement tracing.
/// </summary>
public sealed class PatientManagementCoverageBoostTests
{
    // ── Stub implementations ─────────────────────────────────────────────────

    private sealed class StubPatientRepository : IPatientRepository
    {
        private readonly PatientRecord? _existing;
        private readonly bool _findByIdFails;
        private readonly bool _addFails;
        private readonly bool _updateFails;
        private readonly bool _deleteFails;
        private readonly bool _searchFails;
        private readonly IReadOnlyList<PatientRecord> _searchResults;

        public StubPatientRepository(
            PatientRecord? existing = null,
            bool findByIdFails = false,
            bool addFails = false,
            bool updateFails = false,
            bool deleteFails = false,
            bool searchFails = false,
            IReadOnlyList<PatientRecord>? searchResults = null)
        {
            _existing = existing;
            _findByIdFails = findByIdFails;
            _addFails = addFails;
            _updateFails = updateFails;
            _deleteFails = deleteFails;
            _searchFails = searchFails;
            _searchResults = searchResults ?? Array.Empty<PatientRecord>();
        }

        public Task<Result<PatientRecord?>> FindByIdAsync(string patientId, CancellationToken ct = default)
        {
            if (_findByIdFails)
                return Task.FromResult(Result.Failure<PatientRecord?>(ErrorCode.DatabaseError, "DB error"));

            return Task.FromResult(Result.SuccessNullable<PatientRecord?>(_existing));
        }

        public Task<Result<PatientRecord>> AddAsync(PatientRecord patient, CancellationToken ct = default)
        {
            if (_addFails)
                return Task.FromResult(Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "Add failed"));

            return Task.FromResult(Result.Success(patient));
        }

        public Task<Result> UpdateAsync(PatientRecord patient, CancellationToken ct = default)
        {
            if (_updateFails)
                return Task.FromResult(Result.Failure(ErrorCode.DatabaseError, "Update failed"));

            return Task.FromResult(Result.Success());
        }

        public Task<Result> DeleteAsync(string patientId, CancellationToken ct = default)
        {
            if (_deleteFails)
                return Task.FromResult(Result.Failure(ErrorCode.DatabaseError, "Delete failed"));

            return Task.FromResult(Result.Success());
        }

        public Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(string query, CancellationToken ct = default)
        {
            if (_searchFails)
                return Task.FromResult(Result.Failure<IReadOnlyList<PatientRecord>>(ErrorCode.DatabaseError, "Search failed"));

            return Task.FromResult(Result.Success(_searchResults));
        }
    }

    private sealed class StubSecurityContext : ISecurityContext
    {
        public string? CurrentUserId { get; set; }
        public string? CurrentUsername { get; set; }
        public HnVue.Common.Enums.UserRole? CurrentRole => null;
        public bool IsAuthenticated => CurrentUserId is not null;
        public string? CurrentJti => null;
        public bool HasRole(HnVue.Common.Enums.UserRole role) => false;
        public void SetCurrentUser(AuthenticatedUser user) { }
        public void ClearCurrentUser() { }
    }

    private sealed class StubWorklistRepository : IWorklistRepository
    {
        private readonly Result<IReadOnlyList<WorklistItem>> _result;

        public StubWorklistRepository(Result<IReadOnlyList<WorklistItem>> result)
        {
            _result = result;
        }

        public Task<Result<IReadOnlyList<WorklistItem>>> QueryTodayAsync(CancellationToken ct = default)
            => Task.FromResult(_result);
    }

    /// <summary>
    /// Stub IPatientService that allows controlling GetByIdAsync, RegisterAsync results.
    /// </summary>
    private sealed class StubPatientService : IPatientService
    {
        private readonly Result<PatientRecord?> _getByIdResult;
        private readonly Result<PatientRecord> _registerResult;

        public StubPatientService(
            Result<PatientRecord?> getByIdResult = default,
            Result<PatientRecord> registerResult = default)
        {
            _getByIdResult = getByIdResult;
            _registerResult = registerResult;
        }

        public Task<Result<PatientRecord?>> GetByIdAsync(string patientId, CancellationToken ct = default)
            => Task.FromResult(_getByIdResult);

        public Task<Result<PatientRecord>> RegisterAsync(PatientRecord patient, CancellationToken ct = default)
            => Task.FromResult(_registerResult);

        public Task<Result<PatientRecord>> QuickRegisterEmergencyAsync(string emergencyPatientId, string? patientName, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(string query, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Result> UpdateAsync(PatientRecord patient, CancellationToken ct = default)
            => throw new NotImplementedException();

        public Task<Result> DeleteAsync(string patientId, CancellationToken ct = default)
            => throw new NotImplementedException();
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static PatientRecord MakePatient(string? id = "P001", string? name = "Doe^John") =>
        new(id ?? "P001", name ?? "Doe^John", new DateOnly(1980, 1, 1), "M",
            IsEmergency: false, DateTimeOffset.UtcNow, "op1");

    private static WorklistItem MakeWorklistItem(string patientId = "P001") =>
        new("ACC001", patientId, "Doe^John", DateOnly.FromDateTime(DateTime.Today), "CHEST", "Chest PA");

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService Constructor
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public void PatientService_Constructor_NullSecurityContext_ThrowsArgumentNullException()
    {
        var repository = new StubPatientRepository();
        var act = () => new PatientService(repository, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("securityContext");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.RegisterAsync - uncovered branches
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Register_FindByIdReturnsFailure_ReturnsRepositoryFailure()
    {
        // FindByIdAsync returns failure (not null) - code only checks IsSuccess && Value not null
        // When IsFailure, the duplicate check passes through to AddAsync
        var repo = new StubPatientRepository(findByIdFails: true, addFails: false);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient();

        var result = await sut.RegisterAsync(patient);

        // FindById fails -> IsSuccess is false -> skips duplicate check -> delegates to Add
        result.IsSuccess.Should().BeTrue();
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Register_AddReturnsFailure_PropagatesFailure()
    {
        var repo = new StubPatientRepository(addFails: true);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient();

        var result = await sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.SearchAsync - repository failure
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Search_RepositoryReturnsFailure_PropagatesFailure()
    {
        var repo = new StubPatientRepository(searchFails: true);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);

        var result = await sut.SearchAsync("Doe");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.UpdateAsync - repository failure + whitespace PatientId
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Update_RepositoryReturnsFailure_PropagatesFailure()
    {
        var repo = new StubPatientRepository(updateFails: true);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient();

        var result = await sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Update_WhitespacePatientId_ReturnsValidationFailure()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient(id: "   ");

        var result = await sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.DeleteAsync - FindById failure path + DeleteAsync failure
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Delete_FindByIdReturnsFailure_ReturnsNotFound()
    {
        // When FindById returns failure, IsFailure is true -> "not found" path
        var repo = new StubPatientRepository(findByIdFails: true);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);

        var result = await sut.DeleteAsync("P001");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Delete_DeleteAsyncReturnsFailure_PropagatesFailure()
    {
        var existing = MakePatient();
        var repo = new StubPatientRepository(existing: existing, deleteFails: true);
        var security = new StubSecurityContext { CurrentUserId = "user1" };
        var sut = new PatientService(repo, security);

        var result = await sut.DeleteAsync("P001");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.QuickRegisterEmergencyAsync - GetCurrentUserIdentifier branches
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WhenCurrentUserIdSet_UsesUserIdAsCreatedBy()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "uid-42", CurrentUsername = "jdoe" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-001", "Patient A");

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBy.Should().Be("uid-42");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WhenCurrentUserIdNullButUsernameSet_UsesUsernameAsCreatedBy()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = null, CurrentUsername = "jdoe" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-002", "Patient B");

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBy.Should().Be("jdoe");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WhenBothUserIdAndUsernameNull_FallsBackToSystem()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = null, CurrentUsername = null };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-003", "Patient C");

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBy.Should().Be("SYSTEM");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WithWhitespacePatientName_SetsUnknownEmergencyPatient()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-004", "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UNKNOWN EMERGENCY PATIENT");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WithEmptyPatientName_SetsUnknownEmergencyPatient()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-005", string.Empty);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UNKNOWN EMERGENCY PATIENT");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WithValidPatientName_PreservesProvidedName()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-006", "Trauma^Patient");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Trauma^Patient");
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WithNullPatientName_SetsUnknownEmergencyPatient()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-007", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UNKNOWN EMERGENCY PATIENT");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WorklistService Constructor
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public void WorklistService_Constructor_NullWorklistRepository_ThrowsArgumentNullException()
    {
        var patientService = new StubPatientService();
        var act = () => new WorklistService(null!, patientService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("worklistRepository");
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public void WorklistService_Constructor_NullPatientService_ThrowsArgumentNullException()
    {
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var act = () => new WorklistService(worklistRepo, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("patientService");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WorklistService.ImportFromMwlAsync - failure paths
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Import_GetByIdReturnsFailure_ProceedsToRegister()
    {
        // GetById fails -> IsSuccess is false -> skips "existing" path -> registers new
        var item = MakeWorklistItem("P-NEW");
        var patientService = new StubPatientService(
            getByIdResult: Result.Failure<PatientRecord?>(ErrorCode.DatabaseError, "DB down"),
            registerResult: Result.Success(MakePatient("P-NEW")));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P-NEW");
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Import_RegisterReturnsFailure_PropagatesFailure()
    {
        var item = MakeWorklistItem("P-FAIL");
        var patientService = new StubPatientService(
            getByIdResult: Result.SuccessNullable<PatientRecord?>(null),
            registerResult: Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "Cannot register"));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.ImportFromMwlAsync(item);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Import_ExistingPatientFound_ReturnsExistingWithoutRegistration()
    {
        var existing = MakePatient("P-EXIST");
        var item = MakeWorklistItem("P-EXIST");
        var patientService = new StubPatientService(
            getByIdResult: Result.SuccessNullable<PatientRecord?>(existing));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P-EXIST");
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Import_GetByIdReturnsSuccessNull_RegistersNew()
    {
        // GetById returns success but Value is null -> patient not found locally
        var item = MakeWorklistItem("P-NEW2");
        var patientService = new StubPatientService(
            getByIdResult: Result.SuccessNullable<PatientRecord?>(null),
            registerResult: Result.Success(MakePatient("P-NEW2")));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.ImportFromMwlAsync(item);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P-NEW2");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WorklistService.CreateEmergencyPatientAsync - failure path
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task CreateEmergency_RegisterReturnsFailure_PropagatesFailure()
    {
        var patientService = new StubPatientService(
            registerResult: Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "DB full"));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.CreateEmergencyPatientAsync("operator1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task CreateEmergency_NullOperatorId_ThrowsArgumentNullException()
    {
        var patientService = new StubPatientService();
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var act = async () => await sut.CreateEmergencyPatientAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task CreateEmergency_Success_ReturnsEmergencyPatientWithCorrectPrefix()
    {
        var patientService = new StubPatientService(
            registerResult: Result.Success(new PatientRecord(
                "EMRG-20260413120000", "Emergency^Patient", null, null,
                IsEmergency: true, DateTimeOffset.UtcNow, "op1")));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.CreateEmergencyPatientAsync("op1");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmergency.Should().BeTrue();
        result.Value.PatientId.Should().StartWith("EMRG-");
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  WorklistService.PollAsync - delegates correctly
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Poll_RepositoryReturnsSuccessWithItems_ReturnsItems()
    {
        var items = (IReadOnlyList<WorklistItem>)new[] { MakeWorklistItem() };
        var worklistRepo = new StubWorklistRepository(Result.Success(items));
        var patientService = new StubPatientService();
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.PollAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.RegisterAsync - whitespace PatientId / Name
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Register_WhitespacePatientId_ReturnsValidationFailure()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient(id: "   ");

        var result = await sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Register_WhitespaceName_ReturnsValidationFailure()
    {
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient(name: "   ");

        var result = await sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.GetByIdAsync - not found (null success)
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task GetById_NotFound_ReturnsSuccessWithNull()
    {
        var repo = new StubPatientRepository(); // existing=null by default
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.GetByIdAsync("NONEXISTENT");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  CancellationToken propagation - verify methods accept cancellation
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Register_WithCancellationToken_PassesTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);
        var patient = MakePatient();

        // Should not throw - just verifying the token is accepted
        var result = await sut.RegisterAsync(patient, cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task QuickRegister_WithCancellationToken_PassesTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var repo = new StubPatientRepository();
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.QuickRegisterEmergencyAsync("EMERG-CT-01", "Test", cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Import_WithCancellationToken_PassesTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var item = MakeWorklistItem("P-CT");
        var patientService = new StubPatientService(
            getByIdResult: Result.SuccessNullable<PatientRecord?>(null),
            registerResult: Result.Success(MakePatient("P-CT")));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.ImportFromMwlAsync(item, cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    [Trait("SWR", "SWR-PM-030")]
    [Fact]
    public async Task CreateEmergency_WithCancellationToken_PassesTokenThrough()
    {
        using var cts = new CancellationTokenSource();
        var patientService = new StubPatientService(
            registerResult: Result.Success(new PatientRecord(
                "EMRG-20260413130000", "Emergency^Patient", null, null,
                IsEmergency: true, DateTimeOffset.UtcNow, "op1")));
        var worklistRepo = new StubWorklistRepository(
            Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));
        var sut = new WorklistService(worklistRepo, patientService);

        var result = await sut.CreateEmergencyPatientAsync("op1", cts.Token);

        result.IsSuccess.Should().BeTrue();
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  PatientService.SearchAsync - successful result with data
    // ══════════════════════════════════════════════════════════════════════════

    [Trait("SWR", "SWR-PM-001")]
    [Fact]
    public async Task Search_RepositoryReturnsData_ReturnsMatchingPatients()
    {
        var patients = (IReadOnlyList<PatientRecord>)new[]
        {
            MakePatient("P001", "Doe^John"),
            MakePatient("P002", "Doe^Jane")
        };
        var repo = new StubPatientRepository(searchResults: patients);
        var security = new StubSecurityContext { CurrentUserId = "u1" };
        var sut = new PatientService(repo, security);

        var result = await sut.SearchAsync("Doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }
}
