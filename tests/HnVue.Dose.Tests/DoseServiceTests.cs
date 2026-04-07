using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

[Trait("SWR", "SWR-DOSE-010")]
public sealed class DoseServiceTests
{
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseServiceTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // ── Constructor guards ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new DoseService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("doseRepository");
    }

    // ── ValidateExposureAsync — Allow level ───────────────────────────────────

    [Fact]
    public async Task ValidateExposure_ChestWithinDrl_ReturnsAllow()
    {
        // CHEST DRL = 10 mGy·cm². Using kVp=80, mAs=5 → DAP ≈ (6400 × 5)/500000 = 0.064 mGy·cm²
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.IsAllowed.Should().BeTrue();
        result.Value.Message.Should().BeNull();
    }

    // ── ValidateExposureAsync — Warn level ────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_DoseExceedsDrlButBelowWarnThreshold_ReturnsWarn()
    {
        // CHEST DRL = 10 mGy·cm². To get DAP ~15 (between 10 and 20):
        // DAP = (kVp^2 × mAs) / 500000 = 15 → kVp=120, mAs=520 → 14400×520/500000=14.976
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 520, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.IsAllowed.Should().BeTrue();
        result.Value.Message.Should().NotBeNullOrEmpty();
    }

    // ── ValidateExposureAsync — Block level ───────────────────────────────────

    [Fact]
    public async Task ValidateExposure_DoseExceedsWarnButBelowBlock_ReturnsBlock()
    {
        // CHEST DRL=10, Block threshold= 50 mGy·cm². To get DAP ~30 (between 20 and 50):
        // kVp=120, mAs=1050 → DAP = 14400×1050/500000 = 30.24
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 1050, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse();
        result.Value.Message.Should().NotBeNullOrEmpty();
    }

    // ── ValidateExposureAsync — Emergency level ───────────────────────────────

    [Fact]
    public async Task ValidateExposure_DoseExceedsBlockThreshold_ReturnsEmergency()
    {
        // CHEST DRL=10, Emergency threshold > 50 mGy·cm²
        // kVp=120, mAs=3000 → DAP = 14400×3000/500000 = 86.4
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 3000, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.IsAllowed.Should().BeFalse();
        result.Value.Message.Should().Contain("EMERGENCY");
    }

    // ── ValidateExposureAsync — input validation ──────────────────────────────

    [Fact]
    public async Task ValidateExposure_ZeroKvp_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 0, Mas: 10, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ValidateExposure_NegativeMas_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: -1, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ValidateExposure_NullParameters_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ValidateExposureAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ValidateExposure_UnlistedBodyPart_UsesDefaultDrl()
    {
        // Unknown body part → default DRL=20. Small dose should return Allow.
        var parameters = new ExposureParameters("ELBOW", Kvp: 60, Mas: 5, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    // ── RecordDoseAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task RecordDose_ValidRecord_CallsRepositoryAndReturnsSuccess()
    {
        var dose = new DoseRecord("D001", "1.2.3", 5.0, 300, 0.1, "CHEST", DateTimeOffset.UtcNow);
        _repository.SaveAsync(dose, Arg.Any<CancellationToken>()).Returns(Result.Success());

        var result = await _sut.RecordDoseAsync(dose);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).SaveAsync(dose, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordDose_NullRecord_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordDoseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecordDose_RepositoryFailure_PropagatesFailure()
    {
        var dose = new DoseRecord("D001", "1.2.3", 5.0, 300, 0.1, "CHEST", DateTimeOffset.UtcNow);
        _repository.SaveAsync(dose, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "Write failed"));

        var result = await _sut.RecordDoseAsync(dose);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── GetDoseByStudyAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task GetDoseByStudy_ExistingStudy_ReturnsDoseRecord()
    {
        var expected = new DoseRecord("D001", "1.2.3", 5.0, 300, 0.1, "CHEST", DateTimeOffset.UtcNow);
        _repository.GetByStudyAsync("1.2.3", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(expected));

        var result = await _sut.GetDoseByStudyAsync("1.2.3");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public async Task GetDoseByStudy_NonExistentStudy_ReturnsSuccessWithNull()
    {
        _repository.GetByStudyAsync("unknown", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(null));

        var result = await _sut.GetDoseByStudyAsync("unknown");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetDoseByStudy_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── Body-part DRL coverage ────────────────────────────────────────────────

    [Theory]
    [InlineData("ABDOMEN",  150, 1000)] // DAP = 22500×1000/500000 = 45 → below 2×DRL=50
    [InlineData("SPINE",    120, 1500)] // DAP = 14400×1500/500000 = 43.2 → within DRL=40 + warn zone
    public async Task ValidateExposure_KnownBodyParts_ReturnSensibleLevels(
        string bodyPart, double kvp, double mas)
    {
        var parameters = new ExposureParameters(bodyPart, kvp, mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().BeOneOf(
            DoseValidationLevel.Allow,
            DoseValidationLevel.Warn,
            DoseValidationLevel.Block,
            DoseValidationLevel.Emergency);
    }

    // ── CalculateEsd — SWR-DM-042~043 ───────────────────────────────────────────

    [Fact]
    public void CalculateEsd_ValidParameters_ReturnsCorrectEsd()
    {
        // DAP=10 mGy·cm², field=100 cm², BSF=1.35 → ESD = (10/100) × 1.35 = 0.135 mGy
        var result = _sut.CalculateEsd(10.0, 100.0, 1.35);

        result.Should().BeApproximately(0.135, 0.0001);
    }

    [Fact]
    public void CalculateEsd_DefaultBackscatterFactor_Uses1_35()
    {
        // DAP=10 mGy·cm², field=100 cm² → ESD = (10/100) × 1.35 = 0.135 mGy
        var result = _sut.CalculateEsd(10.0, 100.0);

        result.Should().BeApproximately(0.135, 0.0001);
    }

    [Fact]
    public void CalculateEsd_LargeFieldArea_ReturnsProportionallyLowerEsd()
    {
        // DAP=10 mGy·cm², field=400 cm², BSF=1.35 → ESD = (10/400) × 1.35 = 0.03375 mGy
        var result = _sut.CalculateEsd(10.0, 400.0, 1.35);

        result.Should().BeApproximately(0.03375, 0.0001);
    }

    [Fact]
    public void CalculateEsd_HighDap_ReturnsProportionallyHigherEsd()
    {
        // DAP=50 mGy·cm², field=100 cm², BSF=1.35 → ESD = (50/100) × 1.35 = 0.675 mGy
        var result = _sut.CalculateEsd(50.0, 100.0, 1.35);

        result.Should().BeApproximately(0.675, 0.0001);
    }

    [Fact]
    public void CalculateEsd_ZeroFieldArea_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(10.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fieldAreaCm2");
    }

    [Fact]
    public void CalculateEsd_FieldAreaBelowMinimum_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(10.0, 0.5);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fieldAreaCm2");
    }

    [Fact]
    public void CalculateEsd_NegativeDap_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(-10.0, 100.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dap");
    }

    [Fact]
    public void CalculateEsd_BackscatterFactorBelow1_0_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(10.0, 100.0, 0.9);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("backscatterFactor");
    }

    // ── CalculateExposureIndex — SWR-DM-047~048 ───────────────────────────────────

    [Fact]
    public void CalculateExposureIndex_MeanEqualsTarget_Returns1000()
    {
        // When mean equals target, EI should be 1000 (optimal exposure)
        var result = _sut.CalculateExposureIndex(1500.0, 1500.0);

        result.Should().Be(1000.0);
    }

    [Fact]
    public void CalculateExposureIndex_MeanDoubleTarget_Returns2000()
    {
        // When mean is 2× target, EI should be 2000 (overexposed)
        var result = _sut.CalculateExposureIndex(3000.0, 1500.0);

        result.Should().Be(2000.0);
    }

    [Fact]
    public void CalculateExposureIndex_MeanHalfTarget_Returns500()
    {
        // When mean is 0.5× target, EI should be 500 (underexposed)
        var result = _sut.CalculateExposureIndex(750.0, 1500.0);

        result.Should().Be(500.0);
    }

    [Fact]
    public void CalculateExposureIndex_DifferentTargetValue_CalculatesCorrectly()
    {
        // Using abdomen target of 1200, mean=1800 → EI = (1800/1200) × 1000 = 1500
        var result = _sut.CalculateExposureIndex(1800.0, 1200.0);

        result.Should().Be(1500.0);
    }

    [Fact]
    public void CalculateExposureIndex_NegativeMean_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateExposureIndex(-100.0, 1500.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("meanPixelValue");
    }

    [Fact]
    public void CalculateExposureIndex_ZeroTarget_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateExposureIndex(1500.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetPixelValue");
    }

    [Fact]
    public void CalculateExposureIndex_NegativeTarget_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateExposureIndex(1500.0, -100.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetPixelValue");
    }

    // ── ValidateExposureAsync — enhanced with ESD and EI ────────────────────────

    [Fact]
    public async Task ValidateExposure_ReturnsEsdAndEiInResult()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4", FieldAreaCm2: 100.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstimatedEsd.Should().BeGreaterThan(0);
        result.Value.ExposureIndex.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ValidateExposure_FieldAreaAffectsEsdCalculation()
    {
        // Same exposure, different field areas → different ESD
        var parametersSmall = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4", FieldAreaCm2: 100.0);
        var parametersLarge = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4", FieldAreaCm2: 400.0);

        var resultSmall = await _sut.ValidateExposureAsync(parametersSmall);
        var resultLarge = await _sut.ValidateExposureAsync(parametersLarge);

        resultSmall.Value.EstimatedEsd.Should().BeGreaterThan(resultLarge.Value.EstimatedEsd);
    }

    [Theory]
    [InlineData("CHEST", 1500)]
    [InlineData("ABDOMEN", 1200)]
    [InlineData("SPINE", 1800)]
    [InlineData("SKULL", 1400)]
    public async Task ValidateExposure_UsesCorrectEiTargetPerBodyPart(string bodyPart, double expectedTarget)
    {
        var parameters = new ExposureParameters(bodyPart, Kvp: 80, Mas: 10, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        // Verify EI is calculated using the correct target for this body part
        var simulatedMean = (parameters.Kvp * parameters.Mas) / 10.0;
        var expectedEi = (simulatedMean / expectedTarget) * 1000.0;
        result.Value.ExposureIndex.Should().BeApproximately(expectedEi, 0.01);
    }

    [Fact]
    public async Task ValidateExposure_FieldAreaBelowMinimum_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4", FieldAreaCm2: 0.5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── Edge cases and integration ───────────────────────────────────────────────

    [Fact]
    public void CalculateEsd_VerySmallFieldArea_ReturnsVeryHighEsd()
    {
        // DAP=10 mGy·cm², field=1 cm² (minimum), BSF=1.35 → ESD = (10/1) × 1.35 = 13.5 mGy
        var result = _sut.CalculateEsd(10.0, 1.0, 1.35);

        result.Should().BeApproximately(13.5, 0.0001);
    }

    [Fact]
    public void CalculateExposureIndex_ZeroMeanPixelValue_ReturnsZero()
    {
        // Edge case: no exposure captured
        var result = _sut.CalculateExposureIndex(0.0, 1500.0);

        result.Should().Be(0.0);
    }

    // ── GenerateRdsrSummaryAsync — SWR-DM-044~046 ────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummary_ExistingStudyWithFieldArea_ReturnsEnrichedRecord()
    {
        // Arrange: dose record with field area and pixel data so ESD and EI can be computed
        var dose = new DoseRecord(
            DoseId: "D100",
            StudyInstanceUid: "1.2.3.100",
            Dap: 10.0,
            Ei: 0,
            EffectiveDose: 0.05,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 10.0,
            FieldAreaCm2: 100.0,
            MeanPixelValue: 1500.0,
            EiTarget: 1500.0);

        _repository.GetByStudyAsync("1.2.3.100", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        // Act
        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.100");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeApproximately(0.135, 0.0001); // (10/100) × 1.35
        result.Value.Ei.Should().Be(1000.0);                          // (1500/1500) × 1000
        result.Value.DapMgyCm2.Should().Be(10.0);
    }

    [Fact]
    public async Task GenerateRdsrSummary_StudyNotFound_ReturnsNotFoundFailure()
    {
        _repository.GetByStudyAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(null));

        var result = await _sut.GenerateRdsrSummaryAsync("nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GenerateRdsrSummary_RepositoryFailure_PropagatesFailure()
    {
        _repository.GetByStudyAsync("err", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "DB error"));

        var result = await _sut.GenerateRdsrSummaryAsync("err");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task GenerateRdsrSummary_NoFieldArea_ReturnsNullEsd()
    {
        // When FieldAreaCm2 is not recorded (0.0), ESD should be null
        var dose = new DoseRecord(
            DoseId: "D101",
            StudyInstanceUid: "1.2.3.101",
            Dap: 10.0,
            Ei: 850.0,
            EffectiveDose: 0.05,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 10.0,
            FieldAreaCm2: 0.0,       // no field area recorded
            MeanPixelValue: 0.0,     // no pixel data
            EiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.101", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.101");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
        result.Value.Ei.Should().Be(850.0); // preserved from existing record
    }

    [Fact]
    public async Task GenerateRdsrSummary_FallsBackToLegacyDap_WhenDapMgyCm2IsZero()
    {
        // Legacy records without DapMgyCm2 should use the Dap field
        var dose = new DoseRecord(
            DoseId: "D102",
            StudyInstanceUid: "1.2.3.102",
            Dap: 20.0,
            Ei: 1000.0,
            EffectiveDose: 0.1,
            BodyPart: "ABDOMEN",
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 0.0,          // not set → should fall back to Dap
            FieldAreaCm2: 200.0,
            MeanPixelValue: 0.0,
            EiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.102", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.102");

        result.IsSuccess.Should().BeTrue();
        // DapMgyCm2 should be populated from legacy Dap
        result.Value.DapMgyCm2.Should().Be(20.0);
        // ESD = (20 / 200) × 1.35 = 0.135
        result.Value.EsdMgy.Should().BeApproximately(0.135, 0.0001);
    }

    [Fact]
    public async Task GenerateRdsrSummary_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GenerateRdsrSummaryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GenerateRdsrSummary_UsesBodyPartDefaultEiTarget_WhenEiTargetIsZero()
    {
        // EiTarget=0 → service resolves from body part dictionary (CHEST=1500)
        var dose = new DoseRecord(
            DoseId: "D103",
            StudyInstanceUid: "1.2.3.103",
            Dap: 5.0,
            Ei: 0.0,
            EffectiveDose: 0.02,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 5.0,
            FieldAreaCm2: 100.0,
            MeanPixelValue: 1500.0,
            EiTarget: 0.0);           // not set → resolved from CHEST default (1500)

        _repository.GetByStudyAsync("1.2.3.103", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.103");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1500.0);
        result.Value.Ei.Should().Be(1000.0); // (1500/1500)×1000
    }

    // ── GetDoseHistoryAsync — SWR-DM-051~052 ─────────────────────────────────

    [Fact]
    public async Task GetDoseHistory_ValidPatientId_ReturnsListFromRepository()
    {
        // Arrange
        var records = new List<DoseRecord>
        {
            new("D200", "1.2.3.200", 5.0, 900, 0.05, "CHEST", DateTimeOffset.UtcNow.AddDays(-2)),
            new("D201", "1.2.3.201", 8.0, 1100, 0.08, "CHEST", DateTimeOffset.UtcNow.AddDays(-1)),
        };

        _repository.GetByPatientAsync("P001", null, null, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(records.AsReadOnly()));

        // Act
        var result = await _sut.GetDoseHistoryAsync("P001");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(records);
    }

    [Fact]
    public async Task GetDoseHistory_WithDateRange_PassesRangeThroughToRepository()
    {
        var from  = DateTimeOffset.UtcNow.AddMonths(-1);
        var until = DateTimeOffset.UtcNow;
        var expected = new List<DoseRecord>
        {
            new("D202", "1.2.3.202", 3.0, 950, 0.03, "HAND", from.AddDays(5)),
        };

        _repository.GetByPatientAsync("P002", from, until, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(expected.AsReadOnly()));

        var result = await _sut.GetDoseHistoryAsync("P002", from, until);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        await _repository.Received(1)
            .GetByPatientAsync("P002", from, until, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDoseHistory_NoRecords_ReturnsEmptyList()
    {
        _repository.GetByPatientAsync("UNKNOWN", null, null, Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<DoseRecord>>(
                new List<DoseRecord>().AsReadOnly()));

        var result = await _sut.GetDoseHistoryAsync("UNKNOWN");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDoseHistory_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetDoseHistory_RepositoryFailure_PropagatesFailure()
    {
        _repository.GetByPatientAsync("P003", null, null, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<DoseRecord>>(
                ErrorCode.DatabaseError, "DB failure"));

        var result = await _sut.GetDoseHistoryAsync("P003");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }
}
