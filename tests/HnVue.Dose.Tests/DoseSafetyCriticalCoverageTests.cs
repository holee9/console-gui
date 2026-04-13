using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// Comprehensive safety-critical branch coverage tests for <see cref="DoseService"/>.
/// Targets 90%+ branch coverage as mandated by DOC-012 for IEC 62304 Class B modules.
///
/// Coverage areas:
/// - All 4 dose levels across ALL 9 body parts in DoseReferenceLevels
/// - DefaultDrl fallback for unlisted body parts at all 4 levels
/// - GenerateRdsrSummaryAsync all branch combinations
/// - CalculateEsd boundary and edge cases
/// - CalculateExposureIndex boundary and edge cases
/// - Null parameter validation for all public methods
/// </summary>
public sealed class DoseSafetyCriticalCoverageTests
{
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseSafetyCriticalCoverageTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // =========================================================================
    // 1. ALL 4 DOSE LEVELS FOR EVERY BODY PART IN DoseReferenceLevels
    // =========================================================================
    // DAP = (kVp^2 * mAs) / 500000
    // Strategy: use kVp=100 so kVp^2=10000, then solve for mAs to hit target DAP
    // DAP = (10000 * mAs) / 500000 = mAs / 50
    // Therefore mAs = DAP * 50

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]   // DRL=10
    [InlineData("ABDOMEN",  25.0)]   // DRL=25
    [InlineData("PELVIS",   25.0)]   // DRL=25
    [InlineData("HAND",      3.0)]   // DRL=3
    [InlineData("FOOT",      3.0)]   // DRL=3
    [InlineData("SPINE",    40.0)]   // DRL=40
    [InlineData("SKULL",    30.0)]   // DRL=30
    [InlineData("KNEE",      5.0)]   // DRL=5
    [InlineData("SHOULDER", 15.0)]   // DRL=15
    public async Task ValidateExposure_AllBodyParts_AllowLevelAtExactDrl(
        string bodyPart, double drl)
    {
        // DAP exactly at DRL -> Allow (estimatedDap <= drl)
        var mas = drl * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.IsAllowed.Should().BeTrue();
        result.Value.EstimatedDap.Should().BeApproximately(drl, 0.01);
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]
    [InlineData("ABDOMEN",  25.0)]
    [InlineData("PELVIS",   25.0)]
    [InlineData("HAND",      3.0)]
    [InlineData("FOOT",      3.0)]
    [InlineData("SPINE",    40.0)]
    [InlineData("SKULL",    30.0)]
    [InlineData("KNEE",      5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllBodyParts_WarnLevelBetweenDrlAnd2xDrl(
        string bodyPart, double drl)
    {
        // DAP at 1.5x DRL -> Warn (between 1x and 2x)
        var targetDap = drl * 1.5;
        var mas = targetDap * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.IsAllowed.Should().BeTrue();
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]
    [InlineData("ABDOMEN",  25.0)]
    [InlineData("PELVIS",   25.0)]
    [InlineData("HAND",      3.0)]
    [InlineData("FOOT",      3.0)]
    [InlineData("SPINE",    40.0)]
    [InlineData("SKULL",    30.0)]
    [InlineData("KNEE",      5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllBodyParts_WarnLevelAtExact2xDrl(
        string bodyPart, double drl)
    {
        // DAP exactly at 2x DRL -> Warn (estimatedDap <= drl * 2.0)
        var targetDap = drl * 2.0;
        var mas = targetDap * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.IsAllowed.Should().BeTrue();
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]
    [InlineData("ABDOMEN",  25.0)]
    [InlineData("PELVIS",   25.0)]
    [InlineData("HAND",      3.0)]
    [InlineData("FOOT",      3.0)]
    [InlineData("SPINE",    40.0)]
    [InlineData("SKULL",    30.0)]
    [InlineData("KNEE",      5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllBodyParts_BlockLevelBetween2xAnd5xDrl(
        string bodyPart, double drl)
    {
        // DAP at 3.5x DRL -> Block (between 2x and 5x)
        var targetDap = drl * 3.5;
        var mas = targetDap * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse();
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]
    [InlineData("ABDOMEN",  25.0)]
    [InlineData("PELVIS",   25.0)]
    [InlineData("HAND",      3.0)]
    [InlineData("FOOT",      3.0)]
    [InlineData("SPINE",    40.0)]
    [InlineData("SKULL",    30.0)]
    [InlineData("KNEE",      5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllBodyParts_BlockLevelAtExact5xDrl(
        string bodyPart, double drl)
    {
        // DAP exactly at 5x DRL -> Block (estimatedDap <= drl * 5.0)
        var targetDap = drl * 5.0;
        var mas = targetDap * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.IsAllowed.Should().BeFalse();
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("CHEST",    10.0)]
    [InlineData("ABDOMEN",  25.0)]
    [InlineData("PELVIS",   25.0)]
    [InlineData("HAND",      3.0)]
    [InlineData("FOOT",      3.0)]
    [InlineData("SPINE",    40.0)]
    [InlineData("SKULL",    30.0)]
    [InlineData("KNEE",      5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllBodyParts_EmergencyLevelAbove5xDrl(
        string bodyPart, double drl)
    {
        // DAP at 6x DRL -> Emergency (above 5x)
        var targetDap = drl * 6.0;
        var mas = targetDap * 50;
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.IsAllowed.Should().BeFalse();
    }

    // =========================================================================
    // 2. DefaultDrl FALLBACK FOR UNLISTED BODY PARTS (all 4 levels)
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("NECK")]
    [InlineData("ELBOW")]
    [InlineData("WRIST")]
    [InlineData("ANKLE")]
    [InlineData("FINGER")]
    [InlineData("UNKNOWN")]
    public async Task ValidateExposure_UnlistedBodyPart_AllowLevelWithLowDose(string bodyPart)
    {
        // Default DRL=20, DAP below 20 -> Allow
        var parameters = new ExposureParameters(bodyPart, Kvp: 80, Mas: 10, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("NECK")]
    [InlineData("ELBOW")]
    [InlineData("WRIST")]
    public async Task ValidateExposure_UnlistedBodyPart_WarnLevel(string bodyPart)
    {
        // Default DRL=20, need DAP between 20 and 40
        // DAP = (kVp^2 * mAs) / 500000 = 30 -> kVp=100, mAs=1500 -> 10000*1500/500000=30
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 1500, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("NECK")]
    [InlineData("ELBOW")]
    [InlineData("WRIST")]
    public async Task ValidateExposure_UnlistedBodyPart_BlockLevel(string bodyPart)
    {
        // Default DRL=20, need DAP between 40 and 100
        // DAP = 80 -> kVp=100, mAs=4000 -> 10000*4000/500000=80
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 4000, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("NECK")]
    [InlineData("ELBOW")]
    [InlineData("WRIST")]
    public async Task ValidateExposure_UnlistedBodyPart_EmergencyLevel(string bodyPart)
    {
        // Default DRL=20, need DAP > 100 (5x DRL)
        // DAP = 150 -> kVp=100, mAs=7500 -> 10000*7500/500000=150
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 7500, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
    }

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData("neck")]
    [InlineData("Elbow")]
    [InlineData("WRIST")]
    public async Task ValidateExposure_UnlistedBodyPart_CaseInsensitive(string bodyPart)
    {
        // Case insensitivity should work for unlisted parts too
        var parameters = new ExposureParameters(bodyPart, Kvp: 80, Mas: 10, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    // =========================================================================
    // 3. GenerateRdsrSummaryAsync — ALL BRANCH COMBINATIONS
    // =========================================================================

    // ── 3a. Repository failure propagation ──────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_RepositoryReturnsFailure_PropagatesErrorAndMessage()
    {
        _repository.GetByStudyAsync("fail-study", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "Connection timeout"));

        var result = await _sut.GenerateRdsrSummaryAsync("fail-study");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
        result.ErrorMessage.Should().Contain("Connection timeout");
    }

    // ── 3b. Null dose -> NotFound ──────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_RepositoryReturnsNull_ReturnsNotFound()
    {
        _repository.GetByStudyAsync("missing-study", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(null));

        var result = await _sut.GenerateRdsrSummaryAsync("missing-study");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
        result.ErrorMessage.Should().Contain("missing-study");
    }

    // ── 3c. DapMgyCm2 > 0 (RDSR path) vs = 0 (legacy Dap fallback) ───────

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_DapMgyCm2Positive_UsesRdsrDap()
    {
        var dose = CreateTestDoseRecord("1.2.3.rdsr", dap: 12.0, dapMgyCm2: 15.0,
            fieldAreaCm2: 100.0, meanPixelValue: 1500.0, eiTarget: 1500.0);

        _repository.GetByStudyAsync("1.2.3.rdsr", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.rdsr");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(15.0);  // Used DapMgyCm2, not Dap
        result.Value.EsdMgy.Should().BeApproximately(15.0 / 100.0 * 1.35, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_DapMgyCm2Zero_FallsBackToLegacyDap()
    {
        var dose = CreateTestDoseRecord("1.2.3.legacy", dap: 20.0, dapMgyCm2: 0.0,
            fieldAreaCm2: 200.0, meanPixelValue: 1500.0, eiTarget: 1500.0);

        _repository.GetByStudyAsync("1.2.3.legacy", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.legacy");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(20.0);  // Fell back to Dap
        result.Value.EsdMgy.Should().BeApproximately(20.0 / 200.0 * 1.35, 0.0001);
    }

    // ── 3d. FieldAreaCm2 >= 1.0 (ESD calculated) vs < 1.0 (ESD null) ──────

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public async Task GenerateRdsrSummary_FieldAreaAboveMinimum_CalculatesEsd()
    {
        var dose = CreateTestDoseRecord("1.2.3.esd-ok", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 50.0, meanPixelValue: 0.0, eiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.esd-ok", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.esd-ok");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().NotBeNull();
        result.Value.EsdMgy.Should().BeApproximately(10.0 / 50.0 * 1.35, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public async Task GenerateRdsrSummary_FieldAreaExactlyAtMinimum_CalculatesEsd()
    {
        // FieldAreaCm2 = 1.0 (exactly at minimum) -> should calculate ESD
        var dose = CreateTestDoseRecord("1.2.3.esd-min", dap: 5.0, dapMgyCm2: 5.0,
            fieldAreaCm2: 1.0, meanPixelValue: 0.0, eiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.esd-min", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.esd-min");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().NotBeNull();
        result.Value.EsdMgy.Should().BeApproximately(5.0 / 1.0 * 1.35, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public async Task GenerateRdsrSummary_FieldAreaBelowMinimum_EsdIsNull()
    {
        // FieldAreaCm2 = 0.5 (< 1.0) -> ESD should be null
        var dose = CreateTestDoseRecord("1.2.3.esd-no", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 0.5, meanPixelValue: 0.0, eiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.esd-no", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.esd-no");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public async Task GenerateRdsrSummary_FieldAreaZero_EsdIsNull()
    {
        var dose = CreateTestDoseRecord("1.2.3.esd-zero", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 0.0, meanPixelValue: 0.0, eiTarget: 0.0);

        _repository.GetByStudyAsync("1.2.3.esd-zero", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.esd-zero");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
    }

    // ── 3e. EiTarget > 0 (use record value) vs = 0 (body part fallback) ────

    [Fact]
    [Trait("SWR", "SWR-DM-047")]
    public async Task GenerateRdsrSummary_EiTargetPositive_UsesRecordTarget()
    {
        var dose = CreateTestDoseRecord("1.2.3.ei-rec", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 100.0, meanPixelValue: 1200.0, eiTarget: 1200.0);

        _repository.GetByStudyAsync("1.2.3.ei-rec", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.ei-rec");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1200.0);
        result.Value.Ei.Should().Be(1000.0);  // (1200/1200)*1000
    }

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData("CHEST", 1500.0)]
    [InlineData("ABDOMEN", 1200.0)]
    [InlineData("SPINE", 1800.0)]
    [InlineData("SKULL", 1400.0)]
    public async Task GenerateRdsrSummary_EiTargetZero_UsesBodyPartDefault(
        string bodyPart, double expectedTarget)
    {
        var dose = CreateTestDoseRecord("1.2.3.ei-bp", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 100.0, meanPixelValue: expectedTarget, eiTarget: 0.0,
            bodyPart: bodyPart);

        _repository.GetByStudyAsync("1.2.3.ei-bp", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.ei-bp");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(expectedTarget);
        result.Value.Ei.Should().Be(1000.0);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-047")]
    public async Task GenerateRdsrSummary_EiTargetZeroUnknownBodyPart_UsesDefault1500()
    {
        // Body part not in EI target dictionary -> default 1500
        var dose = CreateTestDoseRecord("1.2.3.ei-def", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 100.0, meanPixelValue: 1500.0, eiTarget: 0.0,
            bodyPart: "NECK");

        _repository.GetByStudyAsync("1.2.3.ei-def", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.ei-def");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1500.0);
        result.Value.Ei.Should().Be(1000.0);
    }

    // ── 3f. MeanPixelValue > 0 (compute EI) vs = 0 (preserve existing EI) ──

    [Fact]
    [Trait("SWR", "SWR-DM-047")]
    public async Task GenerateRdsrSummary_MeanPixelPositive_ComputesEiFromPixelData()
    {
        var dose = CreateTestDoseRecord("1.2.3.pixel-ok", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 100.0, meanPixelValue: 3000.0, eiTarget: 1500.0,
            existingEi: 500.0);

        _repository.GetByStudyAsync("1.2.3.pixel-ok", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.pixel-ok");

        result.IsSuccess.Should().BeTrue();
        result.Value.Ei.Should().Be(2000.0);  // (3000/1500)*1000, NOT the old 500
    }

    [Fact]
    [Trait("SWR", "SWR-DM-047")]
    public async Task GenerateRdsrSummary_MeanPixelZero_PreservesExistingEi()
    {
        var dose = CreateTestDoseRecord("1.2.3.pixel-no", dap: 10.0, dapMgyCm2: 10.0,
            fieldAreaCm2: 100.0, meanPixelValue: 0.0, eiTarget: 1500.0,
            existingEi: 850.0);

        _repository.GetByStudyAsync("1.2.3.pixel-no", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.pixel-no");

        result.IsSuccess.Should().BeTrue();
        result.Value.Ei.Should().Be(850.0);  // Preserved from existing record
    }

    // ── 3g. Full combination: all branches exercised in one record ──────────

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_AllFieldsPopulated_EnrichesCorrectly()
    {
        // Complete record: DapMgyCm2 > 0, FieldAreaCm2 >= 1, EiTarget > 0, MeanPixel > 0
        var dose = CreateTestDoseRecord("1.2.3.full", dap: 12.0, dapMgyCm2: 25.0,
            fieldAreaCm2: 200.0, meanPixelValue: 2400.0, eiTarget: 1200.0);

        _repository.GetByStudyAsync("1.2.3.full", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.full");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(25.0);
        result.Value.EsdMgy.Should().BeApproximately(25.0 / 200.0 * 1.35, 0.0001);
        result.Value.EiTarget.Should().Be(1200.0);
        result.Value.Ei.Should().Be(2000.0);  // (2400/1200)*1000
    }

    [Fact]
    [Trait("SWR", "SWR-DM-044")]
    public async Task GenerateRdsrSummary_MinimalFields_FallsBackGracefully()
    {
        // Minimal record: DapMgyCm2=0 (legacy), FieldAreaCm2=0 (no ESD), EiTarget=0 (body part), MeanPixel=0 (preserve EI)
        var dose = CreateTestDoseRecord("1.2.3.min", dap: 8.0, dapMgyCm2: 0.0,
            fieldAreaCm2: 0.0, meanPixelValue: 0.0, eiTarget: 0.0,
            existingEi: 720.0, bodyPart: "CHEST");

        _repository.GetByStudyAsync("1.2.3.min", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.min");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(8.0);  // Fell back to Dap
        result.Value.EsdMgy.Should().BeNull();     // No field area
        result.Value.EiTarget.Should().Be(1500.0);  // CHEST default
        result.Value.Ei.Should().Be(720.0);         // Preserved existing
    }

    // =========================================================================
    // 4. CalculateEsd — EDGE CASES AND BOUNDARY VALUES
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DM-042")]
    [InlineData(10.0, 100.0, 1.35, 0.135)]
    [InlineData(10.0, 100.0, 1.0, 0.1)]
    [InlineData(10.0, 100.0, 2.0, 0.2)]
    [InlineData(50.0, 200.0, 1.35, 0.3375)]
    [InlineData(0.0, 100.0, 1.35, 0.0)]
    [InlineData(100.0, 1.0, 1.35, 135.0)]
    [InlineData(1.0, 1.0, 1.0, 1.0)]
    public void CalculateEsd_ValidInputs_ReturnsCorrectResult(
        double dap, double fieldArea, double bsf, double expected)
    {
        var result = _sut.CalculateEsd(dap, fieldArea, bsf);

        result.Should().BeApproximately(expected, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public void CalculateEsd_DefaultBackscatterFactor_Is1_35()
    {
        // Calling without backscatterFactor should use 1.35
        var withExplicit = _sut.CalculateEsd(10.0, 100.0, 1.35);
        var withDefault = _sut.CalculateEsd(10.0, 100.0);

        withDefault.Should().BeApproximately(withExplicit, 0.0001);
    }

    [Theory]
    [Trait("SWR", "SWR-DM-042")]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.99)]
    [InlineData(-1.0)]
    [InlineData(-100.0)]
    public void CalculateEsd_FieldAreaBelowMinimum_ThrowsArgumentOutOfRangeException(double fieldArea)
    {
        var act = () => _sut.CalculateEsd(10.0, fieldArea);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("fieldAreaCm2");
    }

    [Theory]
    [Trait("SWR", "SWR-DM-042")]
    [InlineData(-0.1)]
    [InlineData(-1.0)]
    [InlineData(-100.0)]
    public void CalculateEsd_NegativeDap_ThrowsArgumentOutOfRangeException(double dap)
    {
        var act = () => _sut.CalculateEsd(dap, 100.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dap");
    }

    [Theory]
    [Trait("SWR", "SWR-DM-042")]
    [InlineData(0.5)]
    [InlineData(0.99)]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void CalculateEsd_BackscatterFactorBelow1_ThrowsArgumentOutOfRangeException(double bsf)
    {
        var act = () => _sut.CalculateEsd(10.0, 100.0, bsf);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("backscatterFactor");
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public void CalculateEsd_BackscatterFactorExactly1_0_IsAccepted()
    {
        var result = _sut.CalculateEsd(10.0, 100.0, 1.0);

        result.Should().BeApproximately(0.1, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public void CalculateEsd_LargeBackscatterFactor_ProducesProportionallyHigherEsd()
    {
        var result = _sut.CalculateEsd(10.0, 100.0, 3.0);

        result.Should().BeApproximately(0.3, 0.0001);
    }

    [Fact]
    [Trait("SWR", "SWR-DM-042")]
    public void CalculateEsd_VerySmallDapNearZero_ReturnsNearZero()
    {
        var result = _sut.CalculateEsd(0.001, 400.0, 1.35);

        result.Should().BeApproximately(0.001 / 400.0 * 1.35, 0.0000001);
    }

    // =========================================================================
    // 5. CalculateExposureIndex — BOUNDARY VALUES
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData(1500.0, 1500.0, 1000.0)]    // Exact match -> 1000
    [InlineData(3000.0, 1500.0, 2000.0)]    // 2x over -> 2000
    [InlineData(750.0,  1500.0, 500.0)]     // 0.5x -> 500
    [InlineData(0.0,    1500.0, 0.0)]       // Zero mean -> 0
    [InlineData(1.0,    1500.0, 0.6667)]    // Very low
    [InlineData(65535.0, 1500.0, 43690.0)]  // Max 16-bit pixel
    [InlineData(1500.0, 1200.0, 1250.0)]    // Different target
    [InlineData(1500.0, 1800.0, 833.333)]   // Higher target
    public void CalculateExposureIndex_ValidInputs_ReturnsCorrectResult(
        double mean, double target, double expected)
    {
        var result = _sut.CalculateExposureIndex(mean, target);

        result.Should().BeApproximately(expected, 0.01);
    }

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData(-0.001)]
    [InlineData(-1.0)]
    [InlineData(-1000.0)]
    public void CalculateExposureIndex_NegativeMean_ThrowsArgumentOutOfRangeException(double mean)
    {
        var act = () => _sut.CalculateExposureIndex(mean, 1500.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("meanPixelValue");
    }

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData(0.0)]
    [InlineData(-0.001)]
    [InlineData(-1.0)]
    [InlineData(-1000.0)]
    public void CalculateExposureIndex_ZeroOrNegativeTarget_ThrowsArgumentOutOfRangeException(double target)
    {
        var act = () => _sut.CalculateExposureIndex(1500.0, target);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetPixelValue");
    }

    [Fact]
    [Trait("SWR", "SWR-DM-047")]
    public void CalculateExposureIndex_MeanExactlyZero_ReturnsZero()
    {
        // Edge case: mean = 0 is valid (>= 0 check passes)
        var result = _sut.CalculateExposureIndex(0.0, 1500.0);

        result.Should().Be(0.0);
    }

    // =========================================================================
    // 6. RecordDoseAsync null validation
    // =========================================================================

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task RecordDose_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordDoseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // =========================================================================
    // 7. GetDoseByStudyAsync null validation
    // =========================================================================

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task GetDoseByStudy_NullStudyInstanceUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task GetDoseByStudy_RepositoryFailure_PropagatesError()
    {
        _repository.GetByStudyAsync("fail-study", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "Timeout"));

        var result = await _sut.GetDoseByStudyAsync("fail-study");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // =========================================================================
    // 8. GetDoseHistoryAsync null patientId validation
    // =========================================================================

    [Fact]
    [Trait("SWR", "SWR-DM-051")]
    public async Task GetDoseHistory_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("SWR", "SWR-DM-051")]
    public async Task GetDoseHistory_RepositoryFailure_PropagatesError()
    {
        _repository.GetByPatientAsync("fail-patient", null, null, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<DoseRecord>>(ErrorCode.DatabaseError, "Unavailable"));

        var result = await _sut.GetDoseHistoryAsync("fail-patient");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // =========================================================================
    // 9. ValidateExposureAsync — MESSAGE CONTENT VERIFICATION
    // =========================================================================

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task ValidateExposure_AllowLevel_HasNullMessage()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 5, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.Message.Should().BeNull();
    }

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task ValidateExposure_WarnLevel_MessageContainsDapValueAndDrl()
    {
        // CHEST DRL=10, need DAP between 10 and 20
        var parameters = new ExposureParameters("CHEST", Kvp: 100, Mas: 750, "1.2.3.4");
        // DAP = (10000 * 750) / 500000 = 15.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.Message.Should().Contain("15.0");
        result.Value.Message.Should().Contain("DRL");
        result.Value.Message.Should().Contain("10");
    }

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task ValidateExposure_BlockLevel_MessageContainsBlockThreshold()
    {
        // CHEST DRL=10, Block=50.0. Need DAP between 20 and 50
        var parameters = new ExposureParameters("CHEST", Kvp: 100, Mas: 1500, "1.2.3.4");
        // DAP = (10000 * 1500) / 500000 = 30.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.Message.Should().Contain("block");
        result.Value.Message.Should().Contain("50"); // 10*5=50
    }

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public async Task ValidateExposure_EmergencyLevel_MessageContainsSafetyInterlock()
    {
        // CHEST DRL=10, need DAP > 50
        var parameters = new ExposureParameters("CHEST", Kvp: 100, Mas: 5000, "1.2.3.4");
        // DAP = (10000 * 5000) / 500000 = 100.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.Message.Should().Contain("EMERGENCY");
        result.Value.Message.Should().Contain("interlock");
    }

    // =========================================================================
    // 10. ValidateExposureAsync — EI TARGET RESOLUTION
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData("CHEST", 1500.0)]
    [InlineData("ABDOMEN", 1200.0)]
    [InlineData("SPINE", 1800.0)]
    [InlineData("SKULL", 1400.0)]
    public async Task ValidateExposure_KnownBodyParts_EiTargetUsedCorrectly(
        string bodyPart, double expectedTarget)
    {
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 10, "1.2.3.4");
        var simulatedMean = (100.0 * 10.0) / 10.0; // = 100.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        var expectedEi = (simulatedMean / expectedTarget) * 1000.0;
        result.Value.ExposureIndex.Should().BeApproximately(expectedEi, 0.01);
    }

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData("HAND")]
    [InlineData("FOOT")]
    [InlineData("PELVIS")]
    [InlineData("KNEE")]
    [InlineData("SHOULDER")]
    public async Task ValidateExposure_BodyPartNotInEiTargets_UsesDefault1500(string bodyPart)
    {
        // Body parts in DRL dict but not in EI target dict should use default 1500
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 10, "1.2.3.4");
        var simulatedMean = (100.0 * 10.0) / 10.0; // = 100.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        var expectedEi = (simulatedMean / 1500.0) * 1000.0;
        result.Value.ExposureIndex.Should().BeApproximately(expectedEi, 0.01);
    }

    [Theory]
    [Trait("SWR", "SWR-DM-047")]
    [InlineData("NECK")]
    [InlineData("THUMB")]
    [InlineData("UNKNOWN")]
    public async Task ValidateExposure_UnknownBodyPartNotInAnyDict_UsesBothDefaults(string bodyPart)
    {
        // Unknown body part: DRL=20 (default), EI target=1500 (default)
        var parameters = new ExposureParameters(bodyPart, Kvp: 100, Mas: 10, "1.2.3.4");
        var simulatedMean = (100.0 * 10.0) / 10.0; // = 100.0

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        // DAP = (10000*10)/500000 = 0.2, well under default DRL=20
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        var expectedEi = (simulatedMean / 1500.0) * 1000.0;
        result.Value.ExposureIndex.Should().BeApproximately(expectedEi, 0.01);
    }

    // =========================================================================
    // 11. DAP ESTIMATION ACCURACY (EstimateDap is private, tested via public API)
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DS-001")]
    [InlineData(50,  10,   0.05)]       // (2500*10)/500000 = 0.05
    [InlineData(80,  10,   0.128)]      // (6400*10)/500000 = 0.128
    [InlineData(100, 10,   0.2)]        // (10000*10)/500000 = 0.2
    [InlineData(120, 100,  2.88)]       // (14400*100)/500000 = 2.88
    [InlineData(150, 200,  9.0)]        // (22500*200)/500000 = 9.0
    [InlineData(60,  1000, 7.2)]        // (3600*1000)/500000 = 7.2
    public async Task ValidateExposure_DapEstimation_CalculatesCorrectly(
        double kvp, double mas, double expectedDap)
    {
        var parameters = new ExposureParameters("CHEST", kvp, mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstimatedDap.Should().BeApproximately(expectedDap, 0.001);
    }

    // =========================================================================
    // 12. ESD IN ValidateExposure RESULT
    // =========================================================================

    [Theory]
    [Trait("SWR", "SWR-DM-042")]
    [InlineData(100.0)]
    [InlineData(400.0)]
    [InlineData(200.0)]
    [InlineData(1000.0)]
    public async Task ValidateExposure_EsdVariesWithFieldArea(double fieldArea)
    {
        // Same kVp/mAs, different field areas
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 10, "1.2.3.4",
            FieldAreaCm2: fieldArea);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        // DAP = (6400*10)/500000 = 0.128
        // ESD = (0.128 / fieldArea) * 1.35
        var expectedEsd = (0.128 / fieldArea) * 1.35;
        result.Value.EstimatedEsd.Should().BeApproximately(expectedEsd, 0.0001);
    }

    // =========================================================================
    // 13. CONSTRUCTOR VALIDATION
    // =========================================================================

    [Fact]
    [Trait("SWR", "SWR-DS-001")]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new DoseService(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("doseRepository");
    }

    // =========================================================================
    // Helper: Create test DoseRecord with sensible defaults
    // =========================================================================

    /// <summary>
    /// Creates a test <see cref="DoseRecord"/> with configurable fields for branch coverage testing.
    /// </summary>
    private static DoseRecord CreateTestDoseRecord(
        string studyUid,
        double dap = 10.0,
        double dapMgyCm2 = 0.0,
        double fieldAreaCm2 = 0.0,
        double meanPixelValue = 0.0,
        double eiTarget = 0.0,
        double existingEi = 0.0,
        string bodyPart = "CHEST")
    {
        return new DoseRecord(
            DoseId: $"D-{studyUid}",
            StudyInstanceUid: studyUid,
            Dap: dap,
            Ei: existingEi,
            EffectiveDose: 0.05,
            BodyPart: bodyPart,
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: dapMgyCm2,
            FieldAreaCm2: fieldAreaCm2,
            MeanPixelValue: meanPixelValue,
            EiTarget: eiTarget);
    }
}
