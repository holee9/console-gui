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
}
