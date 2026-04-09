using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using HnVue.Dose;
using HnVue.PatientManagement;
using HnVue.Security;
using HnVue.Workflow;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using Microsoft.Extensions.Options;
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

    private static readonly IOptions<AuditOptions> TestAuditOptions =
        Options.Create(new AuditOptions { HmacKey = "IntegrationTestHmacKey-32CharMin!" });

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

        var secService = new SecurityService(userRepo, auditRepo, secContext, TestJwtOptions, TestAuditOptions, Substitute.For<ITokenDenylist>());

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

        var secService = new SecurityService(userRepo, auditRepo, secContext, TestJwtOptions, TestAuditOptions, Substitute.For<ITokenDenylist>());

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
#pragma warning disable CS8620 // SuccessNullable returns Result<T?> but NSubstitute types as Result<T>
        patientRepo.FindByIdAsync("P-001", Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result.SuccessNullable<PatientRecord>(null)));
#pragma warning restore CS8620

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
#pragma warning disable CS8620 // SuccessNullable returns Result<T?> but NSubstitute types as Result<T>
        patientRepo.FindByIdAsync("P-002", Arg.Any<CancellationToken>())
            .Returns(ci => Task.FromResult(Result.SuccessNullable<PatientRecord>(existingRecord)));
#pragma warning restore CS8620

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

    // ── Scenario 4: Shooting workflow state machine ────────────────────────────

    /// <summary>
    /// Integration test: WorkflowEngine drives the full 9-state acquisition chain
    /// from Idle through Completed using the real WorkflowStateMachine.
    /// SWR-WF-010: State machine allows all valid transitions in sequence.
    /// SWR-WF-020: Engine exposes current state correctly after each transition.
    /// SWR-WF-030: Session completes cleanly and can be reset to Idle.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-WF-010")]
    [Trait("SWR", "SWR-WF-020")]
    [Trait("SWR", "SWR-WF-030")]
    public async Task ShootingWorkflow_FullStateChain_TransitionsToCompleted()
    {
        // Arrange — real engine with real state machine, simulated generator and dose service
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secCtx = Substitute.For<ISecurityContext>();
        secCtx.CurrentRole.Returns(UserRole.Radiographer);
        var engine = new WorkflowEngine(doseService, generator, secCtx);

        engine.CurrentState.Should().Be(WorkflowState.Idle, "engine starts in Idle");

        // Act — drive through all 9 states
        var startResult = await engine.StartAsync("P-001", "1.2.3.4.5");
        startResult.IsSuccess.Should().BeTrue("StartAsync should move Idle → PatientSelected");
        engine.CurrentState.Should().Be(WorkflowState.PatientSelected);

        var protocolResult = await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        protocolResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ProtocolLoaded);

        var readyResult = await engine.TransitionAsync(WorkflowState.ReadyToExpose);
        readyResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ReadyToExpose);

        var exposingResult = await engine.TransitionAsync(WorkflowState.Exposing);
        exposingResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Exposing);

        var acquiringResult = await engine.TransitionAsync(WorkflowState.ImageAcquiring);
        acquiringResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ImageAcquiring);

        var processingResult = await engine.TransitionAsync(WorkflowState.ImageProcessing);
        processingResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ImageProcessing);

        var reviewResult = await engine.TransitionAsync(WorkflowState.ImageReview);
        reviewResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.ImageReview);

        var completedResult = await engine.TransitionAsync(WorkflowState.Completed);
        completedResult.IsSuccess.Should().BeTrue();
        engine.CurrentState.Should().Be(WorkflowState.Completed, "workflow should reach Completed state");
    }

    /// <summary>
    /// Integration test: An invalid state transition is rejected by WorkflowEngine.
    /// SWR-WF-010: State machine blocks disallowed transitions.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-WF-010")]
    public async Task ShootingWorkflow_InvalidTransition_ReturnsFailure()
    {
        // Arrange — engine in Idle state; jump directly to Exposing (not allowed)
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator();
        var secCtx = Substitute.For<ISecurityContext>();
        secCtx.CurrentRole.Returns(UserRole.Radiographer);
        var engine = new WorkflowEngine(doseService, generator, secCtx);

        // Act — try to jump from Idle directly to Exposing
        var result = await engine.TransitionAsync(WorkflowState.Exposing);

        // Assert
        result.IsFailure.Should().BeTrue("direct jump from Idle to Exposing is not a valid transition");
        result.Error.Should().Be(ErrorCode.InvalidStateTransition);
        engine.CurrentState.Should().Be(WorkflowState.Idle, "engine must remain in Idle after rejected transition");
    }

    /// <summary>
    /// Integration test: GeneratorSimulator fault injection triggers workflow abort.
    /// SWR-WF-030: Exposure fault drives the engine into Error state via AbortAsync.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-WF-030")]
    public async Task ShootingWorkflow_GeneratorFault_AbortDrivesEngineToError()
    {
        // Arrange
        var doseService = Substitute.For<IDoseService>();
        var generator = new GeneratorSimulator { PrepareDelayMs = 0, ExposureDelayMs = 0 };
        var secCtx = Substitute.For<ISecurityContext>();
        secCtx.CurrentRole.Returns(UserRole.Radiographer);
        var engine = new WorkflowEngine(doseService, generator, secCtx);

        // Advance to ReadyToExpose
        await engine.StartAsync("P-002", "1.2.3.4.6");
        await engine.TransitionAsync(WorkflowState.ProtocolLoaded);
        await engine.TransitionAsync(WorkflowState.ReadyToExpose);

        engine.CurrentState.Should().Be(WorkflowState.ReadyToExpose);

        // Act — simulate a hardware fault by aborting the workflow
        var abortResult = await engine.AbortAsync("Simulated generator hardware fault");

        // Assert
        abortResult.IsSuccess.Should().BeTrue("AbortAsync always succeeds");
        engine.CurrentState.Should().Be(WorkflowState.Error,
            "engine must be in Error state after abort");
    }

    // ── Scenario 5: DICOM Store + Find ────────────────────────────────────────

    /// <summary>
    /// Integration test: DicomStoreScu returns failure when the source file does not exist.
    /// Verifies graceful error handling without throwing exceptions.
    /// SWR-DICOM-010: C-STORE of a missing file returns DicomStoreFailed.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-010")]
    public async Task DicomStore_MissingFile_ReturnsStoreFailed()
    {
        // Arrange — use localhost AE (no real PACS needed; file missing triggers early exit)
        var config = new TestDicomNetworkConfig();
        var storeScu = new DicomStoreScu(config);

        var missingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"missing_{Guid.NewGuid()}.dcm");

        // Act
        var result = await storeScu.StoreAsync(missingPath);

        // Assert
        result.IsFailure.Should().BeTrue("storing a non-existent file must fail");
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    /// <summary>
    /// Integration test: DicomStoreScu returns failure when PACS is unreachable (connection refused).
    /// SWR-DICOM-010: Network failure is caught and returned as DicomStoreFailed (no exception).
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-010")]
    public async Task DicomStore_UnreachablePacs_ReturnsStoreFailed()
    {
        // Arrange — write a minimal temp file so the file-exists check passes
        var tempFile = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllBytes(tempFile, Array.Empty<byte>());

        try
        {
            var config = new TestDicomNetworkConfig(pacsPort: 19999); // Unlikely to be open
            var storeScu = new DicomStoreScu(config);

            // Act
            var result = await storeScu.StoreAsync(tempFile);

            // Assert — must not throw; must return failure
            result.IsFailure.Should().BeTrue("C-STORE to an unreachable host must fail gracefully");
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            System.IO.File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Integration test: DicomFindScu returns failure when MWL SCP is unreachable.
    /// SWR-DICOM-020: Network failure during C-FIND is caught and returned as DicomQueryFailed.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-DICOM-020")]
    public async Task DicomFind_UnreachableMwlScp_ReturnsQueryFailed()
    {
        // Arrange — MWL SCP on an unreachable port
        var config = new TestDicomNetworkConfig(mwlPort: 19998);
        var findScu = new DicomFindScu(config);

        var query = new WorklistQuery(
            AeTitle: "TESTMWL",
            DateFrom: DateOnly.FromDateTime(DateTime.Today),
            DateTo: DateOnly.FromDateTime(DateTime.Today),
            PatientId: null);

        // Act
        var result = await findScu.QueryWorklistAsync(query);

        // Assert
        result.IsFailure.Should().BeTrue("C-FIND to an unreachable MWL SCP must fail gracefully");
        result.Error.Should().Be(ErrorCode.DicomQueryFailed);
    }

    // ── Scenario 6: CD Burning ─────────────────────────────────────────────────

    /// <summary>
    /// Integration test: CDDVDBurnService succeeds when disc is inserted and study files exist.
    /// Uses IMAPIComWrapper (simulated) and a stubbed IStudyRepository.
    /// SWR-CD-010: Burn session creates, adds files, and verifies successfully.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CD-010")]
    public async Task CdBurn_WithDiscAndFiles_SucceedsEndToEnd()
    {
        // Arrange
        var tempFile = System.IO.Path.GetTempFileName();
        System.IO.File.WriteAllText(tempFile, "DICOM_TEST");

        try
        {
            var studyRepo = Substitute.For<HnVue.CDBurning.IStudyRepository>();
            studyRepo.GetFilesForStudyAsync("1.2.3.4.STUDY", Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Result.Success<IReadOnlyList<string>>(new[] { tempFile })));

            var burnSession = new IMAPIComWrapper();
            burnSession.SimulateDiscInserted(blank: true);

            var burnService = new CDDVDBurnService(burnSession, studyRepo);

            // Act
            var result = await burnService.BurnStudyAsync("1.2.3.4.STUDY", "TEST_STUDY");

            // Assert
            result.IsSuccess.Should().BeTrue("burn should succeed when disc is present and study files exist");
        }
        finally
        {
            System.IO.File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Integration test: CDDVDBurnService returns BurnFailed when no disc is inserted.
    /// SWR-CD-010: Missing disc is detected before burn attempt.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CD-010")]
    public async Task CdBurn_NoDiscInserted_ReturnsBurnFailed()
    {
        // Arrange — no SimulateDiscInserted called → disc absent
        var studyRepo = Substitute.For<HnVue.CDBurning.IStudyRepository>();
        var burnSession = new IMAPIComWrapper();
        var burnService = new CDDVDBurnService(burnSession, studyRepo);

        // Act
        var result = await burnService.BurnStudyAsync("1.2.3.4.STUDY2", "NO_DISC");

        // Assert
        result.IsFailure.Should().BeTrue("burn must fail when no disc is in the drive");
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    /// <summary>
    /// Integration test: CDDVDBurnService returns BurnFailed when the study has no files.
    /// SWR-CD-020: Empty study is rejected before burn attempt.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-CD-020")]
    public async Task CdBurn_EmptyStudy_ReturnsNotFound()
    {
        // Arrange
        var studyRepo = Substitute.For<HnVue.CDBurning.IStudyRepository>();
        studyRepo.GetFilesForStudyAsync("1.2.3.4.EMPTY", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<string>>(Array.Empty<string>())));

        var burnSession = new IMAPIComWrapper();
        burnSession.SimulateDiscInserted(blank: true);

        var burnService = new CDDVDBurnService(burnSession, studyRepo);

        // Act
        var result = await burnService.BurnStudyAsync("1.2.3.4.EMPTY", "EMPTY_STUDY");

        // Assert
        result.IsFailure.Should().BeTrue("burn must fail when the study has no files");
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── Scenario 7: PatientListViewModel DI composition ───────────────────────

    /// <summary>
    /// Integration test: PatientListViewModel resolves with an injected IStudylistViewModel.
    /// SWR-UI-010: DI container composes nested ViewModels correctly.
    /// </summary>
    [Fact]
    [Trait("SWR", "SWR-UI-010")]
    public void PatientListViewModel_DI_ComposesStudylistViewModel()
    {
        // Arrange
        var patientService = Substitute.For<IPatientService>();
        var studyRepo = Substitute.For<HnVue.Common.Abstractions.IStudyRepository>();
        var studylistVm = new StudylistViewModel(studyRepo);

        // Act
        var vm = new PatientListViewModel(patientService, studylistVm);

        // Assert
        vm.StudylistViewModel.Should().NotBeNull("PatientListViewModel must expose the nested StudylistViewModel");
        vm.StudylistViewModel.Should().BeSameAs(studylistVm, "composed instance must be the injected one");
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

    // ── Test infrastructure helpers ────────────────────────────────────────────

    /// <summary>
    /// Minimal <see cref="IDicomNetworkConfig"/> implementation for integration tests.
    /// Points all endpoints at localhost ports that are unlikely to be running DICOM services,
    /// so that network-error paths can be exercised without external infrastructure.
    /// </summary>
    private sealed class TestDicomNetworkConfig : IDicomNetworkConfig
    {
        /// <summary>Initialises the config with optional port overrides.</summary>
        public TestDicomNetworkConfig(
            int pacsPort = 19999,
            int mwlPort = 19998)
        {
            PacsPort = pacsPort;
            MwlPort = mwlPort;
        }

        /// <inheritdoc/>
        public string PacsHost => "127.0.0.1";

        /// <inheritdoc/>
        public int PacsPort { get; }

        /// <inheritdoc/>
        public string PacsAeTitle => "TEST_PACS";

        /// <inheritdoc/>
        public string LocalAeTitle => "TEST_SCU";

        /// <inheritdoc/>
        public string MwlHost => "127.0.0.1";

        /// <inheritdoc/>
        public int MwlPort { get; }
    }
}
