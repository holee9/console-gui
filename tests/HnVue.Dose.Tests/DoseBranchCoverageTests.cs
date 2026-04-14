// ─────────────────────────────────────────────────────────────────────────────
// DoseBranchCoverageTests.cs — Targeted branch coverage boost for Dose module
// Focus: Safety-critical branch paths not covered by existing tests
// Target: branch coverage 82.3% → 90%+
// ─────────────────────────────────────────────────────────────────────────────

using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HnVue.Dose.Tests;

[Trait("SWR", "SWR-DS-020")]
public sealed class DoseBranchCoverageTests : IDisposable
{
    private readonly HnVueDbContext _context;
    private readonly DoseService _sut;
    private readonly IDoseRepository _repo;

    public DoseBranchCoverageTests()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _repo = Substitute.For<IDoseRepository>();
        _sut = new DoseService(_repo);
    }

    public void Dispose() => _context.Dispose();

    // ── CalculateEsd — negative DAP branch ──────────────────────────────────────

    [Fact]
    public void CalculateEsd_NegativeDap_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(-1.0, 100.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("dap")
            .And.Message.Should().Contain("DAP cannot be negative");
    }

    [Fact]
    public void CalculateEsd_DapZero_IsAllowed()
    {
        // DAP of 0 is valid (no exposure) — should not throw
        var result = _sut.CalculateEsd(0.0, 100.0);

        result.Should().BeApproximately(0.0, 0.0001);
    }

    // ── CalculateEsd — backscatter factor boundary ──────────────────────────────

    [Fact]
    public void CalculateEsd_BackscatterFactorExactlyOne_IsAllowed()
    {
        var result = _sut.CalculateEsd(10.0, 100.0, backscatterFactor: 1.0);

        result.Should().BeApproximately(0.1, 0.0001);
    }

    [Fact]
    public void CalculateEsd_BackscatterFactorLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateEsd(10.0, 100.0, backscatterFactor: 0.99);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("backscatterFactor")
            .And.Message.Should().Contain("Backscatter factor must be >= 1.0");
    }

    // ── CalculateExposureIndex — boundary branches ──────────────────────────────

    [Fact]
    public void CalculateExposureIndex_TargetPixelValueZero_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateExposureIndex(100.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetPixelValue")
            .And.Message.Should().Contain("Target pixel value must be positive");
    }

    [Fact]
    public void CalculateExposureIndex_TargetPixelValueNegative_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _sut.CalculateExposureIndex(100.0, -5.0);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("targetPixelValue");
    }

    [Fact]
    public void CalculateExposureIndex_MeanPixelValueZero_IsAllowed()
    {
        var result = _sut.CalculateExposureIndex(0.0, 1000.0);

        result.Should().BeApproximately(0.0, 0.0001);
    }

    // ── GenerateRdsrSummaryAsync — uncovered branch combinations ────────────────

    [Fact]
    public async Task GenerateRdsrSummary_MeanPixelValueZero_UsesExistingEi()
    {
        // When MeanPixelValue is 0, should use existing dose.Ei instead of computing
        var dose = new DoseRecord(
            DoseId: "dose-ei-zero",
            StudyInstanceUid: "1.2.3.ei-zero",
            Dap: 5.0,
            Ei: 850.0,   // Existing EI to preserve
            EffectiveDose: 0.1,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            FieldAreaCm2: 100.0,
            DapMgyCm2: 5.0,
            MeanPixelValue: 0.0,   // Zero → should use existing EI
            EiTarget: 0.0,         // Zero → should use body part default
            EsdMgy: null);

        _repo.GetByStudyAsync("1.2.3.ei-zero", default)
            .Returns(Task.FromResult(Result.Success<DoseRecord?>(dose)));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.ei-zero");

        result.IsSuccess.Should().BeTrue();
        result.Value.Ei.Should().Be(850.0); // Preserved existing EI
    }

    [Fact]
    public async Task GenerateRdsrSummary_FieldAreaBelowMinimum_EsdIsNull()
    {
        var dose = new DoseRecord(
            DoseId: "dose-tiny-field",
            StudyInstanceUid: "1.2.3.tiny-field",
            Dap: 5.0,
            Ei: 1000.0,
            EffectiveDose: 0.1,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            FieldAreaCm2: 0.5,     // Below MinimumFieldAreaCm2 (1.0)
            DapMgyCm2: 5.0,
            MeanPixelValue: 100.0,
            EiTarget: 1500.0,
            EsdMgy: null);

        _repo.GetByStudyAsync("1.2.3.tiny-field", default)
            .Returns(Task.FromResult(Result.Success<DoseRecord?>(dose)));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.tiny-field");

        result.IsSuccess.Should().BeTrue();
        result.Value.EsdMgy.Should().BeNull(); // ESD skipped for tiny field area
    }

    [Fact]
    public async Task GenerateRdsrSummary_DapMgyCm2Positive_UsesNewField()
    {
        var dose = new DoseRecord(
            DoseId: "dose-new-dap",
            StudyInstanceUid: "1.2.3.new-dap",
            Dap: 3.0,           // Legacy DAP
            Ei: 1000.0,
            EffectiveDose: 0.1,
            BodyPart: "CHEST",
            RecordedAt: DateTimeOffset.UtcNow,
            FieldAreaCm2: 100.0,
            DapMgyCm2: 7.5,     // New DAP field > 0 → should be used
            MeanPixelValue: 100.0,
            EiTarget: 1500.0,
            EsdMgy: null);

        _repo.GetByStudyAsync("1.2.3.new-dap", default)
            .Returns(Task.FromResult(Result.Success<DoseRecord?>(dose)));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.new-dap");

        result.IsSuccess.Should().BeTrue();
        result.Value.DapMgyCm2.Should().Be(7.5); // Used new field
        // ESD should be computed with 7.5 (new DAP) not 3.0 (legacy)
        result.Value.EsdMgy.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateRdsrSummary_EiTargetPositive_UsesProvidedTarget()
    {
        var dose = new DoseRecord(
            DoseId: "dose-ei-target",
            StudyInstanceUid: "1.2.3.ei-target",
            Dap: 5.0,
            Ei: 1000.0,
            EffectiveDose: 0.1,
            BodyPart: "ABDOMEN",
            RecordedAt: DateTimeOffset.UtcNow,
            FieldAreaCm2: 100.0,
            DapMgyCm2: 5.0,
            MeanPixelValue: 1200.0,
            EiTarget: 2000.0,    // Custom target > 0 → should be preserved
            EsdMgy: null);

        _repo.GetByStudyAsync("1.2.3.ei-target", default)
            .Returns(Task.FromResult(Result.Success<DoseRecord?>(dose)));

        var result = await _sut.GenerateRdsrSummaryAsync("1.2.3.ei-target");

        result.IsSuccess.Should().BeTrue();
        result.Value.EiTarget.Should().Be(2000.0); // Preserved custom target
    }

    // ── ValidateExposureAsync — additional edge cases ───────────────────────────

    [Fact]
    public async Task ValidateExposure_FieldAreaExactlyMinimum_Succeeds()
    {
        var parameters = new ExposureParameters(
            BodyPart: "CHEST", Kvp: 80, Mas: 2.0, StudyInstanceUid: "1.2.3.field-min",
            FieldAreaCm2: 1.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposure_FieldAreaJustBelowMinimum_Fails()
    {
        var parameters = new ExposureParameters(
            BodyPart: "CHEST", Kvp: 80, Mas: 2.0, StudyInstanceUid: "1.2.3.field-below",
            FieldAreaCm2: 0.99);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Field area must be at least");
    }

    [Fact]
    public async Task ValidateExposure_BodyPartNotInDrl_UsesDefaultDrl()
    {
        var parameters = new ExposureParameters(
            BodyPart: "ELBOW", Kvp: 80, Mas: 2.0, StudyInstanceUid: "1.2.3.elbow",
            FieldAreaCm2: 100.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        // ELBOW is not in DRL dictionary → uses DefaultDrl (20.0)
        // 80*80*2 / 500000 = 0.0256 mGy·cm² << 20.0 → Allow
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    [Fact]
    public async Task ValidateExposure_HighKvpEmergencyLevel_ReturnsEmergency()
    {
        // kVp=150, mAs=5000 → DAP = 150^2 * 5000 / 500000 = 225 mGy·cm²
        // CHEST DRL = 10.0 → 225 > 10*5 = 50 → Emergency
        var parameters = new ExposureParameters(
            BodyPart: "CHEST", Kvp: 150, Mas: 5000, StudyInstanceUid: "1.2.3.emergency",
            FieldAreaCm2: 100.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.Message.Should().Contain("EMERGENCY");
    }

    [Fact]
    public async Task ValidateExposure_ExactlyAtBlockThreshold_ReturnsBlock()
    {
        // Target: DAP = exactly BlockMultiplier * DRL = 5 * 10 = 50.0 for CHEST
        // DAP = kVp^2 * mAs / 500000 = 50.0 → kVp=100, mAs=2500 → 100*100*2500/500000 = 50.0
        var parameters = new ExposureParameters(
            BodyPart: "CHEST", Kvp: 100, Mas: 2500, StudyInstanceUid: "1.2.3.block",
            FieldAreaCm2: 100.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Block);
    }

    [Fact]
    public async Task ValidateExposure_ExactlyAtWarnThreshold_ReturnsWarn()
    {
        // Target: DAP = exactly WarnMultiplier * DRL = 2 * 10 = 20.0 for CHEST
        // DAP = kVp^2 * mAs / 500000 = 20.0 → kVp=100, mAs=1000 → 100*100*1000/500000 = 20.0
        var parameters = new ExposureParameters(
            BodyPart: "CHEST", Kvp: 100, Mas: 1000, StudyInstanceUid: "1.2.3.warn",
            FieldAreaCm2: 100.0);

        var result = await _sut.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
    }

    // ── RecordDoseAsync — branch coverage ───────────────────────────────────────

    [Fact]
    public async Task RecordDose_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordDoseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("dose");
    }

    [Fact]
    public async Task RecordDose_ValidRecord_CallsRepository()
    {
        var dose = new DoseRecord(
            DoseId: "rec-001", StudyInstanceUid: "1.2.3.rec",
            Dap: 5.0, Ei: 1000.0, EffectiveDose: 0.1,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow);

        _repo.SaveAsync(dose, default)
            .Returns(Task.FromResult(Result.Success()));

        var result = await _sut.RecordDoseAsync(dose);

        result.IsSuccess.Should().BeTrue();
        await _repo.Received(1).SaveAsync(dose, default);
    }

    // ── GetDoseByStudyAsync — branch coverage ───────────────────────────────────

    [Fact]
    public async Task GetDoseByStudy_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    [Fact]
    public async Task GetDoseByStudy_ValidStudy_CallsRepository()
    {
        var dose = new DoseRecord(
            DoseId: "get-001", StudyInstanceUid: "1.2.3.get",
            Dap: 5.0, Ei: 1000.0, EffectiveDose: 0.1,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow);

        _repo.GetByStudyAsync("1.2.3.get", default)
            .Returns(Task.FromResult(Result.Success<DoseRecord?>(dose)));

        var result = await _sut.GetDoseByStudyAsync("1.2.3.get");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    // ── GetDoseHistoryAsync — branch coverage ───────────────────────────────────

    [Fact]
    public async Task GetDoseHistory_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetDoseHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    [Fact]
    public async Task GetDoseHistory_WithDateRange_CallsRepository()
    {
        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var until = DateTimeOffset.UtcNow;

        _repo.GetByPatientAsync("pat-001", from, until, default)
            .Returns(Task.FromResult(Result.Success<IReadOnlyList<DoseRecord>>([])));

        var result = await _sut.GetDoseHistoryAsync("pat-001", from, until);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── EfDoseRepository — DbUpdateException branch ────────────────────────────

    [Fact]
    public async Task EfSaveAsync_DuplicatePrimaryKey_TrackingConflictHandled()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();

        var dose1 = new DoseRecord(
            DoseId: "dup-id", StudyInstanceUid: "1.2.3.dup",
            Dap: 1.0, Ei: 1.0, EffectiveDose: 0.1,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow);

        var repo = new EfDoseRepository(context);
        var result1 = await repo.SaveAsync(dose1);
        result1.IsSuccess.Should().BeTrue();

        // Same primary key added again — InMemory provider throws InvalidOperationException
        // (tracking conflict) which the repository should wrap as DatabaseError
        var dose2 = new DoseRecord(
            DoseId: "dup-id", StudyInstanceUid: "1.2.3.dup",
            Dap: 2.0, Ei: 2.0, EffectiveDose: 0.2,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow);

        var result2 = await repo.SaveAsync(dose2);
        result2.IsFailure.Should().BeTrue();
        result2.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task EfGetByStudyAsync_ExceptionHandling_ReturnsDatabaseError()
    {
        // Test the generic exception handler by using a disposed context
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        context.Dispose(); // Dispose before use to trigger exception

        var repo = new EfDoseRepository(context);

        // This should hit the catch block (ObjectDisposedException → wrapped as database error)
        var result = await repo.GetByStudyAsync("any-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task EfGetByPatientAsync_ExceptionHandling_ReturnsDatabaseError()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        context.Dispose();

        var repo = new EfDoseRepository(context);

        var result = await repo.GetByPatientAsync("any-patient", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── DoseRepository (legacy) — branch coverage ──────────────────────────────

    [Fact]
    public async Task LegacyRepository_SaveAsync_NullDose_Throws()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        var repo = new DoseRepository(context);

        var act = async () => await repo.SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("dose");
    }

    [Fact]
    public async Task LegacyRepository_GetByStudyAsync_NullUid_Throws()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        var repo = new DoseRepository(context);

        var act = async () => await repo.GetByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    [Fact]
    public async Task LegacyRepository_GetByPatientAsync_NullPatientId_Throws()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        var repo = new DoseRepository(context);

        var act = async () => await repo.GetByPatientAsync(null!, null, null);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    [Fact]
    public async Task LegacyRepository_GetByStudyAsync_DisposedContext_ReturnsDatabaseError()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        context.Dispose();

        var repo = new DoseRepository(context);

        var result = await repo.GetByStudyAsync("any-study");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task LegacyRepository_GetByPatientAsync_DisposedContext_ReturnsDatabaseError()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        context.Dispose();

        var repo = new DoseRepository(context);

        var result = await repo.GetByPatientAsync("any-patient", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task LegacyRepository_SaveAsync_DisposedContext_ReturnsDatabaseError()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new HnVueDbContext(opts);
        context.Database.EnsureCreated();
        context.Dispose();

        var repo = new DoseRepository(context);

        var dose = new DoseRecord(
            DoseId: "fail-id", StudyInstanceUid: "1.2.3.fail",
            Dap: 1.0, Ei: 1.0, EffectiveDose: 0.1,
            BodyPart: "CHEST", RecordedAt: DateTimeOffset.UtcNow);

        var result = await repo.SaveAsync(dose);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }
}
