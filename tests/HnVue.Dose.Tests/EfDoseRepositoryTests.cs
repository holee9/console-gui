using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Dose.Tests;

[Trait("SWR", "SWR-DS-020")]
public sealed class EfDoseRepositoryTests : IDisposable
{
    private readonly HnVueDbContext _context;
    private readonly EfDoseRepository _sut;

    public EfDoseRepositoryTests()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _sut = new EfDoseRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private static DoseRecord CreateDoseRecord(
        string studyUid = "1.2.3.4.5",
        string doseId = "dose-001",
        double dap = 1.5,
        double ei = 0.8,
        double effectiveDose = 0.12,
        string bodyPart = "Chest",
        DateTimeOffset? recordedAt = null) =>
        new(doseId, studyUid, dap, ei, effectiveDose, bodyPart,
            recordedAt ?? DateTimeOffset.UtcNow);

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

    // ── SaveAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidRecord_SavesToDatabase()
    {
        var dose = CreateDoseRecord();

        var result = await _sut.SaveAsync(dose);

        result.IsSuccess.Should().BeTrue();
        var saved = await _context.DoseRecords.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.StudyInstanceUid.Should().Be("1.2.3.4.5");
        saved.Dap.Should().Be(1.5);
    }

    [Fact]
    public async Task SaveAsync_NullDose_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("dose");
    }

    [Fact]
    public async Task SaveAsync_MultipleRecords_SavesAll()
    {
        var dose1 = CreateDoseRecord(doseId: "d1", studyUid: "uid1");
        var dose2 = CreateDoseRecord(doseId: "d2", studyUid: "uid2");

        await _sut.SaveAsync(dose1);
        await _sut.SaveAsync(dose2);

        var count = await _context.DoseRecords.CountAsync();
        count.Should().Be(2);
    }

    [Fact]
    public async Task SaveAsync_PreservesTimestamp()
    {
        var timestamp = new DateTimeOffset(2026, 4, 13, 10, 30, 0, TimeSpan.FromHours(9));
        var dose = CreateDoseRecord(recordedAt: timestamp);

        await _sut.SaveAsync(dose);

        var saved = await _context.DoseRecords.FirstOrDefaultAsync();
        saved!.RecordedAtTicks.Should().Be(timestamp.UtcTicks);
        saved.RecordedAtOffsetMinutes.Should().Be((int)timestamp.Offset.TotalMinutes);
    }

    [Fact]
    public async Task SaveAsync_PreservesAllFields()
    {
        var dose = CreateDoseRecord(
            studyUid: "1.2.3.4.5.6",
            doseId: "dose-full",
            dap: 2.5,
            ei: 1.3,
            effectiveDose: 0.45,
            bodyPart: "Abdomen");

        await _sut.SaveAsync(dose);

        var saved = await _context.DoseRecords.FirstOrDefaultAsync();
        saved!.DoseId.Should().Be("dose-full");
        saved.StudyInstanceUid.Should().Be("1.2.3.4.5.6");
        saved.Dap.Should().Be(2.5);
        saved.Ei.Should().Be(1.3);
        saved.EffectiveDose.Should().Be(0.45);
        saved.BodyPart.Should().Be("Abdomen");
    }

    // ── GetByStudyAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByStudyAsync_ExistingRecord_ReturnsRecord()
    {
        var dose = CreateDoseRecord(studyUid: "study-exists");
        await _sut.SaveAsync(dose);

        var result = await _sut.GetByStudyAsync("study-exists");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.StudyInstanceUid.Should().Be("study-exists");
    }

    [Fact]
    public async Task GetByStudyAsync_NonExistent_ReturnsNull()
    {
        var result = await _sut.GetByStudyAsync("nonexistent-uid");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetByStudyAsync_NullStudyUid_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetByStudyAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("studyInstanceUid");
    }

    [Fact]
    public async Task GetByStudyAsync_MapsFieldsCorrectly()
    {
        var timestamp = new DateTimeOffset(2026, 4, 13, 12, 0, 0, TimeSpan.FromHours(9));
        var dose = CreateDoseRecord(studyUid: "map-test", dap: 3.14, recordedAt: timestamp);
        await _sut.SaveAsync(dose);

        var result = await _sut.GetByStudyAsync("map-test");

        result.Value!.Dap.Should().Be(3.14);
        result.Value.StudyInstanceUid.Should().Be("map-test");
        result.Value.RecordedAt.Offset.Should().Be(TimeSpan.FromHours(9));
    }

    // ── GetByPatientAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetByPatientAsync_NoStudies_ReturnsEmptyList()
    {
        var result = await _sut.GetByPatientAsync("no-patient", null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetByPatientAsync(null!, null, null);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    [Fact]
    public async Task GetByPatientAsync_WithStudies_ReturnsMatchingDoseRecords()
    {
        var patientId = "pat-001";
        var studyUid = "study-for-pat";
        await SeedPatientAndStudy(patientId, studyUid);

        var dose = CreateDoseRecord(studyUid: studyUid);
        await _sut.SaveAsync(dose);

        var result = await _sut.GetByPatientAsync(patientId, null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].StudyInstanceUid.Should().Be(studyUid);
    }

    [Fact]
    public async Task GetByPatientAsync_WithFromFilter_FiltersCorrectly()
    {
        var patientId = "pat-filter";
        var studyUid = "study-filter";
        await SeedPatientAndStudy(patientId, studyUid);

        var oldDose = CreateDoseRecord(doseId: "old", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow.AddDays(-10));
        var newDose = CreateDoseRecord(doseId: "new", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow);
        await _sut.SaveAsync(oldDose);
        await _sut.SaveAsync(newDose);

        var from = DateTimeOffset.UtcNow.AddDays(-1);
        var result = await _sut.GetByPatientAsync(patientId, from, null);

        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("new");
    }

    [Fact]
    public async Task GetByPatientAsync_WithUntilFilter_FiltersCorrectly()
    {
        var patientId = "pat-until";
        var studyUid = "study-until";
        await SeedPatientAndStudy(patientId, studyUid);

        var oldDose = CreateDoseRecord(doseId: "old-u", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow.AddDays(-10));
        var newDose = CreateDoseRecord(doseId: "new-u", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow);
        await _sut.SaveAsync(oldDose);
        await _sut.SaveAsync(newDose);

        var until = DateTimeOffset.UtcNow.AddDays(-1);
        var result = await _sut.GetByPatientAsync(patientId, null, until);

        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("old-u");
    }

    [Fact]
    public async Task GetByPatientAsync_NoDateFilter_ReturnsAllForPatient()
    {
        var patientId = "pat-all";
        var studyUid = "study-all";
        await SeedPatientAndStudy(patientId, studyUid);

        await _sut.SaveAsync(CreateDoseRecord(doseId: "d1", studyUid: studyUid));
        await _sut.SaveAsync(CreateDoseRecord(doseId: "d2", studyUid: studyUid));

        var result = await _sut.GetByPatientAsync(patientId, null, null);

        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByPatientAsync_BothFilters_ReturnsOnlyInRange()
    {
        var patientId = "pat-range";
        var studyUid = "study-range";
        await SeedPatientAndStudy(patientId, studyUid);

        var d1 = CreateDoseRecord(doseId: "day-5", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow.AddDays(-5));
        var d2 = CreateDoseRecord(doseId: "day-2", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow.AddDays(-2));
        var d3 = CreateDoseRecord(doseId: "day-0", studyUid: studyUid,
            recordedAt: DateTimeOffset.UtcNow);
        await _sut.SaveAsync(d1);
        await _sut.SaveAsync(d2);
        await _sut.SaveAsync(d3);

        var from = DateTimeOffset.UtcNow.AddDays(-3);
        var until = DateTimeOffset.UtcNow.AddDays(-1);
        var result = await _sut.GetByPatientAsync(patientId, from, until);

        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("day-2");
    }
}
