using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfDoseRepository"/> using an in-memory EF Core database.
/// REQ-COORD-001: SPEC-COORDINATOR-001 EF Core dose persistence.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfDoseRepositoryTests
{
    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    private static DoseRecord CreateSampleDose(
        string studyInstanceUid = "STUDY-001") =>
        new(
            DoseId: "DOSE-001",
            StudyInstanceUid: studyInstanceUid,
            Dap: 100.5,
            Ei: 50.2,
            EffectiveDose: 2.5,
            BodyPart: "CHEST",
            RecordedAt: new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero));

    // ── SaveAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidDose_ReturnsSuccess()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);
        var dose = CreateSampleDose();

        // Arrange - Create parent study for FK constraint
        ctx.Patients.Add(new Data.Entities.PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        });
        ctx.Studies.Add(new Data.Entities.StudyEntity
        {
            StudyInstanceUid = "STUDY-001",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.Ticks
        });
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.SaveAsync(dose);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_NullDose_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SaveAsync(null!));
    }

    // ── GetByStudyAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByStudyAsync_ExistingDose_ReturnsDoseRecord()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);
        var dose = CreateSampleDose();

        // Arrange - Create parent study for FK constraint
        ctx.Patients.Add(new Data.Entities.PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        });
        ctx.Studies.Add(new Data.Entities.StudyEntity
        {
            StudyInstanceUid = "STUDY-001",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.Ticks
        });
        await ctx.SaveChangesAsync();

        await repo.SaveAsync(dose);

        // Act
        var result = await repo.GetByStudyAsync("STUDY-001");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DoseId.Should().Be("DOSE-001");
        result.Value.Dap.Should().Be(100.5);
        result.Value.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public async Task GetByStudyAsync_NonExistingStudy_ReturnsNull()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Act
        var result = await repo.GetByStudyAsync("NON-EXISTING");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetByStudyAsync_NullStudyInstanceUid_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.GetByStudyAsync(null!));
    }

    // ── GetByPatientAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByPatientAsync_ExistingPatient_ReturnsDoseRecords()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Arrange - Add patient and studies
        ctx.Patients.Add(new Data.Entities.PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        });
        ctx.Studies.Add(new Data.Entities.StudyEntity
        {
            StudyInstanceUid = "STUDY-001",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.Ticks
        });
        ctx.Studies.Add(new Data.Entities.StudyEntity
        {
            StudyInstanceUid = "STUDY-002",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.Ticks
        });
        await ctx.SaveChangesAsync();

        // Add dose records
        await repo.SaveAsync(CreateSampleDose("STUDY-001"));
        await repo.SaveAsync(new DoseRecord(
            "DOSE-002", "STUDY-002", 200, 100, 5, "ABDOMEN",
            new DateTimeOffset(2026, 4, 12, 11, 0, 0, TimeSpan.Zero)));

        // Act
        var result = await repo.GetByPatientAsync("P001", null, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByPatientAsync_WithDateRange_ReturnsFilteredRecords()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Arrange
        ctx.Patients.Add(new Data.Entities.PatientEntity
        {
            PatientId = "P001",
            Name = "Test^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        });
        ctx.Studies.Add(new Data.Entities.StudyEntity
        {
            StudyInstanceUid = "STUDY-001",
            PatientId = "P001",
            StudyDateTicks = DateTimeOffset.UtcNow.Ticks
        });
        await ctx.SaveChangesAsync();

        var baseDate = new DateTimeOffset(2026, 4, 12, 10, 0, 0, TimeSpan.Zero);
        await repo.SaveAsync(new DoseRecord(
            "DOSE-001", "STUDY-001", 100, 50, 2.5, "CHEST", baseDate));

        // Act - Query with date range that excludes the record
        var from = new DateTimeOffset(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.GetByPatientAsync("P001", from, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByPatientAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.GetByPatientAsync(null!, null, null));
    }
}
