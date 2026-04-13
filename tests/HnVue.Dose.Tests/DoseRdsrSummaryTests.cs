using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dose;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

[Trait("SWR", "SWR-DM-044")]
public sealed class DoseRdsrSummaryTests
{
    private readonly IDoseRepository _repository;
    private readonly DoseService _sut;

    public DoseRdsrSummaryTests()
    {
        _repository = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repository);
    }

    // ── GenerateRdsrSummaryAsync — null argument ───────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = () => _sut.GenerateRdsrSummaryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    // ── GenerateRdsrSummaryAsync — repository failure ──────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_RepositoryFails_ReturnsFailure()
    {
        _repository.GetByStudyAsync("study-1", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, "DB connection lost"));

        var result = await _sut.GenerateRdsrSummaryAsync("study-1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── GenerateRdsrSummaryAsync — not found ───────────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_DoseNotFound_ReturnsNotFound()
    {
        // Simulate repository returning a "not found" via a DoseRecord with empty study UID
        // Result.Success cannot accept null value, so we use a DoseRecord with a marker
        var emptyDose = new DoseRecord(
            DoseId: string.Empty, StudyInstanceUid: string.Empty,
            Dap: 0, Ei: 0, EffectiveDose: 0, BodyPart: string.Empty,
            RecordedAt: DateTimeOffset.UtcNow);
        _repository.GetByStudyAsync("study-2", Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                // Return a result where value IS null - need to use reflection or
                // simulate the IsFailure path instead
                return Result.Failure<DoseRecord?>(
                    ErrorCode.NotFound, "No dose record found.");
            });

        var result = await _sut.GenerateRdsrSummaryAsync("study-2");

        result.IsFailure.Should().BeTrue();
    }

    // ── GenerateRdsrSummaryAsync — DapMgyCm2 path ─────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithDapMgyCm2_UsesRdsrDap()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 15.0, dap: 10.0, fieldAreaCm2: 100.0);
        _repository.GetByStudyAsync("study-3", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-3");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(15.0);
    }

    // ── GenerateRdsrSummaryAsync — legacy Dap fallback ─────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithoutDapMgyCm2_FallsBackToDap()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 0.0, dap: 8.0, fieldAreaCm2: 100.0);
        _repository.GetByStudyAsync("study-4", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-4");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(8.0);
    }

    // ── GenerateRdsrSummaryAsync — ESD calculation ─────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithValidFieldArea_CalculatesEsd()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 10.0, fieldAreaCm2: 100.0);
        _repository.GetByStudyAsync("study-5", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-5");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().NotBeNull();
        // ESD = DAP / FieldArea * BackscatterFactor = 10 / 100 * 1.35 = 0.135
        result.Value.EsdMgy.Should().BeApproximately(0.135, 0.001);
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithSmallFieldArea_CalculatesEsd()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, fieldAreaCm2: 10.0);
        _repository.GetByStudyAsync("study-5b", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-5b");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().NotBeNull();
        // ESD = 5 / 10 * 1.35 = 0.675
        result.Value.EsdMgy.Should().BeApproximately(0.675, 0.001);
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithZeroFieldArea_EsdIsNull()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 10.0, fieldAreaCm2: 0.0);
        _repository.GetByStudyAsync("study-6", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-6");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithFieldAreaBelowMinimum_EsdIsNull()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 10.0, fieldAreaCm2: 0.5);
        _repository.GetByStudyAsync("study-6b", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-6b");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull();
    }

    // ── GenerateRdsrSummaryAsync — EI target path ──────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithEiTarget_UsesDoseEiTarget()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, eiTarget: 1800.0, meanPixelValue: 900.0);
        _repository.GetByStudyAsync("study-7", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-7");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1800.0);
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithoutEiTarget_UsesBodyPartDefault()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, eiTarget: 0.0, bodyPart: "CHEST", meanPixelValue: 900.0);
        _repository.GetByStudyAsync("study-8", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-8");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1500.0); // CHEST default
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithUnknownBodyPart_UsesGenericDefault()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, eiTarget: 0.0, bodyPart: "EXTREMITY", meanPixelValue: 900.0);
        _repository.GetByStudyAsync("study-8b", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-8b");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(1500.0); // Default
    }

    // ── GenerateRdsrSummaryAsync — EI computation ──────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithMeanPixelValue_CalculatesEi()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, eiTarget: 1500.0, meanPixelValue: 1500.0);
        _repository.GetByStudyAsync("study-9", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-9");

        result.IsSuccess.Should().BeTrue();
        // EI = meanPixelValue / target * 1000 = 1500/1500*1000 = 1000
        result.Value.Ei.Should().BeApproximately(1000.0, 0.01);
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_WithZeroMeanPixelValue_PreservesExistingEi()
    {
        var dose = CreateDoseRecord(dapMgyCm2: 5.0, ei: 850.0, meanPixelValue: 0.0);
        _repository.GetByStudyAsync("study-10", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-10");

        result.IsSuccess.Should().BeTrue();
        result.Value.Ei.Should().Be(850.0);
    }

    // ── GenerateRdsrSummaryAsync — enriched record ─────────────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_EnrichesAllFields()
    {
        var dose = CreateDoseRecord(
            dapMgyCm2: 20.0,
            fieldAreaCm2: 200.0,
            eiTarget: 1200.0,
            meanPixelValue: 600.0);

        _repository.GetByStudyAsync("study-full", Arg.Any<CancellationToken>())
            .Returns(Result.Success<DoseRecord?>(dose));

        var result = await _sut.GenerateRdsrSummaryAsync("study-full");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(20.0);
        result.Value.EsdMgy.Should().NotBeNull();
        result.Value.EiTarget.Should().Be(1200.0);
        result.Value.Ei.Should().BeApproximately(500.0, 0.01); // 600/1200*1000
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    private static DoseRecord CreateDoseRecord(
        double dapMgyCm2 = 0.0,
        double dap = 5.0,
        double fieldAreaCm2 = 0.0,
        double eiTarget = 0.0,
        double meanPixelValue = 0.0,
        double ei = 0.0,
        string bodyPart = "CHEST")
    {
        return new DoseRecord(
            DoseId: "dose-001",
            StudyInstanceUid: "study-test",
            Dap: dap,
            Ei: ei,
            EffectiveDose: 0.1,
            BodyPart: bodyPart,
            RecordedAt: DateTimeOffset.UtcNow,
            PatientId: "P001",
            DapMgyCm2: dapMgyCm2,
            FieldAreaCm2: fieldAreaCm2,
            MeanPixelValue: meanPixelValue,
            EiTarget: eiTarget,
            EsdMgy: null);
    }
}
