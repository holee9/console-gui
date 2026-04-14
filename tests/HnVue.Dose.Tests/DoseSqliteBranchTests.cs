using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Dose.Tests;

/// <summary>
/// SQLite in-memory tests for EfDoseRepository.SaveAsync DbUpdateException branches.
/// SQLite enforces unique and FK constraints, enabling real DbUpdateException testing.
/// Safety-critical: SWR-DS-081 — repository exception handling validation.
/// </summary>
[Trait("SWR", "SWR-DS-081")]
public sealed class DoseSqliteBranchTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HnVueDbContext _context;
    private readonly EfDoseRepository _sut;

    public DoseSqliteBranchTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Enable FK enforcement for SQLite
        using (var cmd = _connection.CreateCommand())
        {
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
        }

        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(_connection)
            .Options;
        _context = new HnVueDbContext(opts);
        _context.Database.EnsureCreated();
        _sut = new EfDoseRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

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

    private async Task SeedStudyAsync(string studyUid, string patientId = "pat-sqlite")
    {
        // Ensure patient exists
        if (!await _context.Patients.AnyAsync(p => p.PatientId == patientId))
        {
            _context.Patients.Add(new PatientEntity
            {
                PatientId = patientId, Name = "SQLite Patient", CreatedBy = "test",
            });
        }

        _context.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = studyUid, PatientId = patientId,
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks, StudyDateOffsetMinutes = 0,
        });
        await _context.SaveChangesAsync();
    }

    // ── SaveAsync — DbUpdateException with InnerException ──────────────────────

    [Fact]
    public async Task SaveAsync_DuplicatePrimaryKey_ReturnsDatabaseError()
    {
        await SeedStudyAsync("study-dup");

        // Save first record
        var dose1 = CreateDoseRecord(doseId: "unique-id", studyUid: "study-dup");
        var result1 = await _sut.SaveAsync(dose1);
        result1.IsSuccess.Should().BeTrue();

        // Try to save with same DoseId (primary key) — SQLite throws DbUpdateException
        var dose2 = CreateDoseRecord(doseId: "unique-id", studyUid: "study-dup");
        var result2 = await _sut.SaveAsync(dose2);

        // EfDoseRepository.SaveAsync catches DbUpdateException → Result.Failure
        result2.IsFailure.Should().BeTrue();
        result2.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── SaveAsync — success path via SQLite ────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidRecord_SavesToSqlite()
    {
        await SeedStudyAsync("sqlite-study");

        var dose = CreateDoseRecord(
            doseId: "sqlite-test",
            studyUid: "sqlite-study",
            dap: 5.5,
            ei: 2.0,
            effectiveDose: 0.55,
            bodyPart: "Abdomen");

        var result = await _sut.SaveAsync(dose);

        result.IsSuccess.Should().BeTrue();
        var saved = await _context.DoseRecords
            .FirstOrDefaultAsync(d => d.DoseId == "sqlite-test");
        saved.Should().NotBeNull();
        saved!.Dap.Should().Be(5.5);
        saved.BodyPart.Should().Be("Abdomen");
    }

    // ── GetByStudyAsync — success and not-found via SQLite ─────────────────────

    [Fact]
    public async Task GetByStudyAsync_ExistingRecord_ReturnsFromSqlite()
    {
        await SeedStudyAsync("find-me");

        var dose = CreateDoseRecord(studyUid: "find-me");
        await _sut.SaveAsync(dose);

        var result = await _sut.GetByStudyAsync("find-me");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.StudyInstanceUid.Should().Be("find-me");
    }

    [Fact]
    public async Task GetByStudyAsync_NotFound_ReturnsNull()
    {
        var result = await _sut.GetByStudyAsync("nonexistent-sqlite");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── EfDoseRepository — disposed SQLite connection triggers exception with InnerException ─

    [Fact]
    public async Task SaveAsync_OnDisposedConnection_HandlesError()
    {
        await SeedStudyAsync("study-disposed");
        var dose = CreateDoseRecord(studyUid: "study-disposed");

        // Close the underlying connection to force a database error
        _connection.Close();

        var result = await _sut.SaveAsync(dose);

        // Should catch the exception and return DatabaseError
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task GetByStudyAsync_OnDisposedConnection_HandlesError()
    {
        await SeedStudyAsync("study-query-disposed");

        // Close the underlying connection to force a query error
        _connection.Close();

        var result = await _sut.GetByStudyAsync("study-query-disposed");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task GetByPatientAsync_OnDisposedConnection_HandlesError()
    {
        await SeedStudyAsync("study-pat-disposed");

        // Close the underlying connection to force a query error
        _connection.Close();

        var result = await _sut.GetByPatientAsync("pat-sqlite", null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }
}
