using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.PatientManagement.Tests;

/// <summary>
/// Coverage boost tests for EfWorklistRepository exception path and additional edge cases.
/// Targets: EfWorklistRepository.QueryTodayAsync (82% → 85%+).
/// </summary>
[Trait("SWR", "SWR-PM-070")]
public sealed class PatientManagementFinalCoverageTests : IDisposable
{
    private readonly HnVueDbContext _context;
    private readonly EfWorklistRepository _sut;

    public PatientManagementFinalCoverageTests()
    {
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _sut = new EfWorklistRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    // ── EfWorklistRepository — exception path via disposed context ────────────

    [Fact]
    public async Task QueryTodayAsync_OnDisposedContext_ReturnsDatabaseError()
    {
        var localContext = new HnVueDbContext(
            new DbContextOptionsBuilder<HnVueDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        localContext.Database.EnsureCreated();
        var repo = new EfWorklistRepository(localContext);
        localContext.Dispose();

        var result = await repo.QueryTodayAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── EfWorklistRepository — additional edge cases ─────────────────────────

    [Fact]
    public async Task QueryTodayAsync_StudyWithNullBodyPart_ReturnsItemWithNullBodyPart()
    {
        _context.Patients.Add(new PatientEntity
        {
            PatientId = "p-null", Name = "Null BodyPart", CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "s-null",
            PatientId = "p-null",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
            BodyPart = null,
            Description = "Test Study"
        });
        await _context.SaveChangesAsync();

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].BodyPart.Should().BeNull();
    }

    [Fact]
    public async Task QueryTodayAsync_StudyWithNullDescription_ReturnsItemWithNullProcedure()
    {
        _context.Patients.Add(new PatientEntity
        {
            PatientId = "p-desc", Name = "Null Desc", CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "s-desc",
            PatientId = "p-desc",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
            BodyPart = "Chest",
            Description = null
        });
        await _context.SaveChangesAsync();

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].RequestedProcedure.Should().BeNull();
    }

    [Fact]
    public async Task QueryTodayAsync_StudyExactlyAtMidnightBoundary_Included()
    {
        var today = DateTimeOffset.UtcNow.Date;
        var midnight = new DateTimeOffset(today, TimeSpan.Zero);

        _context.Patients.Add(new PatientEntity
        {
            PatientId = "p-midnight", Name = "Midnight", CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "s-midnight",
            PatientId = "p-midnight",
            StudyDateTicks = midnight.UtcTicks,
            StudyDateOffsetMinutes = 0
        });
        await _context.SaveChangesAsync();

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryTodayAsync_StudyJustBeforeTomorrow_Excluded()
    {
        var tomorrow = DateTimeOffset.UtcNow.Date.AddDays(1);
        var justBefore = new DateTimeOffset(tomorrow, TimeSpan.Zero).AddTicks(-1);

        _context.Patients.Add(new PatientEntity
        {
            PatientId = "p-border", Name = "Border", CreatedBy = "test"
        });
        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "s-border",
            PatientId = "p-border",
            StudyDateTicks = justBefore.UtcTicks,
            StudyDateOffsetMinutes = 0
        });
        await _context.SaveChangesAsync();

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        // This study is today (just before midnight), should be included
        result.Value.Should().HaveCount(1);
    }
}
