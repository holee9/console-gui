using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// Additional edge-case tests for <see cref="DoseService"/> targeting 90%+ branch coverage.
/// Covers all body parts in DRL dictionary, boundary conditions on dose validation levels,
/// and RDSR summary edge cases.
/// </summary>
[Trait("SWR", "SWR-DOSE-010")]
public sealed class DoseServiceEdgeCaseTests
{
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseServiceEdgeCaseTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // ── ValidateExposureAsync — all body parts with known DRL ────────────────

    [Theory]
    [InlineData("CHEST", 10.0)]
    [InlineData("ABDOMEN", 25.0)]
    [InlineData("PELVIS", 25.0)]
    [InlineData("HAND", 3.0)]
    [InlineData("FOOT", 3.0)]
    [InlineData("SPINE", 40.0)]
    [InlineData("SKULL", 30.0)]
    [InlineData("KNEE", 5.0)]
    [InlineData("SHOULDER", 15.0)]
    public async Task ValidateExposure_AllKnownBodyParts_AllowLevelForLowDose(
        string bodyPart, double drl)
    {
        // Very low dose — should always be Allow
        var parameters = new ExposureParameters(bodyPart, Kvp: 40, Mas: 1, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.EstimatedDap.Should().BeLessThan(drl);
    }

    // ── ValidateExposureAsync — case insensitivity for body parts ────────────

    [Theory]
    [InlineData("chest")]
    [InlineData("Chest")]
    [InlineData("CHEST")]
    public async Task ValidateExposure_BodyPartCaseInsensitive_UsesCorrectDrl(string bodyPart)
    {
        var parameters = new ExposureParameters(bodyPart, Kvp: 40, Mas: 1, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    // ── ValidateExposureAsync — negative kVp ─────────────────────────────────

    [Fact]
    public async Task ValidateExposure_NegativeKvp_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: -80, Mas: 10, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("kVp");
    }

    // ── ValidateExposureAsync — zero mAs ────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_ZeroMas_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 0, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("mAs");
    }

    // ── ValidateExposureAsync — boundary: exactly at DRL ────────────────────

    [Fact]
    public async Task ValidateExposure_ExactlyAtDrl_ReturnsAllow()
    {
        // HAND DRL = 3.0 mGy·cm²
        // DAP = (kVp² × mAs) / 500000 = 3.0 → kVp=100, mAs=150 → 10000×150/500000 = 3.0
        var parameters = new ExposureParameters("HAND", Kvp: 100, Mas: 150, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    // ── ValidateExposureAsync — boundary: exactly at WarnMultiplier×DRL ─────

    [Fact]
    public async Task ValidateExposure_ExactlyAtWarnThreshold_ReturnsWarn()
    {
        // HAND DRL = 3.0, Warn = 2.0×3.0 = 6.0
        // DAP = 6.0 → kVp=100, mAs=300 → 10000×300/500000 = 6.0
        var parameters = new ExposureParameters("HAND", Kvp: 100, Mas: 300, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
    }

    // ── ValidateExposureAsync — boundary: exactly at BlockMultiplier×DRL ────

    [Fact]
    public async Task ValidateExposure_ExactlyAtBlockThreshold_ReturnsBlock()
    {
        // HAND DRL = 3.0, Block = 5.0×3.0 = 15.0
        // DAP = 15.0 → kVp=100, mAs=750 → 10000×750/500000 = 15.0
        var parameters = new ExposureParameters("HAND", Kvp: 100, Mas: 750, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    // ── ValidateExposureAsync — just above Block threshold ────────────────────

    [Fact]
    public async Task ValidateExposure_JustAboveBlockThreshold_ReturnsEmergency()
    {
        // HAND DRL = 3.0, Block = 15.0
        // DAP = 15.1 → kVp=100, mAs=755 → 10000×755/500000 = 15.1
        var parameters = new ExposureParameters("HAND", Kvp: 100, Mas: 755, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
    }

    // ── CalculateEsd — exact boundary: fieldArea = 1.0 (minimum) ────────────

    [Fact]
    public void CalculateEsd_ExactlyAtMinimumFieldArea_ReturnsValidResult()
    {
        var result = _sut.CalculateEsd(10.0, 1.0);

        result.Should().BeApproximately(13.5, 0.0001); // (10/1) × 1.35
    }

    // ── CalculateEsd — zero DAP ─────────────────────────────────────────────

    [Fact]
    public void CalculateEsd_ZeroDap_ReturnsZero()
    {
        var result = _sut.CalculateEsd(0.0, 100.0);

        result.Should().Be(0.0);
    }

    // ── CalculateEsd — backscatter factor exactly 1.0 ───────────────────────

    [Fact]
    public void CalculateEsd_BackscatterFactorExactly1_ReturnsCorrectResult()
    {
        var result = _sut.CalculateEsd(10.0, 100.0, 1.0);

        result.Should().BeApproximately(0.1, 0.0001); // (10/100) × 1.0
    }

    // ── CalculateExposureIndex — very large values ──────────────────────────

    [Fact]
    public void CalculateExposureIndex_VeryLargeMean_ReturnsProportionalResult()
    {
        var result = _sut.CalculateExposureIndex(100000.0, 1500.0);

        result.Should().BeApproximately(66666.667, 0.01);
    }

    // ── CalculateExposureIndex — very small mean ────────────────────────────

    [Fact]
    public void CalculateExposureIndex_VerySmallMean_ReturnsSmallResult()
    {
        var result = _sut.CalculateExposureIndex(1.0, 1500.0);

        result.Should().BeApproximately(0.6667, 0.01);
    }

    // ── GenerateRdsrSummaryAsync — record with mean pixel value but no field area

    [Fact]
    public async Task GenerateRdsrSummary_WithMeanPixelValueButNoFieldArea_ComputesEiButNotEsd()
    {
        var dose = new DoseRecord(
            DoseId: "D200",
            StudyInstanceUid: "1.2.3.200",
            Dap: 15.0,
            Ei: 0.0,
            EffectiveDose: 0.08,
            BodyPart: "ABDOMEN",
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 15.0,
            FieldAreaCm2: 0.0,       // no field area → no ESD
            MeanPixelValue: 1200.0,  // has pixel data → EI can be computed
            EiTarget: 1200.0);

        _repository.GetByStudyAsync("1.2.3.200", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.200");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
        result.Value.Ei.Should().Be(1000.0); // (1200/1200)×1000
    }

    // ── GenerateRdsrSummaryAsync — unknown body part uses default EI target

    [Fact]
    public async Task GenerateRdsrSummary_UnknownBodyPart_UsesDefaultEiTarget()
    {
        var dose = new DoseRecord(
            DoseId: "D201",
            StudyInstanceUid: "1.2.3.201",
            Dap: 10.0,
            Ei: 0.0,
            EffectiveDose: 0.05,
            BodyPart: "ELBOW",       // not in EI target dictionary
            RecordedAt: DateTimeOffset.UtcNow,
            DapMgyCm2: 10.0,
            FieldAreaCm2: 100.0,
            MeanPixelValue: 1500.0,
            EiTarget: 0.0);          // will use default (1500)

        _repository.GetByStudyAsync("1.2.3.201", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.201");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1500.0);
        result.Value.Ei.Should().Be(1000.0);
    }

    // ── ValidateExposureAsync — DAP estimation is correct ────────────────────

    [Theory]
    [InlineData(80, 10, 0.128)]     // (6400 × 10) / 500000 = 0.128
    [InlineData(100, 20, 0.4)]      // (10000 × 20) / 500000 = 0.4
    [InlineData(120, 100, 2.88)]    // (14400 × 100) / 500000 = 2.88
    public async Task ValidateExposure_DapEstimationIsCorrect(
        double kvp, double mas, double expectedDap)
    {
        var parameters = new ExposureParameters("CHEST", kvp, mas, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.EstimatedDap.Should().BeApproximately(expectedDap, 0.001);
    }

    // ── ValidateExposureAsync — ESD is included in result ────────────────────

    [Fact]
    public async Task ValidateExposure_DefaultFieldArea_EsdCalculatedCorrectly()
    {
        // Default FieldAreaCm2 in ExposureParameters should produce valid ESD
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 10, "1.2.3.4", FieldAreaCm2: 200.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        // DAP = (6400×10)/500000 = 0.128
        // ESD = (0.128/200) × 1.35 = 0.000864
        result.Value.EstimatedEsd.Should().BeApproximately(0.000864, 0.0001);
    }

    // ── Warn message format ──────────────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_WarnLevel_MessageContainsDrl()
    {
        // CHEST DRL = 10, need DAP between 10 and 20
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 520, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.Message.Should().Contain("DRL");
        result.Value.Message.Should().Contain("10");
    }

    // ── Block message format ─────────────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_BlockLevel_MessageContainsBlockThreshold()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 1050, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.Message.Should().Contain("block");
    }

    // ── Emergency message format ─────────────────────────────────────────────

    [Fact]
    public async Task ValidateExposure_EmergencyLevel_MessageContainsEmergency()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 120, Mas: 3000, "1.2.3.4");

        var result = await _sut.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.Message.Should().Contain("EMERGENCY");
        result.Value.Message.Should().Contain("interlock");
    }
}
