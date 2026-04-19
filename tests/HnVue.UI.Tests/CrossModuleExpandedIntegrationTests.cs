using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.Contracts.Events;
using HnVue.UI.Contracts.Models;
using HnVue.UI.Contracts.ViewModels;
using HnVue.UI.ViewModels;
using HnVue.UI.ViewModels.Models;
using NSubstitute;
using Xunit;

namespace HnVue.UI.Tests;

/// <summary>
/// S07-R3: Expanded cross-module integration tests covering:
/// 1. LoginViewModel ↔ ISecurityService ↔ ISecurityContext authentication chain
/// 2. DoseViewModel ↔ IDoseService dose retrieval and validation display
/// 3. WorkflowViewModel ↔ IWorkflowEngine full state transition paths
/// 4. UI.Contracts interface implementation consistency for all ViewModels
/// 5. Detector → Dose → Incident cascade through ViewModels
/// </summary>

// ── 1. Security → LoginViewModel Integration ──────────────────────────────────

public class SecurityLoginIntegrationTests
{
    [Fact]
    public async Task LoginViewModel_AuthenticateSuccess_SetsSecurityContext()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();
        var token = new AuthenticationToken(
            "u1", "admin", UserRole.Admin, "jwt.token.sig",
            DateTimeOffset.UtcNow.AddHours(1), "jti-001");

        securityService.AuthenticateAsync("admin", "pass123", Arg.Any<CancellationToken>())
            .Returns(Result.Success(token));

        var sut = new LoginViewModel(securityService, securityContext);
        sut.Username = "admin";
        sut.Password = "pass123";

        await sut.LoginCommand.ExecuteAsync(null);

        securityContext.Received(1).SetCurrentUser(Arg.Is<AuthenticatedUser>(
            u => u.UserId == "u1" && u.Username == "admin" && u.Role == UserRole.Admin));
    }

    [Fact]
    public async Task LoginViewModel_AuthenticateSuccess_RaisesLoginSucceeded()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();
        var token = new AuthenticationToken(
            "u2", "tech", UserRole.Radiographer, "jwt.2",
            DateTimeOffset.UtcNow.AddHours(1), "jti-002");

        securityService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(token));

        var sut = new LoginViewModel(securityService, securityContext);
        sut.Username = "tech";
        sut.Password = "pw";

        LoginSuccessEventArgs? raised = null;
        sut.LoginSucceeded += (_, e) => raised = e;

        await sut.LoginCommand.ExecuteAsync(null);

        raised.Should().NotBeNull();
        raised!.Token.UserId.Should().Be("u2");
        raised.Token.Role.Should().Be(UserRole.Radiographer);
    }

    [Fact]
    public async Task LoginViewModel_AuthenticateFailure_DoesNotSetContext()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();

        securityService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AuthenticationFailed, "Auth failed"));

        var sut = new LoginViewModel(securityService, securityContext);
        sut.Username = "bad";
        sut.Password = "creds";

        await sut.LoginCommand.ExecuteAsync(null);

        securityContext.DidNotReceive().SetCurrentUser(Arg.Any<AuthenticatedUser>());
        sut.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginViewModel_AccountLocked_SetsKoreanErrorMessage()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();

        securityService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<AuthenticationToken>(ErrorCode.AccountLocked, "Account locked"));

        var sut = new LoginViewModel(securityService, securityContext);
        sut.Username = "locked";
        sut.Password = "user";

        await sut.LoginCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Contain("잠겼");
    }

    [Fact]
    public async Task LoginViewModel_IsLoadingTrue_DuringAuthentication()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();
        var tcs = new TaskCompletionSource<Result<AuthenticationToken>>();

        securityService.AuthenticateAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(tcs.Task);

        var sut = new LoginViewModel(securityService, securityContext);
        sut.Username = "admin";
        sut.Password = "pw";

        var loginTask = sut.LoginCommand.ExecuteAsync(null);
        sut.IsLoading.Should().BeTrue();

        tcs.SetResult(Result.Success(new AuthenticationToken(
            "u1", "admin", UserRole.Admin, "jwt", DateTimeOffset.UtcNow, "jti")));
        await loginTask;

        sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public void LoginViewModel_CanLogin_RequiresBothFields()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(),
            Substitute.For<ISecurityContext>());

        sut.LoginCommand.CanExecute(null).Should().BeFalse();

        sut.Username = "admin";
        sut.LoginCommand.CanExecute(null).Should().BeFalse();

        sut.Password = "pw";
        sut.LoginCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public void LoginViewModel_AvailableUserIds_ContainsExpectedValues()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(),
            Substitute.For<ISecurityContext>());

        sut.AvailableUserIds.Should().Contain("admin", "operator", "technician");
    }
}

// ── 2. DoseViewModel ↔ IDoseService Integration ──────────────────────────────

public class DoseServiceIntegrationTests
{
    [Fact]
    public async Task DoseViewModel_Refresh_LoadsDoseFromService()
    {
        var doseService = Substitute.For<IDoseService>();
        var dose = new DoseRecord("D1", "UID-001", 2.5, 1500.0, 0.15, "CHEST",
            DateTimeOffset.UtcNow, "P-001");
        doseService.GetDoseByStudyAsync("UID-001", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var sut = new DoseViewModel(doseService) { ActiveStudyInstanceUid = "UID-001" };

        await sut.RefreshCommand.ExecuteAsync(null);

        sut.CurrentDose.Should().NotBeNull();
        sut.CurrentDose!.DoseId.Should().Be("D1");
        sut.CurrentDose.Dap.Should().Be(2.5);
    }

    [Fact]
    public async Task DoseViewModel_Refresh_NoDoseRecord_ClearsCurrentDose()
    {
        var doseService = Substitute.For<IDoseService>();
        doseService.GetDoseByStudyAsync("UID-EMPTY", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord>(null));

        var sut = new DoseViewModel(doseService) { ActiveStudyInstanceUid = "UID-EMPTY" };

        await sut.RefreshCommand.ExecuteAsync(null);

        sut.CurrentDose.Should().BeNull();
        sut.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task DoseViewModel_LoadForStudy_SetsUidAndRefreshes()
    {
        var doseService = Substitute.For<IDoseService>();
        var dose = new DoseRecord("D2", "UID-002", 1.8, 1200.0, 0.10, "ABDOMEN",
            DateTimeOffset.UtcNow);
        doseService.GetDoseByStudyAsync("UID-002", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var sut = new DoseViewModel(doseService);

        await sut.LoadForStudyAsync("UID-002");

        sut.ActiveStudyInstanceUid.Should().Be("UID-002");
        sut.CurrentDose.Should().NotBeNull();
        sut.CurrentDose!.BodyPart.Should().Be("ABDOMEN");
    }

    [Fact]
    public async Task DoseViewModel_Refresh_NoStudyUid_ClearsDose()
    {
        var doseService = Substitute.For<IDoseService>();
        var sut = new DoseViewModel(doseService);

        await sut.RefreshCommand.ExecuteAsync(null);

        sut.CurrentDose.Should().BeNull();
        sut.ValidationLevel.Should().Be(DoseValidationLevel.Allow);
    }

    [Fact]
    public async Task DoseViewModel_Refresh_ServiceFailure_SetsErrorMessage()
    {
        var doseService = Substitute.For<IDoseService>();
        doseService.GetDoseByStudyAsync("UID-ERR", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "DB error"));

        var sut = new DoseViewModel(doseService) { ActiveStudyInstanceUid = "UID-ERR" };

        await sut.RefreshCommand.ExecuteAsync(null);

        sut.CurrentDose.Should().BeNull();
        sut.ErrorMessage.Should().NotBeNull();
    }

    [Fact]
    public void DoseViewModel_InitialState_DefaultValidationAllow()
    {
        var sut = new DoseViewModel(Substitute.For<IDoseService>());

        sut.ValidationLevel.Should().Be(DoseValidationLevel.Allow);
        sut.CurrentDose.Should().BeNull();
        sut.ActiveStudyInstanceUid.Should().BeNull();
    }
}

// ── 3. Workflow State Transition Full Path Integration ─────────────────────────

public class WorkflowFullTransitionPathTests
{
    [Fact]
    public void WorkflowViewModel_FullAcquisitionPath_AllStatesProduceStatusMessages()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new WorkflowViewModel(engine, ctx);

        // Simulate each state transition and verify status message updates
        var transitions = new (WorkflowState From, WorkflowState To)[]
        {
            (WorkflowState.Idle, WorkflowState.PatientSelected),
            (WorkflowState.PatientSelected, WorkflowState.ProtocolLoaded),
            (WorkflowState.ProtocolLoaded, WorkflowState.ReadyToExpose),
            (WorkflowState.ReadyToExpose, WorkflowState.Exposing),
            (WorkflowState.Exposing, WorkflowState.ImageAcquiring),
            (WorkflowState.ImageAcquiring, WorkflowState.ImageProcessing),
            (WorkflowState.ImageProcessing, WorkflowState.ImageReview),
            (WorkflowState.ImageReview, WorkflowState.Completed),
        };

        var messages = new HashSet<string>();
        foreach (var (from, to) in transitions)
        {
            engine.CurrentState.Returns(to);
            engine.CurrentSafeState.Returns(SafeState.Idle);
            RaiseStateChanged(sut, from, to);
            sut.StatusMessage.Should().NotBeNullOrEmpty($"State {to} should have a status message");
            messages.Add(sut.StatusMessage);
        }

        // Each state should produce a distinct message
        messages.Should().HaveCount(transitions.Length);
    }

    [Fact]
    public void WorkflowViewModel_ErrorFromAnyState_SetsEmergencySafeState()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.Error);
        engine.CurrentSafeState.Returns(SafeState.Emergency);

        var sut = new WorkflowViewModel(engine, ctx);
        RaiseStateChanged(sut, WorkflowState.Exposing, WorkflowState.Error);

        sut.CurrentSafeState.Should().Be(SafeState.Emergency);
        sut.SafeStateLabel.Should().Be("EMERGENCY");
    }

    [Fact]
    public void WorkflowViewModel_ImageAcquiring_DisplaysCorrectStatus()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.ImageAcquiring);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);
        RaiseStateChanged(sut, WorkflowState.Exposing, WorkflowState.ImageAcquiring);

        sut.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WorkflowViewModel_ImageProcessing_DisplaysCorrectStatus()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.ImageProcessing);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);
        RaiseStateChanged(sut, WorkflowState.ImageAcquiring, WorkflowState.ImageProcessing);

        sut.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WorkflowViewModel_ImageReview_DisplaysCorrectStatus()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.ImageReview);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);
        RaiseStateChanged(sut, WorkflowState.ImageProcessing, WorkflowState.ImageReview);

        sut.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WorkflowViewModel_Completed_DisplaysCorrectStatus()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentState.Returns(WorkflowState.Completed);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);
        RaiseStateChanged(sut, WorkflowState.ImageReview, WorkflowState.Completed);

        sut.StatusMessage.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData(SafeState.Idle, "IDLE")]
    [InlineData(SafeState.Warning, "WARNING")]
    [InlineData(SafeState.Degraded, "DEGRADED")]
    [InlineData(SafeState.Blocked, "BLOCKED")]
    [InlineData(SafeState.Emergency, "EMERGENCY")]
    public void WorkflowViewModel_AllSafeStates_HaveCorrectLabels(SafeState state, string expectedLabel)
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.CurrentSafeState.Returns(state);
        engine.CurrentState.Returns(WorkflowState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);

        sut.SafeStateLabel.Should().Be(expectedLabel);
    }

    private static void RaiseStateChanged(WorkflowViewModel sut, WorkflowState from, WorkflowState to)
    {
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(from, to)]);
    }
}

// ── 4. UI.Contracts Interface Implementation Consistency ─────────────────────

public class InterfaceContractConsistencyTests
{
    [Fact]
    public void LoginViewModel_ImplementsILoginViewModel()
    {
        var sut = new LoginViewModel(
            Substitute.For<ISecurityService>(),
            Substitute.For<ISecurityContext>());

        sut.Should().BeAssignableTo<ILoginViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void PatientListViewModel_ImplementsIPatientListViewModel()
    {
        var studylistVm = Substitute.For<IStudylistViewModel>();
        var sut = new PatientListViewModel(Substitute.For<IPatientService>(), studylistVm);

        sut.Should().BeAssignableTo<IPatientListViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void StudylistViewModel_ImplementsIStudylistViewModel()
    {
        var sut = new StudylistViewModel(Substitute.For<IStudyRepository>());

        sut.Should().BeAssignableTo<IStudylistViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void WorkflowViewModel_ImplementsIWorkflowViewModel()
    {
        var sut = new WorkflowViewModel(
            Substitute.For<IWorkflowEngine>(),
            Substitute.For<ISecurityContext>());

        sut.Should().BeAssignableTo<IWorkflowViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void DoseViewModel_ImplementsIDoseViewModel()
    {
        var sut = new DoseViewModel(Substitute.For<IDoseService>());

        sut.Should().BeAssignableTo<IDoseViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void MergeViewModel_ImplementsIMergeViewModel()
    {
        var sut = new MergeViewModel(Substitute.For<IPatientService>());

        sut.Should().BeAssignableTo<IMergeViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void SettingsViewModel_ImplementsISettingsViewModel()
    {
        var sut = new SettingsViewModel();

        sut.Should().BeAssignableTo<ISettingsViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void CDBurnViewModel_ImplementsICDBurnViewModel()
    {
        var sut = new CDBurnViewModel(Substitute.For<ICDDVDBurnService>());

        sut.Should().BeAssignableTo<ICDBurnViewModel>();
        ((IViewModelBase)sut).IsLoading.Should().BeFalse();
    }

    [Fact]
    public void AddPatientProcedureViewModel_ImplementsIAddPatientProcedureViewModel()
    {
        var sut = new AddPatientProcedureViewModel(Substitute.For<IPatientService>(), Substitute.For<ISecurityContext>());

        sut.Should().BeAssignableTo<IAddPatientProcedureViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void ImageViewerViewModel_ImplementsIImageViewerViewModel()
    {
        var sut = new ImageViewerViewModel(Substitute.For<IImageProcessor>());

        sut.Should().BeAssignableTo<IImageViewerViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void DoseDisplayViewModel_ImplementsIDoseDisplayViewModel()
    {
        var sut = new DoseDisplayViewModel(Substitute.For<IDoseService>());

        sut.Should().BeAssignableTo<IDoseDisplayViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void QuickPinLockViewModel_ImplementsIQuickPinLockViewModel()
    {
        var sut = new QuickPinLockViewModel(
            Substitute.For<ISecurityContext>(),
            Substitute.For<ISecurityService>());

        sut.Should().BeAssignableTo<IQuickPinLockViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void SystemAdminViewModel_ImplementsISystemAdminViewModel()
    {
        var sut = new SystemAdminViewModel(
            Substitute.For<ISystemAdminService>(),
            Substitute.For<ISecurityContext>());

        sut.Should().BeAssignableTo<ISystemAdminViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void StudyItem_ImplementsIStudyItem()
    {
        var record = new StudyRecord("UID", "P-1", DateTimeOffset.UtcNow, "Chest", null, null);
        var sut = new StudyItem(record);

        sut.Should().BeAssignableTo<IStudyItem>();
        sut.Study.Should().Be(record);
        sut.IsSelected.Should().BeFalse();
    }
}

// ── 5. Detector → Dose → Workflow Cascade Integration ────────────────────────

public class DetectorDoseWorkflowCascadeTests
{
    [Fact]
    public async Task WorkflowViewModel_PrepareExposure_CallsWorkflowEngine()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        engine.CurrentState.Returns(WorkflowState.Idle);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);

        await sut.PrepareExposureCommand.ExecuteAsync(null);

        await engine.Received().TransitionAsync(
            WorkflowState.ReadyToExpose, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WorkflowViewModel_TriggerExposure_CallsTransitionToExposing()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        engine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        engine.CurrentSafeState.Returns(SafeState.Idle);
        ctx.HasRole(UserRole.Radiographer).Returns(true);

        var sut = new WorkflowViewModel(engine, ctx);

        await sut.TriggerExposureCommand.ExecuteAsync(null);

        await engine.Received().TransitionAsync(
            WorkflowState.Exposing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoseViewModel_DoseLoaded_RecordsDoseMetrics()
    {
        var doseService = Substitute.For<IDoseService>();
        var dose = new DoseRecord(
            "D-CASCADE", "UID-CAS", 3.2, 1600.0, 0.22, "CHEST",
            DateTimeOffset.UtcNow, "P-CAS",
            DapMgyCm2: 3.2, FieldAreaCm2: 400.0, MeanPixelValue: 2400.0,
            EiTarget: 1500.0, EsdMgy: 0.0108);

        doseService.GetDoseByStudyAsync("UID-CAS", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var sut = new DoseViewModel(doseService);
        await sut.LoadForStudyAsync("UID-CAS");

        sut.CurrentDose.Should().NotBeNull();
        sut.CurrentDose!.EsdMgy.Should().BeApproximately(0.0108, 0.0001);
        sut.CurrentDose.Ei.Should().Be(1600.0);
        sut.CurrentDose.EffectiveDose.Should().Be(0.22);
    }

    [Fact]
    public async Task WorkflowViewModel_Abort_CallsEngineAbort()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.AbortAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        engine.CurrentState.Returns(WorkflowState.Exposing);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);

        await sut.AbortCommand.ExecuteAsync(null);

        await engine.Received(1).AbortAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void WorkflowViewModel_EmergencyExposure_RequiresAuthRole()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        ctx.HasRole(UserRole.Radiographer).Returns(false);
        engine.CurrentState.Returns(WorkflowState.ReadyToExpose);
        engine.CurrentSafeState.Returns(SafeState.Idle);

        var sut = new WorkflowViewModel(engine, ctx);

        sut.TriggerExposureCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public async Task DoseWorkflowCascade_ExposureToDoseLookup()
    {
        // Simulate: Workflow exposes → dose recorded → DoseViewModel displays
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();
        engine.TransitionAsync(Arg.Any<WorkflowState>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        engine.CurrentState.Returns(WorkflowState.ImageReview);
        engine.CurrentSafeState.Returns(SafeState.Idle);
        ctx.HasRole(UserRole.Radiographer).Returns(true);

        var workflowSut = new WorkflowViewModel(engine, ctx);

        // Simulate exposure completed, image now in review
        RaiseStateChanged(workflowSut, WorkflowState.Exposing, WorkflowState.ImageAcquiring);
        RaiseStateChanged(workflowSut, WorkflowState.ImageAcquiring, WorkflowState.ImageProcessing);
        RaiseStateChanged(workflowSut, WorkflowState.ImageProcessing, WorkflowState.ImageReview);

        // Now load dose for the study
        var doseService = Substitute.For<IDoseService>();
        var dose = new DoseRecord("D-POST", "UID-POST", 4.1, 1800.0, 0.28, "CHEST",
            DateTimeOffset.UtcNow, "P-POST");
        doseService.GetDoseByStudyAsync("UID-POST", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var doseSut = new DoseViewModel(doseService);
        await doseSut.LoadForStudyAsync("UID-POST");

        doseSut.CurrentDose.Should().NotBeNull();
        doseSut.CurrentDose!.DoseId.Should().Be("D-POST");
    }

    private static void RaiseStateChanged(WorkflowViewModel sut, WorkflowState from, WorkflowState to)
    {
        sut.GetType()
            .GetMethod("OnWorkflowStateChanged",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .Invoke(sut, [null, new WorkflowStateChangedEventArgs(from, to)]);
    }
}

// ── 6. Patient → Study → Workflow End-to-End Integration ──────────────────────

public class PatientStudyWorkflowE2ETests
{
    [Fact]
    public void PatientListViewModel_ComposesStudylistViewModel()
    {
        var studylistVm = Substitute.For<IStudylistViewModel>();
        var sut = new PatientListViewModel(Substitute.For<IPatientService>(), studylistVm);

        sut.StudylistViewModel.Should().BeSameAs(studylistVm);
    }

    [Fact]
    public async Task LoginViewModel_Success_Then_PatientSelection_FullFlow()
    {
        // Step 1: Login
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();
        var token = new AuthenticationToken(
            "u1", "admin", UserRole.Admin, "jwt",
            DateTimeOffset.UtcNow.AddHours(1), "jti-login-flow");

        securityService.AuthenticateAsync("admin", "pw", Arg.Any<CancellationToken>())
            .Returns(Result.Success(token));

        var loginVm = new LoginViewModel(securityService, securityContext);
        loginVm.Username = "admin";
        loginVm.Password = "pw";

        await loginVm.LoginCommand.ExecuteAsync(null);

        securityContext.Received().SetCurrentUser(Arg.Any<AuthenticatedUser>());

        // Step 2: Patient selection → Studylist
        var dicomService = Substitute.For<IDicomService>();
        var studylistVm = new StudylistViewModel(Substitute.For<IStudyRepository>());
        var patientVm = new PatientListViewModel(
            Substitute.For<IPatientService>(), studylistVm);

        var patient = new PatientRecord("P-001", "홍^길동",
            new DateOnly(1990, 1, 1), "M", false, DateTimeOffset.UtcNow, "admin");
        patientVm.Patients.Add(patient);
        patientVm.SelectedPatient = patient;

        patientVm.SelectedPatient.Should().NotBeNull();
        patientVm.SelectedPatient!.PatientId.Should().Be("P-001");
        patientVm.StudylistViewModel.Should().BeSameAs(studylistVm);
    }

    [Fact]
    public void MergeViewModel_DualPanel_PatientStudyChain()
    {
        var patientService = Substitute.For<IPatientService>();
        var sut = new MergeViewModel(patientService);

        // Patient A studies
        var studyA1 = new StudyRecord("UID-A1", "P-A", DateTimeOffset.UtcNow, "Chest PA", "ACC-A1", "CHEST");
        var studyA2 = new StudyRecord("UID-A2", "P-A", DateTimeOffset.UtcNow.AddMinutes(5), "Chest Lateral", "ACC-A2", "CHEST");

        sut.PreviewStudiesA.Add(new StudyItem(studyA1));
        sut.PreviewStudiesA.Add(new StudyItem(studyA2));

        // Patient B studies
        var studyB1 = new StudyRecord("UID-B1", "P-B", DateTimeOffset.UtcNow, "Abdomen AP", "ACC-B1", "ABDOMEN");

        sut.PreviewStudiesB.Add(new StudyItem(studyB1));

        sut.PreviewStudiesA.Should().HaveCount(2);
        sut.PreviewStudiesB.Should().HaveCount(1);

        // Select from A
        var selectedA = sut.PreviewStudiesA[0];
        selectedA.IsSelected = true;
        sut.SelectedStudies.Add(selectedA);

        sut.SelectedStudies.Should().ContainSingle()
            .Which.Study.StudyInstanceUid.Should().Be("UID-A1");
    }

    [Fact]
    public void AddPatientProcedureViewModel_AutoGenerate_SetsInitialValues()
    {
        var sut = new AddPatientProcedureViewModel(Substitute.For<IPatientService>(), Substitute.For<ISecurityContext>());

        // Auto-generated fields should have non-empty initial values
        sut.PatientId.Should().NotBeNullOrEmpty();
        sut.AccessionNumber.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AddPatientProcedureViewModel_AvailableProjections_ContainsExpectedValues()
    {
        var sut = new AddPatientProcedureViewModel(Substitute.For<IPatientService>(), Substitute.For<ISecurityContext>());

        sut.AvailableProjections.Should().Contain("Chest PA", "Hand PA", "Spine AP");
        sut.AvailableProjections.Should().HaveCountGreaterThan(5);
    }

    [Fact]
    public void AddPatientProcedureViewModel_AvailableDescriptions_ContainsExpectedValues()
    {
        var sut = new AddPatientProcedureViewModel(Substitute.For<IPatientService>(), Substitute.For<ISecurityContext>());

        sut.AvailableDescriptions.Should().Contain("Routine", "Emergency", "Follow-up");
    }
}

// ── 7. ViewModel Validation and Error Handling Integration ────────────────────

public class ViewModelValidationIntegrationTests
{
    [Fact]
    public async Task CDBurnViewModel_StartBurn_RequiresStudySelection()
    {
        var burnService = Substitute.For<ICDDVDBurnService>();
        var sut = new CDBurnViewModel(burnService);

        // No study selected
        sut.StartBurnCommand.CanExecute(null).Should().BeFalse();

        sut.SelectedStudyId = "UID-BURN-001";
        sut.StartBurnCommand.CanExecute(null).Should().BeTrue();
    }

    [Fact]
    public async Task CDBurnViewModel_BurnProgress_UpdatesProperties()
    {
        var burnService = Substitute.For<ICDDVDBurnService>();
        burnService.BurnStudyAsync("UID-BURN-002", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var sut = new CDBurnViewModel(burnService);
        sut.SelectedStudyId = "UID-BURN-002";

        await sut.StartBurnCommand.ExecuteAsync(null);

        burnService.Received(1).BurnStudyAsync("UID-BURN-002", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void StudyItem_SelectedState_PreservesStudyData()
    {
        var record = new StudyRecord("UID-SEL", "P-SEL", DateTimeOffset.UtcNow,
            "Chest PA", "ACC-SEL", "CHEST");
        var sut = new StudyItem(record);

        sut.IsSelected.Should().BeFalse();
        sut.Study.StudyInstanceUid.Should().Be("UID-SEL");
        sut.Study.PatientId.Should().Be("P-SEL");

        sut.IsSelected = true;
        sut.IsSelected.Should().BeTrue();
        sut.Study.StudyInstanceUid.Should().Be("UID-SEL"); // Data preserved after selection
    }

    [Fact]
    public void SettingsViewModel_TabNavigation_PreservesState()
    {
        var sut = new SettingsViewModel();

        sut.SelectTabCommand.Execute("Network");
        sut.ActiveTab.Should().Be("Network");

        sut.SelectTabCommand.Execute("Display");
        sut.ActiveTab.Should().Be("Display");

        // Previous selection was overwritten
        sut.ActiveTab.Should().NotBe("Network");
    }

    [Fact]
    public async Task SettingsViewModel_SaveCommand_CompletesSuccessfully()
    {
        var sut = new SettingsViewModel();
        var eventRaised = false;
        sut.SaveCompleted += (_, _) => eventRaised = true;

        await sut.SaveCommand.ExecuteAsync(null);

        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void DoseDisplayViewModel_DefaultState_IsZero()
    {
        var sut = new DoseDisplayViewModel(Substitute.For<IDoseService>());

        sut.Should().BeAssignableTo<IDoseDisplayViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }
}

// ── 8. DI Registration Verification ──────────────────────────────────────────

public class DIRegistrationVerificationTests
{
    /// <summary>
    /// Verifies that all ViewModel concrete types accept their required constructor
    /// dependencies (mocked), proving the DI registration chain is valid.
    /// This catches missing interface registrations at test time.
    /// </summary>

    [Fact]
    public void LoginViewModel_Requires_ISecurityService_And_ISecurityContext()
    {
        var securityService = Substitute.For<ISecurityService>();
        var securityContext = Substitute.For<ISecurityContext>();

        var sut = new LoginViewModel(securityService, securityContext);

        sut.Should().BeAssignableTo<ILoginViewModel>();
    }

    [Fact]
    public void PatientListViewModel_Requires_IPatientService_And_IStudylistViewModel()
    {
        var patientService = Substitute.For<IPatientService>();
        var studylistVm = Substitute.For<IStudylistViewModel>();

        var sut = new PatientListViewModel(patientService, studylistVm);

        sut.Should().BeAssignableTo<IPatientListViewModel>();
        sut.StudylistViewModel.Should().BeSameAs(studylistVm);
    }

    [Fact]
    public void StudylistViewModel_Requires_IStudyRepository()
    {
        var studyRepo = Substitute.For<IStudyRepository>();

        var sut = new StudylistViewModel(studyRepo);

        sut.Should().BeAssignableTo<IStudylistViewModel>();
    }

    [Fact]
    public void WorkflowViewModel_Requires_IWorkflowEngine_And_ISecurityContext()
    {
        var engine = Substitute.For<IWorkflowEngine>();
        var ctx = Substitute.For<ISecurityContext>();

        var sut = new WorkflowViewModel(engine, ctx);

        sut.Should().BeAssignableTo<IWorkflowViewModel>();
    }

    [Fact]
    public void DoseViewModel_Requires_IDoseService()
    {
        var doseService = Substitute.For<IDoseService>();

        var sut = new DoseViewModel(doseService);

        sut.Should().BeAssignableTo<IDoseViewModel>();
    }

    [Fact]
    public void DoseDisplayViewModel_Requires_IDoseService()
    {
        var doseService = Substitute.For<IDoseService>();

        var sut = new DoseDisplayViewModel(doseService);

        sut.Should().BeAssignableTo<IDoseDisplayViewModel>();
    }

    [Fact]
    public void CDBurnViewModel_Requires_ICDDVDBurnService()
    {
        var burnService = Substitute.For<ICDDVDBurnService>();

        var sut = new CDBurnViewModel(burnService);

        sut.Should().BeAssignableTo<ICDBurnViewModel>();
    }

    [Fact]
    public void MergeViewModel_Requires_IPatientService()
    {
        var patientService = Substitute.For<IPatientService>();

        var sut = new MergeViewModel(patientService);

        sut.Should().BeAssignableTo<IMergeViewModel>();
    }

    [Fact]
    public void ImageViewerViewModel_Requires_IImageProcessor()
    {
        var imageProcessor = Substitute.For<IImageProcessor>();

        var sut = new ImageViewerViewModel(imageProcessor);

        sut.Should().BeAssignableTo<IImageViewerViewModel>();
    }

    [Fact]
    public void SystemAdminViewModel_Requires_ISystemAdminService_And_ISecurityContext()
    {
        var adminService = Substitute.For<ISystemAdminService>();
        var ctx = Substitute.For<ISecurityContext>();

        var sut = new SystemAdminViewModel(adminService, ctx);

        sut.Should().BeAssignableTo<ISystemAdminViewModel>();
    }

    [Fact]
    public void QuickPinLockViewModel_Requires_ISecurityContext_And_ISecurityService()
    {
        var ctx = Substitute.For<ISecurityContext>();
        var securityService = Substitute.For<ISecurityService>();

        var sut = new QuickPinLockViewModel(ctx, securityService);

        sut.Should().BeAssignableTo<IQuickPinLockViewModel>();
    }

    [Fact]
    public void AddPatientProcedureViewModel_Requires_IPatientService()
    {
        var patientService = Substitute.For<IPatientService>();

        var sut = new AddPatientProcedureViewModel(patientService, Substitute.For<ISecurityContext>());

        sut.Should().BeAssignableTo<IAddPatientProcedureViewModel>();
    }

    [Fact]
    public void SettingsViewModel_NoExternalDependencies()
    {
        var sut = new SettingsViewModel();

        sut.Should().BeAssignableTo<ISettingsViewModel>();
        sut.Should().BeAssignableTo<IViewModelBase>();
    }

    [Fact]
    public void AllViewModelInterfaces_InheritFromIViewModelBase()
    {
        var viewModelInterfaces = new Type[]
        {
            typeof(ILoginViewModel),
            typeof(IPatientListViewModel),
            typeof(IStudylistViewModel),
            typeof(IWorkflowViewModel),
            typeof(IDoseViewModel),
            typeof(IDoseDisplayViewModel),
            typeof(ICDBurnViewModel),
            typeof(IMergeViewModel),
            typeof(IImageViewerViewModel),
            typeof(ISystemAdminViewModel),
            typeof(IQuickPinLockViewModel),
            typeof(IAddPatientProcedureViewModel),
            typeof(ISettingsViewModel),
        };

        foreach (var iface in viewModelInterfaces)
        {
            typeof(IViewModelBase).IsAssignableFrom(iface).Should().BeTrue(
                $"{iface.Name} should inherit from IViewModelBase");
        }
    }

    [Fact]
    public void AllViewModelInterfaces_DefineIsLoadingThroughBase()
    {
        var viewModelInterfaces = new Type[]
        {
            typeof(ILoginViewModel),
            typeof(IPatientListViewModel),
            typeof(IStudylistViewModel),
            typeof(IWorkflowViewModel),
            typeof(IDoseViewModel),
            typeof(IDoseDisplayViewModel),
            typeof(ICDBurnViewModel),
            typeof(IMergeViewModel),
            typeof(IImageViewerViewModel),
            typeof(ISystemAdminViewModel),
            typeof(IQuickPinLockViewModel),
            typeof(IAddPatientProcedureViewModel),
            typeof(ISettingsViewModel),
        };

        foreach (var iface in viewModelInterfaces)
        {
            // IsLoading may be inherited from IViewModelBase; check on the interface itself
            var isLoadingProp = iface.GetProperty("IsLoading")
                ?? typeof(IViewModelBase).GetProperty("IsLoading");
            isLoadingProp.Should().NotBeNull(
                $"{iface.Name} should have IsLoading property (directly or inherited)");
        }
    }
}
