using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// Safety-Critical tests for dose interlock enhancements:
/// - Cumulative DAP threshold validation (3x DRL forces Block)
/// - Warn level segmentation (WarnLow / WarnHigh)
/// - Interlock state transitions (TriggerInterlockAsync)
/// - Boundary value tests at exact DRL multiples
/// - Edge cases (zero, negative inputs, emergency safety flag)
/// </summary>
[Trait("SWR", "SWR-DS-001")]
public sealed class DoseInterlockEnhancedTests
{
    private const string PatientId = "P001";
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseInterlockEnhancedTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // ── Helper: create dose records for cumulative history ───────────────────

    private static DoseRecord CreateDoseRecord(
        string bodyPart,
        double dap,
        double dapMgyCm2 = 0.0,
        string? patientId = PatientId,
        DateTimeOffset? recordedAt = null)
    {
        return new DoseRecord(
            DoseId: Guid.NewGuid().ToString(),
            StudyInstanceUid: Guid.NewGuid().ToString(),
            Dap: dap,
            Ei: 1000,
            EffectiveDose: 0.05,
            BodyPart: bodyPart,
            RecordedAt: recordedAt ?? DateTimeOffset.UtcNow.AddMinutes(-30),
            PatientId: patientId,
            DapMgyCm2: dapMgyCm2 > 0 ? dapMgyCm2 : dap);
    }

    private void SetupCumulativeHistory(IReadOnlyList<DoseRecord> records)
    {
        _repository.GetByPatientAsync(PatientId, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(records));
    }

    private void SetupEmptyCumulativeHistory()
    {
        _repository.GetByPatientAsync(PatientId, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(new List<DoseRecord>().AsReadOnly()));
    }

    /// <summary>
    /// Creates ExposureParameters with PatientId set for cumulative DAP testing.
    /// </summary>
    private static ExposureParameters WithPatient(string bodyPart, double kvp, double mas, string studyUid = "1.2.3.4")
        => new(BodyPart: bodyPart, Kvp: kvp, Mas: mas, StudyInstanceUid: studyUid, PatientId: PatientId);

    /// <summary>
    /// Creates ExposureParameters without PatientId (anonymous, no cumulative tracking).
    /// </summary>
    private static ExposureParameters WithoutPatient(string bodyPart, double kvp, double mas, string studyUid = "1.2.3.4")
        => new(BodyPart: bodyPart, Kvp: kvp, Mas: mas, StudyInstanceUid: studyUid);

    // ── Constructor ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new DoseService(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("doseRepository");
    }

    // ── Warn Level Segmentation (without cumulative tracking) ──────────────────

    [Fact]
    public async Task ValidateExposure_WarnLow_DapBetween1xAnd1_5xDrl_ReturnsWarnLow()
    {
        // CHEST DRL=10. DAP ~12 (between 10 and 15)
        // kVp=100, mAs=600 -> DAP = (10000*600)/500000 = 12.0
        var parameters = WithoutPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.Low);
        result.Value.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposure_WarnHigh_DapBetween1_5xAnd2xDrl_ReturnsWarnHigh()
    {
        // CHEST DRL=10. DAP ~18 (between 15 and 20)
        // kVp=120, mAs=625 -> DAP = (14400*625)/500000 = 18.0
        var parameters = WithoutPatient("CHEST", 120, 625);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.High);
        result.Value.IsAllowed.Should().BeTrue();
        result.Value.Message.Should().Contain("HIGH warning zone");
    }

    [Fact]
    public async Task ValidateExposure_AllowLevel_ReturnsWarnLevelNone()
    {
        var parameters = WithoutPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.None);
    }

    [Fact]
    public async Task ValidateExposure_BlockLevel_ReturnsWarnLevelNone()
    {
        var parameters = WithoutPatient("CHEST", 120, 1050);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.None);
    }

    [Fact]
    public async Task ValidateExposure_EmergencyLevel_ReturnsWarnLevelNone()
    {
        var parameters = WithoutPatient("CHEST", 120, 3000);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.None);
    }

    // ── Warn Level Boundary Tests ─────────────────────────────────────────────

    [Theory]
    [InlineData("CHEST", 100, 600, DoseWarnLevel.Low)]       // DAP=12 -> Low (10 < 12 <= 15)
    [InlineData("CHEST", 120, 520, DoseWarnLevel.Low)]       // DAP~14.98 -> Low
    [InlineData("CHEST", 120, 625, DoseWarnLevel.High)]      // DAP=18 -> High (15 < 18 <= 20)
    [InlineData("CHEST", 120, 694, DoseWarnLevel.High)]      // DAP~20.0 -> High (at 2x boundary)
    public async Task ValidateExposure_WarnSubLevels_ClassifiedCorrectly(
        string bodyPart, double kvp, double mas, DoseWarnLevel expectedWarnLevel)
    {
        var parameters = WithoutPatient(bodyPart, kvp, mas);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(expectedWarnLevel);
    }

    // ── 4-Level Boundary Value Tests (Safety-Critical) ────────────────────────

    [Theory]
    [InlineData(80, 5, "CHEST", DoseValidationLevel.Allow)]       // DAP=0.064, well below DRL
    [InlineData(120, 1050, "CHEST", DoseValidationLevel.Block)]   // DAP~30.24, between 2x and 5x
    [InlineData(120, 3000, "CHEST", DoseValidationLevel.Emergency)] // DAP=86.4, above 5x
    public async Task ValidateExposure_BoundaryValues_AllFourLevels(
        double kvp, double mas, string bodyPart, DoseValidationLevel expectedLevel)
    {
        var parameters = WithoutPatient(bodyPart, kvp, mas);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(expectedLevel);
    }

    // ── Cumulative DAP Threshold Tests ────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_CumulativeDapBelow3xDrl_RemainsAllow()
    {
        // CHEST DRL=10, 3xDRL=30
        // Prior records: 2x10 = 20 cumulative
        // New exposure: DAP=0.064 -> cumulative = 20.064 < 30 -> still Allow
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", 10.0),
            CreateDoseRecord("CHEST", 10.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        var parameters = WithPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.CumulativeDap.Should().BeApproximately(20.0, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_CumulativeDapExceeds3xDrl_ForcesBlock()
    {
        // CHEST DRL=10, 3xDRL=30
        // Prior records: 3x10 = 30 cumulative
        // New exposure: DAP=0.064 -> cumulative = 30.064 > 30 -> forced Block
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", 10.0),
            CreateDoseRecord("CHEST", 10.0),
            CreateDoseRecord("CHEST", 10.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        var parameters = WithPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse();
        result.Value.CumulativeDap.Should().BeApproximately(30.0, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_CumulativeDapExceeds3xDrl_ForcesBlockFromWarn()
    {
        // CHEST DRL=10, 3xDRL=30
        // Prior records: 2x12 = 24 cumulative
        // New exposure: DAP~12 -> WarnLow normally, but cumulative 24+12=36 > 30 -> forced Block
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", 12.0),
            CreateDoseRecord("CHEST", 12.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0
        var parameters = WithPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateExposure_CumulativeDapExactAt3xDrl_RemainsOriginalLevel()
    {
        // CHEST DRL=10, 3xDRL=30
        // Prior records: 1x25 = 25 cumulative
        // New exposure: DAP=5 -> cumulative 25+5=30 = exactly 3xDRL -> NOT forced Block (<= is not >)
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", 25.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=50, mAs=1000 -> DAP = (2500*1000)/500000 = 5.0
        var parameters = WithPatient("CHEST", 50, 1000);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.CumulativeDap.Should().BeApproximately(25.0, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_CumulativeDapFiltersByBodyPart()
    {
        // CHEST DRL=10, 3xDRL=30
        // Prior records: 2 ABDOMEN (should not count for CHEST) + 1 CHEST
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("ABDOMEN", 20.0),
            CreateDoseRecord("ABDOMEN", 20.0),
            CreateDoseRecord("CHEST", 10.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0 -> cumulative CHEST=10+12=22 < 30 -> WarnLow
        var parameters = WithPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.Low);
        result.Value.CumulativeDap.Should().BeApproximately(10.0, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_EmptyHistory_CumulativeDapIsZero()
    {
        SetupEmptyCumulativeHistory();

        var parameters = WithPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.CumulativeDap.Should().Be(0.0);
    }

    [Fact]
    public async Task ValidateExposure_RepositoryFailure_CumulativeDapIsZero()
    {
        _repository.GetByPatientAsync(Arg.Any<string>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<DoseRecord>>(ErrorCode.DatabaseError, "DB error"));

        var parameters = WithPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.CumulativeDap.Should().Be(0.0);
    }

    [Fact]
    public async Task ValidateExposure_CumulativeDapUsesDapMgyCm2_WhenAvailable()
    {
        // When DapMgyCm2 is set, it should be used instead of Dap for cumulative calculation
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", dap: 5.0, dapMgyCm2: 25.0), // DapMgyCm2 overrides Dap
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0 -> cumulative 25+12=37 > 30 -> forced Block
        var parameters = WithPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.CumulativeDap.Should().BeApproximately(25.0, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_NoPatientId_SkipsCumulativeCheck()
    {
        // Without PatientId, cumulative DAP should be 0 and no repository call made
        var parameters = WithoutPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.CumulativeDap.Should().Be(0.0);
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        await _repository.DidNotReceive().GetByPatientAsync(
            Arg.Any<string>(), Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>());
    }

    // ── GetCumulativeDapAsync Tests ───────────────────────────────────────────

    [Fact]
    public async Task GetCumulativeDapAsync_ValidPatient_ReturnsSum()
    {
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("CHEST", 5.0),
            CreateDoseRecord("CHEST", 8.0),
            CreateDoseRecord("ABDOMEN", 10.0), // Different body part
        }.AsReadOnly();
        _repository.GetByPatientAsync(PatientId, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(history));

        var result = await _sut.GetCumulativeDapAsync(PatientId, "CHEST");

        result.Should().BeApproximately(13.0, 0.01);
    }

    [Fact]
    public async Task GetCumulativeDapAsync_NoRecords_ReturnsZero()
    {
        _repository.GetByPatientAsync(PatientId, Arg.Any<DateTimeOffset?>(), Arg.Any<DateTimeOffset?>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(new List<DoseRecord>().AsReadOnly()));

        var result = await _sut.GetCumulativeDapAsync(PatientId, "CHEST");

        result.Should().Be(0.0);
    }

    [Fact]
    public async Task GetCumulativeDapAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetCumulativeDapAsync(null!, "CHEST");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── TriggerInterlockAsync Tests ───────────────────────────────────────────

    [Fact]
    public async Task TriggerInterlock_Emergency_SetsSafetyFlag()
    {
        _sut.IsEmergencySafetyActive.Should().BeFalse();

        var result = await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.4");

        result.IsSuccess.Should().BeTrue();
        _sut.IsEmergencySafetyActive.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerInterlock_Emergency_RequiresPhysicalReset()
    {
        DoseInterlockEventArgs? captured = null;
        _sut.InterlockTriggered += (_, e) => captured = e;

        await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.4");

        captured.Should().NotBeNull();
        captured!.Level.Should().Be(DoseValidationLevel.Emergency);
        captured.StudyInstanceUid.Should().Be("1.2.3.4");
        captured.RequiresPhysicalReset.Should().BeTrue();
        captured.Reason.Should().Contain("EMERGENCY");
        captured.Reason.Should().Contain("Physical reset required");
        captured.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TriggerInterlock_Block_DoesNotSetSafetyFlag()
    {
        var result = await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.5");

        result.IsSuccess.Should().BeTrue();
        _sut.IsEmergencySafetyActive.Should().BeFalse();
    }

    [Fact]
    public async Task TriggerInterlock_Block_PublishesEvent()
    {
        DoseInterlockEventArgs? captured = null;
        _sut.InterlockTriggered += (_, e) => captured = e;

        await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.5");

        captured.Should().NotBeNull();
        captured!.Level.Should().Be(DoseValidationLevel.Block);
        captured.StudyInstanceUid.Should().Be("1.2.3.5");
        captured.RequiresPhysicalReset.Should().BeFalse();
        captured.Reason.Should().Contain("blocked");
    }

    [Fact]
    public async Task TriggerInterlock_Allow_ReturnsValidationFailure()
    {
        var result = await _sut.TriggerInterlockAsync(DoseValidationLevel.Allow, "1.2.3.4");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task TriggerInterlock_Warn_ReturnsValidationFailure()
    {
        var result = await _sut.TriggerInterlockAsync(DoseValidationLevel.Warn, "1.2.3.4");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task TriggerInterlock_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task TriggerInterlock_MultipleSubscribers_AllReceiveEvent()
    {
        DoseInterlockEventArgs? captured1 = null;
        DoseInterlockEventArgs? captured2 = null;
        _sut.InterlockTriggered += (_, e) => captured1 = e;
        _sut.InterlockTriggered += (_, e) => captured2 = e;

        await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.4");

        captured1.Should().NotBeNull();
        captured2.Should().NotBeNull();
        captured1!.StudyInstanceUid.Should().Be(captured2!.StudyInstanceUid);
    }

    [Fact]
    public async Task TriggerInterlock_NoSubscriber_DoesNotThrow()
    {
        var act = async () => await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.4");

        await act.Should().NotThrowAsync();
    }

    // ── Emergency Safety Flag Tests ───────────────────────────────────────────

    [Fact]
    public void IsEmergencySafetyActive_InitiallyFalse()
    {
        _sut.IsEmergencySafetyActive.Should().BeFalse();
    }

    [Fact]
    public async Task TriggerInterlock_EmergencyTwice_SafetyFlagStaysTrue()
    {
        await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.4");
        await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.5");

        _sut.IsEmergencySafetyActive.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerInterlock_BlockAfterEmergency_SafetyFlagRemainsTrue()
    {
        await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.4");
        await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.5");

        _sut.IsEmergencySafetyActive.Should().BeTrue();
    }

    // ── All Body Parts with Realistic Dose Values ─────────────────────────────

    [Theory]
    [InlineData("CHEST", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("CHEST", 100, 600, DoseValidationLevel.Warn)]
    [InlineData("CHEST", 120, 1050, DoseValidationLevel.Block)]
    [InlineData("CHEST", 120, 3000, DoseValidationLevel.Emergency)]
    [InlineData("ABDOMEN", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("ABDOMEN", 120, 1200, DoseValidationLevel.Warn)]
    [InlineData("SPINE", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("SPINE", 120, 2000, DoseValidationLevel.Warn)]
    [InlineData("HAND", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("PELVIS", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("SKULL", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("KNEE", 80, 2, DoseValidationLevel.Allow)]
    [InlineData("SHOULDER", 80, 2, DoseValidationLevel.Allow)]
    public async Task ValidateExposure_AllBodyParts_RealisticDoseValues(
        string bodyPart, double kvp, double mas, DoseValidationLevel expectedLevel)
    {
        var parameters = WithoutPatient(bodyPart, kvp, mas);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(expectedLevel);
    }

    // ── Message Content Verification ──────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_WarnLowMessage_ContainsProceedWithCaution()
    {
        var parameters = WithoutPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Message.Should().Contain("Proceed with caution");
        result.Value.Message.Should().Contain("Cumulative DAP");
    }

    [Fact]
    public async Task ValidateExposure_WarnHighMessage_ContainsAcknowledgmentRequired()
    {
        var parameters = WithoutPatient("CHEST", 120, 625);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Message.Should().Contain("Acknowledgment required");
        result.Value.Message.Should().Contain("exceeds DRL");
        result.Value.Message.Should().Contain("HIGH warning zone");
    }

    [Fact]
    public async Task ValidateExposure_BlockMessage_ContainsBlocked()
    {
        var parameters = WithoutPatient("CHEST", 120, 1050);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Message.Should().Contain("blocked");
    }

    [Fact]
    public async Task ValidateExposure_EmergencyMessage_ContainsEmergency()
    {
        var parameters = WithoutPatient("CHEST", 120, 3000);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Message.Should().Contain("EMERGENCY");
    }

    [Fact]
    public async Task ValidateExposure_AllowLevel_NoMessage()
    {
        var parameters = WithoutPatient("CHEST", 80, 5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Message.Should().BeNull();
    }

    // ── Edge Cases ────────────────────────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_ExactlyAt1xDrl_ReturnsAllow()
    {
        // CHEST DRL=10. Need DAP exactly 10.0
        // kVp=100, mAs=500 -> DAP = (10000*500)/500000 = 10.0
        var parameters = WithoutPatient("CHEST", 100, 500);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.EstimatedDap.Should().BeApproximately(10.0, 0.001);
    }

    [Fact]
    public async Task ValidateExposure_JustOver1xDrl_ReturnsWarn()
    {
        // CHEST DRL=10. DAP slightly above 10.
        // kVp=100, mAs=501 -> DAP = (10000*501)/500000 = 10.02
        var parameters = WithoutPatient("CHEST", 100, 501);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.Low);
    }

    [Fact]
    public async Task ValidateExposure_ExactlyAt1_5xDrl_ReturnsWarnLow()
    {
        // CHEST DRL=10. 1.5xDRL = 15.0
        // kVp=100, mAs=750 -> DAP = (10000*750)/500000 = 15.0
        var parameters = WithoutPatient("CHEST", 100, 750);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.Low);
    }

    [Fact]
    public async Task ValidateExposure_JustOver1_5xDrl_ReturnsWarnHigh()
    {
        // CHEST DRL=10. Slightly above 1.5xDRL = 15.0
        // kVp=100, mAs=751 -> DAP = (10000*751)/500000 = 15.02
        var parameters = WithoutPatient("CHEST", 100, 751);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.High);
    }

    [Fact]
    public async Task ValidateExposure_ExactlyAt2xDrl_ReturnsWarnHigh()
    {
        // CHEST DRL=10. 2xDRL = 20.0
        // kVp=100, mAs=1000 -> DAP = (10000*1000)/500000 = 20.0
        var parameters = WithoutPatient("CHEST", 100, 1000);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.WarnLevel.Should().Be(DoseWarnLevel.High);
    }

    [Fact]
    public async Task ValidateExposure_ExactlyAt5xDrl_ReturnsBlock()
    {
        // CHEST DRL=10. 5xDRL = 50.0
        // kVp=100, mAs=2500 -> DAP = (10000*2500)/500000 = 50.0
        var parameters = WithoutPatient("CHEST", 100, 2500);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    [Fact]
    public async Task ValidateExposure_JustOver5xDrl_ReturnsEmergency()
    {
        // CHEST DRL=10. Slightly above 5xDRL = 50.0
        // kVp=100, mAs=2501 -> DAP = (10000*2501)/500000 = 50.02
        var parameters = WithoutPatient("CHEST", 100, 2501);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
    }

    // ── Cumulative DAP with Various Body Parts ────────────────────────────────

    [Fact]
    public async Task ValidateExposure_Abdomen_CumulativeThresholdWorks()
    {
        // ABDOMEN DRL=25, 3xDRL=75
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("ABDOMEN", 40.0),
            CreateDoseRecord("ABDOMEN", 30.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0 -> cumulative 70+12=82 > 75 -> forced Block
        var parameters = WithPatient("ABDOMEN", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    [Fact]
    public async Task ValidateExposure_Spine_CumulativeThresholdWorks()
    {
        // SPINE DRL=40, 3xDRL=120
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("SPINE", 60.0),
            CreateDoseRecord("SPINE", 50.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=120, mAs=600 -> DAP=17.28 -> cumulative 110+17.28=127.28 > 120 -> forced Block
        var parameters = WithPatient("SPINE", 120, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    // ── Unlisted Body Part Uses Default DRL ──────────────────────────────────

    [Fact]
    public async Task ValidateExposure_UnlistedBodyPart_CumulativeUsesDefaultDrl()
    {
        // Unknown body part -> default DRL=20, 3xDRL=60
        var history = new List<DoseRecord>
        {
            CreateDoseRecord("ELBOW", 30.0),
            CreateDoseRecord("ELBOW", 25.0),
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0 -> cumulative 55+12=67 > 60 -> forced Block
        var parameters = WithPatient("ELBOW", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    // ── Interlock Event Sequence ──────────────────────────────────────────────

    [Fact]
    public async Task TriggerInterlock_EmergencyThenBlock_BothEventsFired()
    {
        var events = new List<DoseInterlockEventArgs>();
        _sut.InterlockTriggered += (_, e) => events.Add(e);

        await _sut.TriggerInterlockAsync(DoseValidationLevel.Emergency, "1.2.3.4");
        await _sut.TriggerInterlockAsync(DoseValidationLevel.Block, "1.2.3.5");

        events.Should().HaveCount(2);
        events[0].Level.Should().Be(DoseValidationLevel.Emergency);
        events[0].RequiresPhysicalReset.Should().BeTrue();
        events[1].Level.Should().Be(DoseValidationLevel.Block);
        events[1].RequiresPhysicalReset.Should().BeFalse();
    }

    // ── Cumulative DAP with record having zero DapMgyCm2 ─────────────────────

    [Fact]
    public async Task ValidateExposure_CumulativeDapFallsBackToDapField()
    {
        // Record with DapMgyCm2=0 should use Dap field
        var history = new List<DoseRecord>
        {
            new DoseRecord(
                DoseId: "D1",
                StudyInstanceUid: "1.2.3.1",
                Dap: 15.0,
                Ei: 1000,
                EffectiveDose: 0.05,
                BodyPart: "CHEST",
                RecordedAt: DateTimeOffset.UtcNow.AddMinutes(-30),
                PatientId: PatientId,
                DapMgyCm2: 0.0), // DapMgyCm2 is zero, should fall back to Dap=15
        }.AsReadOnly();
        SetupCumulativeHistory(history);

        // kVp=100, mAs=600 -> DAP=12.0 -> cumulative 15+12=27 < 30 -> Allow
        var parameters = WithPatient("CHEST", 100, 600);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.CumulativeDap.Should().BeApproximately(15.0, 0.01);
    }
}
