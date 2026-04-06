using HnVue.Data.Entities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests;

/// <summary>
/// Basic smoke tests for <see cref="HnVueDbContext"/> configuration.
/// </summary>
public sealed class HnVueDbContextTests
{
    [Fact]
    public void Create_InMemoryContext_HasAllDbSets()
    {
        using var ctx = TestDbContextFactory.Create();

        ctx.Patients.Should().NotBeNull();
        ctx.Studies.Should().NotBeNull();
        ctx.Images.Should().NotBeNull();
        ctx.DoseRecords.Should().NotBeNull();
        ctx.Users.Should().NotBeNull();
        ctx.AuditLogs.Should().NotBeNull();
    }

    [Fact]
    public void Create_MultipleContexts_AreIsolated()
    {
        using var ctx1 = TestDbContextFactory.Create();
        using var ctx2 = TestDbContextFactory.Create();

        ctx1.Patients.Add(new Entities.PatientEntity
        {
            PatientId = "P999",
            Name = "Test^Patient",
            IsEmergency = false,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "test",
        });
        ctx1.SaveChanges();

        // ctx2 should not see changes from ctx1 (different in-memory databases)
        ctx2.Patients.Any(p => p.PatientId == "P999").Should().BeFalse();
    }

    // ── Issue #32: DeleteBehavior.Restrict guards (data integrity) ─────────────
    // These tests use SQLite in-memory instead of the EF InMemory provider because
    // the InMemory provider does not enforce foreign-key constraints.

    /// <summary>
    /// Creates a <see cref="HnVueDbContext"/> backed by a real SQLite in-memory database
    /// with foreign-key enforcement enabled. The caller owns the returned context and
    /// the underlying <see cref="SqliteConnection"/> and must dispose both.
    /// </summary>
    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        // Enable FK enforcement (SQLite disables it by default)
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    /// <summary>
    /// Verifies that deleting a patient that has associated studies raises an exception.
    /// DeleteBehavior.Restrict prevents silent cascade loss of audit-critical dose records.
    /// </summary>
    [Fact]
    public void DeletePatient_WithExistingStudy_ThrowsDbUpdateException()
    {
        var (ctx, conn) = CreateSqliteContext();
        using (conn)
        using (ctx)
        {
            var patient = new PatientEntity
            {
                PatientId = "P-RESTRICT-01",
                Name = "Restrict^Test",
                IsEmergency = false,
                CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                CreatedAtOffsetMinutes = 0,
                CreatedBy = "test",
            };
            var study = new StudyEntity
            {
                StudyInstanceUid = "1.2.3.999",
                PatientId = patient.PatientId,
                StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
                StudyDateOffsetMinutes = 0,
            };

            ctx.Patients.Add(patient);
            ctx.Studies.Add(study);
            ctx.SaveChanges();

            // Detach child so EF does not try to cascade-delete via tracked entities
            ctx.Entry(study).State = EntityState.Detached;
            ctx.Patients.Remove(patient);

            var act = () => ctx.SaveChanges();

            act.Should().Throw<Exception>("DeleteBehavior.Restrict must block patient deletion when studies exist");
        }
    }

    /// <summary>
    /// Verifies that deleting a study that has associated dose records raises an exception.
    /// Dose records are regulatory audit data and must not be silently removed.
    /// </summary>
    [Fact]
    public void DeleteStudy_WithExistingDoseRecord_ThrowsDbUpdateException()
    {
        var (ctx, conn) = CreateSqliteContext();
        using (conn)
        using (ctx)
        {
            var patient = new PatientEntity
            {
                PatientId = "P-RESTRICT-02",
                Name = "DoseRestrict^Test",
                IsEmergency = false,
                CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                CreatedAtOffsetMinutes = 0,
                CreatedBy = "test",
            };
            var study = new StudyEntity
            {
                StudyInstanceUid = "1.2.3.1000",
                PatientId = patient.PatientId,
                StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
                StudyDateOffsetMinutes = 0,
            };

            ctx.Patients.Add(patient);
            ctx.Studies.Add(study);
            ctx.SaveChanges();

            var dose = new DoseRecordEntity
            {
                DoseId = Guid.NewGuid().ToString(),
                StudyInstanceUid = study.StudyInstanceUid,
                BodyPart = "CHEST",
                Dap = 12.5,
                Ei = 400.0,
                EffectiveDose = 0.05,
                RecordedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
                RecordedAtOffsetMinutes = 0,
            };
            ctx.DoseRecords.Add(dose);
            ctx.SaveChanges();

            // Detach dose so EF does not try to cascade-delete via tracked entities
            ctx.Entry(dose).State = EntityState.Detached;
            ctx.Studies.Remove(study);

            var act = () => ctx.SaveChanges();

            act.Should().Throw<Exception>("DeleteBehavior.Restrict must block study deletion when dose records exist");
        }
    }
}
