using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// Coverage boost tests for EfDoseRepository exception paths and DoseService edge cases.
/// Targets: EfDoseRepository (75-88%), DoseService.ValidateExposureAsync (96%).
/// </summary>
[Trait("SWR", "SWR-DS-070")]
public sealed class DoseCoverageFinalTests : IDisposable
{
    private readonly HnVueDbContext _context;
    private readonly EfDoseRepository _efRepo;
    private readonly DoseService _doseService;
    private readonly DoseRepository _doseRepo;

    public DoseCoverageFinalTests()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _efRepo = new EfDoseRepository(_context);
        _doseService = new DoseService(_efRepo);
        _doseRepo = new DoseRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static DoseRecord CreateDoseRecord(
        string studyUid = "1.2.3.4.5",
        string doseId = "dose-001",
        double dap = 1.5,
        double ei = 0.8,
        double effectiveDose = 0.12,
        string bodyPart = "Chest",
        DateTimeOffset? recordedAt = null,
        string? patientId = null) =>
        new(doseId, studyUid, dap, ei, effectiveDose, bodyPart,
            recordedAt ?? DateTimeOffset.UtcNow, patientId);

    private async Task SeedPatientAndStudy(
        string patientId, string studyUid, string patientName = "Test")
    {
        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId, Name = patientName, CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid, PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks, StudyDateOffsetMinutes = 0
        });
        await _context.SaveChangesAsync();
    }

    // ── EfDoseRepository — exception paths via disposed context ──────────────
    // Note: EfDoseRepository.SaveAsync only catches DbUpdateException (not ObjectDisposedException).
    // GetByStudyAsync and GetByPatientAsync catch generic Exception, so those paths are testable.

    [Fact]
    public async Task EfRepo_GetByStudyAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new EfDoseRepository(localContext);
        localContext.Dispose();

        var result = await repo.GetByStudyAsync("any-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task EfRepo_GetByPatientAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new EfDoseRepository(localContext);
        localContext.Dispose();

        var result = await repo.GetByPatientAsync("any-patient", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task EfRepo_SaveAsync_ValidRecord_SucceedsAndCanBeQueried()
    {
        var dose = CreateDoseRecord(doseId: "save-test", studyUid: "save-study");
        var saveResult = await _efRepo.SaveAsync(dose);

        saveResult.IsSuccess.Should().BeTrue();

        var queryResult = await _efRepo.GetByStudyAsync("save-study");
        queryResult.IsSuccess.Should().BeTrue();
        queryResult.Value.Should().NotBeNull();
        queryResult.Value!.DoseId.Should().Be("save-test");
    }

    // ── DoseRepository — exception paths ──────────────────────────────────────

    [Fact]
    public async Task Repo_SaveAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new DoseRepository(localContext);
        localContext.Dispose();

        var dose = CreateDoseRecord();
        var result = await repo.SaveAsync(dose);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task Repo_GetByStudyAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new DoseRepository(localContext);
        localContext.Dispose();

        var result = await repo.GetByStudyAsync("any-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task Repo_GetByPatientAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new DoseRepository(localContext);
        localContext.Dispose();

        var result = await repo.GetByPatientAsync("any-patient", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public void DoseRepository_NullContext_ThrowsArgumentNullException()
    {
        var act = () => new DoseRepository(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dbContext");
    }

    [Fact]
    public async Task Repo_SaveAsync_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _doseRepo.SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("dose");
    }

    [Fact]
    public async Task Repo_GetByStudyAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _doseRepo.GetByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    [Fact]
    public async Task Repo_GetByPatientAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _doseRepo.GetByPatientAsync(null!, null, null);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    // ── DoseService — ValidateExposureAsync full branch coverage ─────────────

    [Fact]
    public async Task ValidateExposureAsync_AllowLevel_ReturnsSuccessWithNullMessage()
    {
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 80, Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
        result.Value.Message.Should().BeNull();
        result.Value.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposureAsync_WarnLevel_ReturnsWarningMessage()
    {
        // DRL for Chest is 10.0, warn at 2x = 20.0
        // Need DAP > 10 for Chest warn: kvp=200, mAs=200 => (40000*200)/500000 = 16
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 200, Mas: 200,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Warn);
        result.Value.Message.Should().Contain("DRL");
        result.Value.IsAllowed.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposureAsync_BlockLevel_ReturnsBlockMessage()
    {
        // DAP > 20 (2x Chest DRL) but <= 50 (5x Chest DRL)
        // kvp=300, mAs=200 => (90000*200)/500000 = 36
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 300, Mas: 200,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Block);
        result.Value.Message.Should().Contain("blocked");
        result.Value.IsAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateExposureAsync_EmergencyLevel_ReturnsEmergencyMessage()
    {
        // DAP > 50 (5x Chest DRL)
        // kvp=500, mAs=200 => (250000*200)/500000 = 100
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 500, Mas: 200,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.Value.Level.Should().Be(DoseValidationLevel.Emergency);
        result.Value.Message.Should().Contain("EMERGENCY");
        result.Value.IsAllowed.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateExposureAsync_ZeroKvp_ReturnsValidationError()
    {
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 0, Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ValidateExposureAsync_NegativeKvp_ReturnsValidationError()
    {
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: -10, Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateExposureAsync_ZeroMas_ReturnsValidationError()
    {
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 80, Mas: 0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ValidateExposureAsync_SmallFieldArea_ReturnsValidationError()
    {
        var parameters = new ExposureParameters(
            BodyPart: "Chest", Kvp: 80, Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 0.5);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ValidateExposureAsync_UnknownBodyPart_UsesDefaultDrl()
    {
        var parameters = new ExposureParameters(
            BodyPart: "UnknownPart", Kvp: 80, Mas: 2.0,
            StudyInstanceUid: "1.2.3.4.5", DistanceCm: 100.0, FieldAreaCm2: 100.0);

        var result = await _doseService.ValidateExposureAsync(parameters);

        result.IsSuccess.Should().BeTrue();
        result.Value.Level.Should().Be(DoseValidationLevel.Allow);
    }

    [Fact]
    public async Task ValidateExposureAsync_NullParameters_ThrowsArgumentNullException()
    {
        var act = async () => await _doseService.ValidateExposureAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── DoseService — CalculateEsd edge cases ────────────────────────────────

    [Fact]
    public void CalculateEsd_NegativeDap_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _doseService.CalculateEsd(-1.0, 100.0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("dap");
    }

    [Fact]
    public void CalculateEsd_BackscatterFactorLessThanOne_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _doseService.CalculateEsd(10.0, 100.0, backscatterFactor: 0.5);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("backscatterFactor");
    }

    [Fact]
    public void CalculateEsd_ZeroFieldArea_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _doseService.CalculateEsd(10.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("fieldAreaCm2");
    }

    // ── DoseService — CalculateExposureIndex edge cases ──────────────────────

    [Fact]
    public void CalculateExposureIndex_NegativeMeanPixel_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _doseService.CalculateExposureIndex(-1.0, 1000.0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("meanPixelValue");
    }

    [Fact]
    public void CalculateExposureIndex_ZeroTarget_ThrowsArgumentOutOfRangeException()
    {
        var act = () => _doseService.CalculateExposureIndex(500.0, 0.0);

        act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("targetPixelValue");
    }

    // ── DoseService — GenerateRdsrSummaryAsync edge cases ────────────────────

    [Fact]
    public async Task GenerateRdsrSummaryAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _doseService.GenerateRdsrSummaryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GenerateRdsrSummaryAsync_NonExistentStudy_ReturnsNotFound()
    {
        var result = await _doseService.GenerateRdsrSummaryAsync("nonexistent-uid");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── DoseService — RecordDoseAsync / GetDoseByStudyAsync edge cases ───────

    [Fact]
    public async Task RecordDoseAsync_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _doseService.RecordDoseAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetDoseByStudyAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _doseService.GetDoseByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetDoseHistoryAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _doseService.GetDoseHistoryAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── DoseService constructor ───────────────────────────────────────────────

    [Fact]
    public void DoseService_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new DoseService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("doseRepository");
    }
}
