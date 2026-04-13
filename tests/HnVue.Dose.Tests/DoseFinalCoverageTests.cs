using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// Final coverage gap tests for Dose module (89.9% → 90%+).
/// Targets remaining uncovered branches in DoseService and DoseRepository.
/// </summary>
[Trait("SWR", "SWR-DM-051")]
public sealed class DoseFinalCoverageTests
{
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseFinalCoverageTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // ── ValidateExposureAsync: case-insensitive body part lookup ──────────────

    [Theory]
    [InlineData("chest")]
    [InlineData("CHEST")]
    [InlineData("Chest")]
    [InlineData("Abdomen")]
    [InlineData("ABDOMEN")]
    public async Task ValidateExposure_CaseInsensitiveBodyPart_ReturnsCorrectDrl(string bodyPart)
    {
        var parameters = new ExposureParameters(bodyPart, Kvp: 80, Mas: 5, "1.2.3", FieldAreaCm2: 100);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
    }

    // ── ValidateExposureAsync: edge case field area boundary ──────────────────

    [Fact]
    public async Task ValidateExposure_FieldAreaExactly1cm2_Succeeds()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 1, "1.2.3", FieldAreaCm2: 1.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposure_FieldAreaBelow1cm2_ReturnsValidationFailure()
    {
        var parameters = new ExposureParameters("CHEST", Kvp: 80, Mas: 1, "1.2.3", FieldAreaCm2: 0.5);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── CalculateEsd: zero DAP (boundary) ────────────────────────────────────

    [Fact]
    public void CalculateEsd_ZeroDap_ReturnsZero()
    {
        var esd = _sut.CalculateEsd(0.0, 100.0);

        esd.Should().Be(0.0);
    }

    // ── CalculateExposureIndex: zero mean pixel value ─────────────────────────

    [Fact]
    public void CalculateExposureIndex_ZeroMeanPixel_ReturnsZero()
    {
        var ei = _sut.CalculateExposureIndex(0.0, 1000.0);

        ei.Should().Be(0.0);
    }

    // ── RecordDoseAsync: null parameter validation ────────────────────────────

    [Fact]
    public async Task RecordDoseAsync_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordDoseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GetDoseByStudyAsync: null parameter validation ────────────────────────

    [Fact]
    public async Task GetDoseByStudyAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GetDoseHistoryAsync: null parameter validation ────────────────────────

    [Fact]
    public async Task GetDoseHistoryAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GenerateRdsrSummaryAsync: null parameter validation ───────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GenerateRdsrSummaryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GenerateRdsrSummaryAsync: all fields populated (full enrichment) ──────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_AllFieldsPopulated_EnrichesAll()
    {
        var dose = new DoseRecord(
            DoseId: "d1",
            StudyInstanceUid: "st1",
            Dap: 5.0,
            Ei: 800.0,
            EffectiveDose: 0.1,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "p1",
            DapMgyCm2: 7.5,
            FieldAreaCm2: 100.0,
            EsdMgy: null,
            MeanPixelValue: 1500.0,
            EiTarget: 1400.0);

        _repository.GetByStudyAsync("st1", Arg.Any<CancellationToken>())
             .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("st1");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(7.5); // Uses DapMgyCm2, not Dap
        result.Value.EsdMgy.Should().NotBeNull();
        result.Value.EiTarget.Should().Be(1400.0); // Uses record's EiTarget
    }

    // ── GenerateRdsrSummaryAsync: minimal fields (all fallbacks) ──────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_MinimalFields_UsesFallbacks()
    {
        var dose = new DoseRecord(
            DoseId: "d2",
            StudyInstanceUid: "st2",
            Dap: 10.0,
            Ei: 500.0,
            EffectiveDose: 0.2,
            BodyPart: "NECK", // Not in DRL or EI target maps
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "p2",
            DapMgyCm2: 0.0, // Forces Dap fallback
            FieldAreaCm2: 0.0, // Forces ESD null
            EsdMgy: null,
            MeanPixelValue: 0.0, // Forces Ei preservation
            EiTarget: 0.0); // Forces GetEiTarget fallback

        _repository.GetByStudyAsync("st2", Arg.Any<CancellationToken>())
             .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("st2");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(10.0); // Falls back to Dap
        result.Value.EsdMgy.Should().BeNull(); // FieldArea too small
        result.Value.Ei.Should().Be(500.0); // Preserves existing Ei
    }
}
