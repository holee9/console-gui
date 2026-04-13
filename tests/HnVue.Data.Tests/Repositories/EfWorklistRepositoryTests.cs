using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfWorklistRepository"/> using an in-memory EF Core database.
/// REQ-COORD-002: SPEC-COORDINATOR-001 EF Core worklist query.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfWorklistRepositoryTests
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

    private static (Data.Entities.PatientEntity Patient, Data.Entities.StudyEntity Study) CreateSamplePatientAndStudy(
        string patientId = "P001",
        string studyInstanceUid = "STUDY-001",
        DateOnly? studyDate = null)
    {
        var patient = new Data.Entities.PatientEntity
        {
            PatientId = patientId,
            Name = "Test^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        };

        var studyDateValue = studyDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var studyDateTime = new DateTimeOffset(studyDateValue, TimeSpan.Zero);

        var study = new Data.Entities.StudyEntity
        {
            StudyInstanceUid = studyInstanceUid,
            PatientId = patientId,
            AccessionNumber = "ACC-001",
            BodyPart = "CHEST",
            Description = "CXR",
            StudyDateTicks = studyDateTime.UtcTicks,
            StudyDateOffsetMinutes = (int)studyDateTime.Offset.TotalMinutes
        };

        return (patient, study);
    }

    // ── QueryTodayAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryTodayAsync_TodaysStudies_ReturnsWorklistItems()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfWorklistRepository(ctx);

        // Arrange - Add study scheduled for today
        var (patient, study) = CreateSamplePatientAndStudy(
            studyDate: DateOnly.FromDateTime(DateTime.UtcNow));
        ctx.Patients.Add(patient);
        ctx.Studies.Add(study);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        var item = result.Value[0];
        item.PatientId.Should().Be("P001");
        item.PatientName.Should().Be("Test^Patient");
        item.AccessionNumber.Should().Be("ACC-001");
        item.BodyPart.Should().Be("CHEST");
        item.RequestedProcedure.Should().Be("CXR");
    }

    [Fact]
    public async Task QueryTodayAsync_NoStudiesToday_ReturnsEmptyList()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfWorklistRepository(ctx);

        // Arrange - Add study scheduled for yesterday
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        var (patient, study) = CreateSamplePatientAndStudy(studyDate: yesterday);
        ctx.Patients.Add(patient);
        ctx.Studies.Add(study);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_MultipleStudiesToday_ReturnsAllItems()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfWorklistRepository(ctx);

        // Arrange - Add multiple studies for today
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var (patient1, study1) = CreateSamplePatientAndStudy("P001", "STUDY-001", today);
        study1.AccessionNumber = "ACC-001";
        study1.BodyPart = "CHEST";
        study1.Description = "CXR";

        var (patient2, study2) = CreateSamplePatientAndStudy("P002", "STUDY-002", today);
        study2.AccessionNumber = "ACC-002";
        study2.BodyPart = "ABDOMEN";
        study2.Description = "AXR";

        ctx.Patients.Add(patient1);
        ctx.Patients.Add(patient2);
        ctx.Studies.Add(study1);
        ctx.Studies.Add(study2);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Any(i => i.AccessionNumber == "ACC-001").Should().BeTrue();
        result.Value.Any(i => i.AccessionNumber == "ACC-002").Should().BeTrue();
    }

    [Fact]
    public async Task QueryTodayAsync_WithNullAccessionNumber_ReturnsEmptyAccession()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfWorklistRepository(ctx);

        // Arrange
        var (patient, study) = CreateSamplePatientAndStudy();
        study.AccessionNumber = null;
        ctx.Patients.Add(patient);
        ctx.Studies.Add(study);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].AccessionNumber.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_PreservesStudyDate()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfWorklistRepository(ctx);

        // Arrange
        var expectedDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var (patient, study) = CreateSamplePatientAndStudy(studyDate: expectedDate);
        ctx.Patients.Add(patient);
        ctx.Studies.Add(study);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.QueryTodayAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].StudyDate.Should().Be(expectedDate);
    }
}
