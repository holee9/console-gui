using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-PM-020")]
public sealed class EfWorklistRepositoryTests : IDisposable
{
    private readonly HnVueDbContext _context;
    private readonly EfWorklistRepository _sut;

    public EfWorklistRepositoryTests()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _sut = new EfWorklistRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task SeedStudyWithPatient(
        string patientId = "pat-001",
        string patientName = "Test Patient",
        string studyUid = "1.2.3.4.5",
        string? accessionNumber = "ACC-001",
        string? bodyPart = "Chest",
        string? description = "Chest PA",
        DateTimeOffset? studyDate = null)
    {
        var date = studyDate ?? DateTimeOffset.UtcNow;
        _context.Patients.Add(new PatientEntity
        {
            PatientId = patientId, Name = patientName, CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid,
            PatientId = patientId,
            StudyDateTicks = date.UtcTicks,
            StudyDateOffsetMinutes = (int)date.Offset.TotalMinutes,
            AccessionNumber = accessionNumber,
            BodyPart = bodyPart,
            Description = description
        });
        await _context.SaveChangesAsync();
    }

    // ── QueryTodayAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task QueryTodayAsync_NoStudies_ReturnsEmptyList()
    {
        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_WithTodayStudy_ReturnsItem()
    {
        await SeedStudyWithPatient();

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be("pat-001");
        result.Value[0].PatientName.Should().Be("Test Patient");
    }

    [Fact]
    public async Task QueryTodayAsync_MapsFieldsCorrectly()
    {
        await SeedStudyWithPatient(
            accessionNumber: "ACC-MAP",
            bodyPart: "Abdomen",
            description: "Abdomen CT");

        var result = await _sut.QueryTodayAsync();

        result.Value[0].AccessionNumber.Should().Be("ACC-MAP");
        result.Value[0].BodyPart.Should().Be("Abdomen");
        result.Value[0].RequestedProcedure.Should().Be("Abdomen CT");
    }

    [Fact]
    public async Task QueryTodayAsync_ExcludesYesterdayStudy()
    {
        await SeedStudyWithPatient(studyUid: "yesterday",
            studyDate: DateTimeOffset.UtcNow.AddDays(-1));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_ExcludesTomorrowStudy()
    {
        await SeedStudyWithPatient(studyUid: "tomorrow",
            studyDate: DateTimeOffset.UtcNow.AddDays(1));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_MultipleTodayStudies_ReturnsAll()
    {
        await SeedStudyWithPatient(patientId: "p1", studyUid: "s1");
        await SeedStudyWithPatient(patientId: "p2", studyUid: "s2",
            patientName: "Second Patient");

        var result = await _sut.QueryTodayAsync();

        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryTodayAsync_StudyDateMappedCorrectly()
    {
        var today = DateTimeOffset.UtcNow;
        await SeedStudyWithPatient(studyDate: today);

        var result = await _sut.QueryTodayAsync();

        result.Value[0].StudyDate.Should().NotBeNull();
        result.Value[0].StudyDate!.Value.Should().Be(DateOnly.FromDateTime(today.Date));
    }

    [Fact]
    public async Task QueryTodayAsync_NullAccessionNumber_MapsToEmpty()
    {
        await SeedStudyWithPatient(accessionNumber: null);

        var result = await _sut.QueryTodayAsync();

        result.Value[0].AccessionNumber.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_MixedTodayAndYesterday_ReturnsOnlyToday()
    {
        await SeedStudyWithPatient(patientId: "p-today", studyUid: "s-today",
            studyDate: DateTimeOffset.UtcNow);
        await SeedStudyWithPatient(patientId: "p-yesterday", studyUid: "s-yesterday",
            studyDate: DateTimeOffset.UtcNow.AddDays(-1));

        var result = await _sut.QueryTodayAsync();

        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be("p-today");
    }
}
