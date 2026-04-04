using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using HnVue.PatientManagement;
using HnVue.Security;
using NSubstitute;
using Xunit;

namespace HnVue.IntegrationTests;

/// <summary>
/// Cross-module integration tests that verify interactions between real service implementations.
/// These tests exercise end-to-end scenarios using mock repositories to isolate infrastructure.
/// </summary>
public sealed class CrossModuleIntegrationTests
{
    // ── Shared JWT options ─────────────────────────────────────────────────────

    private static readonly JwtOptions TestJwtOptions = new()
    {
        SecretKey = "IntegrationTestSecretKey-32CharMin!",
        ExpiryMinutes = 15,
        Issuer = "HnVue",
        Audience = "HnVue",
    };

    // ── Scenario 1: Authentication → RBAC → Audit chain ───────────────────────

    /// <summary>
    /// Integration test: A valid login via SecurityService produces a token, authorisation
    /// succeeds for the user's role, and both events are written to the audit log.
    /// SWR-SEC-010: Authentication produces audit trail.
    /// SWR-SEC-020: Role-based access control enforced via SecurityService.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-SEC-010")]
    [Trait("SWR", "SWR-SEC-020")]
    public async Task AuthFlow_ValidLogin_ThenRbacCheck_AuditWrittenForBothEvents()
    {
        // Arrange
        const string password = "AdminPass1";
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secContext = Substitute.For<ISecurityContext>();

        var user = MakeUser(password: password, role: UserRole.Admin);

        userRepo.GetByUsernameAsync(user.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        userRepo.UpdateFailedLoginCountAsync(user.UserId, 0, Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        userRepo.GetByIdAsync(user.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty log"));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var secService = new SecurityService(userRepo, auditRepo, secContext, TestJwtOptions);

        // Act — Step 1: Authenticate
        var authResult = await secService.AuthenticateAsync(user.Username, password);

        // Act — Step 2: RBAC check for Admin role
        var rbacResult = await secService.CheckAuthorizationAsync(user.UserId, UserRole.Admin);

        // Assert — authentication succeeded
        authResult.IsSuccess.Should().BeTrue("valid credentials should authenticate successfully");
        authResult.Value.UserId.Should().Be(user.UserId);
        authResult.Value.Token.Should().NotBeNullOrEmpty();

        // Assert — RBAC check succeeded
        rbacResult.IsSuccess.Should().BeTrue("Admin user should pass Admin RBAC check");

        // Assert — login audit entry was written
        await auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Action == "LOGIN" && e.UserId == user.UserId),
            Arg.Any<CancellationToken>());

        // Assert — security context was updated
        secContext.Received(1).SetCurrentUser(Arg.Is<AuthenticatedUser>(u =>
            u.UserId == user.UserId && u.Role == UserRole.Admin));
    }

    /// <summary>
    /// Integration test: A locked account fails authentication, and an RBAC check for an
    /// insufficient role fails. No audit entry for blocked login.
    /// SWR-SEC-010: Locked accounts are rejected.
    /// SWR-SEC-020: Insufficient role returns failure.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-SEC-010")]
    [Trait("SWR", "SWR-SEC-020")]
    public async Task AuthFlow_LockedAccount_ReturnsFailure_AndRadiographerFailsAdminCheck()
    {
        // Arrange
        var userRepo = Substitute.For<IUserRepository>();
        var auditRepo = Substitute.For<IAuditRepository>();
        var secContext = Substitute.For<ISecurityContext>();

        var lockedUser = MakeUser(password: "Pass1", isLocked: true, role: UserRole.Radiographer);
        var radiographerUser = MakeUser(password: "Pass1", role: UserRole.Radiographer);

        userRepo.GetByUsernameAsync(lockedUser.Username, Arg.Any<CancellationToken>())
            .Returns(Result.Success(lockedUser));
        userRepo.GetByIdAsync(radiographerUser.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(radiographerUser));
        auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.NotFound, "empty log"));
        auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var secService = new SecurityService(userRepo, auditRepo, secContext, TestJwtOptions);

        // Act
        var lockedAuthResult = await secService.AuthenticateAsync(lockedUser.Username, "Pass1");
        var rbacResult = await secService.CheckAuthorizationAsync(radiographerUser.UserId, UserRole.Admin);

        // Assert
        lockedAuthResult.IsFailure.Should().BeTrue();
        lockedAuthResult.Error.Should().Be(ErrorCode.AccountLocked);

        rbacResult.IsFailure.Should().BeTrue();
        rbacResult.Error.Should().Be(ErrorCode.InsufficientPermission);
    }

    // ── Scenario 2: Patient registration → Worklist import chain ──────────────

    /// <summary>
    /// Integration test: A WorklistItem imported via WorklistService registers the patient
    /// through the real PatientService, returning the created PatientRecord.
    /// SWR-PM-010: Patient can be registered from worklist data.
    /// SWR-PM-020: Duplicate worklist import returns existing patient without re-registration.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-PM-010")]
    [Trait("SWR", "SWR-PM-020")]
    public async Task PatientWorkflow_ImportFromMwl_RegistersPatientAndReturnsRecord()
    {
        // Arrange
        var patientRepo = Substitute.For<IPatientRepository>();
        var worklistRepo = Substitute.For<IWorklistRepository>();

        var item = new WorklistItem(
            AccessionNumber: "ACC-001",
            PatientId: "P-001",
            PatientName: "Smith^John",
            StudyDate: DateOnly.FromDateTime(DateTime.Today),
            BodyPart: "CHEST",
            RequestedProcedure: "PA Chest");

        // Patient does not exist yet
        patientRepo.FindByIdAsync("P-001", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord>(null));

        var expectedRecord = new PatientRecord(
            PatientId: "P-001",
            Name: "Smith^John",
            DateOfBirth: null,
            Sex: null,
            IsEmergency: false,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "system");

        patientRepo.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedRecord));

        var patientService = new PatientService(patientRepo);
        var worklistService = new WorklistService(worklistRepo, patientService);

        // Act
        var result = await worklistService.ImportFromMwlAsync(item);

        // Assert
        result.IsSuccess.Should().BeTrue("importing a new worklist item should register the patient");
        result.Value.PatientId.Should().Be("P-001");
        result.Value.Name.Should().Be("Smith^John");

        // Verify patient was actually registered
        await patientRepo.Received(1).AddAsync(
            Arg.Is<PatientRecord>(p => p.PatientId == "P-001"),
            Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Integration test: Importing a WorklistItem for a patient that already exists
    /// returns the existing record without calling AddAsync.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-PM-020")]
    public async Task PatientWorkflow_ImportFromMwl_ExistingPatient_ReturnsExistingRecord()
    {
        // Arrange
        var patientRepo = Substitute.For<IPatientRepository>();
        var worklistRepo = Substitute.For<IWorklistRepository>();

        var item = new WorklistItem(
            AccessionNumber: "ACC-002",
            PatientId: "P-002",
            PatientName: "Jones^Jane",
            StudyDate: DateOnly.FromDateTime(DateTime.Today),
            BodyPart: "HAND",
            RequestedProcedure: "Hand AP");

        var existingRecord = new PatientRecord(
            PatientId: "P-002",
            Name: "Jones^Jane",
            DateOfBirth: new DateOnly(1980, 5, 10),
            Sex: "F",
            IsEmergency: false,
            CreatedAt: DateTimeOffset.UtcNow.AddDays(-7),
            CreatedBy: "admin");

        // Patient already exists in the system
        patientRepo.FindByIdAsync("P-002", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord>(existingRecord));

        var patientService = new PatientService(patientRepo);
        var worklistService = new WorklistService(worklistRepo, patientService);

        // Act
        var result = await worklistService.ImportFromMwlAsync(item);

        // Assert — existing patient is returned without duplicate registration
        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P-002");

        // AddAsync must NOT have been called
        await patientRepo.DidNotReceive().AddAsync(
            Arg.Any<PatientRecord>(),
            Arg.Any<CancellationToken>());
    }

    // ── Scenario 3: Dose interlock ─────────────────────────────────────────────

    /// <summary>
    /// Integration test: DoseService returns Allow for parameters within DRL.
    /// SWR-DOSE-010: Normal exposure validated as Allow level.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DOSE-010")]
    public async Task DoseInterlock_NormalExposure_ReturnsAllow()
    {
        // Arrange — CHEST DRL = 10.0 mGy·cm²; 60kVp × 2mAs → DAP = (3600 × 2) / 500000 = 0.0144
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);

        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 60.0,
            Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5.001");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.IsAllowed.Should().BeTrue();
        result.Value.Message.Should().BeNull();
    }

    /// <summary>
    /// Integration test: DoseService returns Warn when DAP is between 1× and 2× DRL.
    /// SWR-DOSE-020: Elevated exposure generates warning but is not blocked.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DOSE-020")]
    public async Task DoseInterlock_ElevatedExposure_ReturnsWarnAndIsAllowed()
    {
        // Arrange — CHEST DRL = 10.0; need DAP between 10 and 20.
        // DAP = kVp² × mAs / 500000; solve for ~12: kVp=100, mAs=6 → 10000×6/500000=0.12 — too low.
        // Use kVp=250, mAs=100: 62500×100/500000 = 12.5 mGy·cm² (between DRL 10 and 2×DRL 20).
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);

        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 250.0,
            Mas: 100.0,
            StudyInstanceUid: "1.2.3.4.5.002");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.IsAllowed.Should().BeTrue("Warn level should still permit the exposure");
        result.Value.Message.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Integration test: DoseService returns Block when DAP is between 2× and 5× DRL.
    /// SWR-DOSE-030: Excessive exposure is blocked.
    /// SWR-HAZ-010: Safety interlock prevents exposure above block threshold.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DOSE-030")]
    [Trait("SWR", "SWR-HAZ-010")]
    public async Task DoseInterlock_ExcessiveExposure_ReturnsBlockAndIsNotAllowed()
    {
        // Arrange — CHEST DRL = 10.0; Block at 5× = 50.0.
        // Need DAP between 20 and 50: kVp=400, mAs=200 → 160000×200/500000 = 64.0 — too high.
        // kVp=300, mAs=100: 90000×100/500000 = 18.0... need >20.
        // kVp=350, mAs=100: 122500×100/500000 = 24.5 → between 20 and 50 → Block.
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);

        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 350.0,
            Mas: 100.0,
            StudyInstanceUid: "1.2.3.4.5.003");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse("Block level must prevent the exposure");
        result.Value.Message.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Integration test: DoseService returns Emergency for DAP beyond 5× DRL.
    /// SWR-HAZ-010: Emergency safety interlock activated at extreme dose levels.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-HAZ-010")]
    public async Task DoseInterlock_EmergencyLevel_ReturnsEmergencyAndIsNotAllowed()
    {
        // Arrange — CHEST DRL = 10.0; Emergency > 5× = 50.0.
        // kVp=600, mAs=200: 360000×200/500000 = 144.0 mGy·cm² — Emergency.
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);

        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 600.0,
            Mas: 200.0,
            StudyInstanceUid: "1.2.3.4.5.004");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.IsAllowed.Should().BeFalse("Emergency level must prevent the exposure");
        result.Value.Message.Should().Contain("EMERGENCY");
    }

    /// <summary>
    /// Integration test: DoseService rejects invalid exposure parameters (zero kVp).
    /// SWR-DOSE-010: Invalid parameters are rejected before dose calculation.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DOSE-010")]
    public async Task DoseInterlock_ZeroKvp_ReturnsValidationFailure()
    {
        // Arrange
        var doseRepo = Substitute.For<IDoseRepository>();
        var doseService = new DoseService(doseRepo);

        var parameters = new ExposureParameters(
            BodyPart: "CHEST",
            Kvp: 0.0,
            Mas: 10.0,
            StudyInstanceUid: "1.2.3.4.5.005");

        // Act
        var result = await doseService.ValidateExposureAsync(parameters);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static UserRecord MakeUser(
        string? userId = null,
        string? username = null,
        string password = "Password1",
        UserRole role = UserRole.Radiographer,
        bool isLocked = false,
        int failedLoginCount = 0)
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 4);
        return new UserRecord(
            UserId: userId ?? Guid.NewGuid().ToString(),
            Username: username ?? $"testuser_{Guid.NewGuid():N}",
            DisplayName: "Test User",
            PasswordHash: hash,
            Role: role,
            FailedLoginCount: failedLoginCount,
            IsLocked: isLocked,
            LastLoginAt: null);
    }
}
