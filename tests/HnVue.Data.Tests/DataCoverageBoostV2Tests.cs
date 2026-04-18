using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Data.Tests;

/// <summary>
/// Additional coverage boost tests for HnVue.Data module targeting 85%+ coverage.
/// Focuses on CancellationToken, exception paths, and edge cases in repositories.
/// </summary>
[Trait("Category", "Data")]
public sealed class DataCoverageBoostV2Tests
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

    // ── EfCdStudyRepository: CancellationToken ────────────────────────────────

    [Fact]
    public async Task EfCdStudyRepository_GetFilesForStudyAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetFilesForStudyAsync("STUDY-001", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfDoseRepository: Additional edge cases ───────────────────────────────

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_NoMatchingStudies_ReturnsEmpty()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // Arrange - Patient exists but has no studies
        ctx.Patients.Add(new PatientEntity
        {
            PatientId = "P-EMPTY",
            Name = "Empty^Patient",
            CreatedAtTicks = DateTimeOffset.UtcNow.Ticks
        });
        await ctx.SaveChangesAsync();

        var result = await repo.GetByPatientAsync("P-EMPTY", null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_WithFromAndUntil_ReturnsInRange()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        var now = DateTimeOffset.UtcNow;
        ctx.Patients.Add(new PatientEntity
        {
            PatientId = "P-DR",
            Name = "Date^Range",
            IsEmergency = false,
            CreatedAtTicks = now.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "test",
        });
        ctx.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "1.2.3.DR",
            PatientId = "P-DR",
            StudyDateTicks = now.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await ctx.SaveChangesAsync();

        // Add dose at specific times
        var early = now.AddHours(-2);
        var mid = now;
        var late = now.AddHours(2);

        await repo.SaveAsync(new DoseRecord("D-EARLY", "1.2.3.DR", 10.0, 200.0, 0.03, "CHEST", early));
        await repo.SaveAsync(new DoseRecord("D-MID", "1.2.3.DR", 15.0, 300.0, 0.04, "ABD", mid));
        await repo.SaveAsync(new DoseRecord("D-LATE", "1.2.3.DR", 20.0, 400.0, 0.05, "PELVIS", late));

        // Query for range that includes only mid
        var result = await repo.GetByPatientAsync("P-DR", now.AddHours(-1), now.AddHours(1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("D-MID");
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByPatientAsync("P001", null, null, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfDoseRepository_SaveAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var dose = new DoseRecord("D-CANCEL", "STUDY-001", 10.0, 200.0, 0.03, "CHEST", DateTimeOffset.UtcNow);
        var act = async () => await repo.SaveAsync(dose, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfDoseRepository_GetByStudyAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByStudyAsync("STUDY-001", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_RecordsOrderedByRecordedAtAscending()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        var now = DateTimeOffset.UtcNow;
        ctx.Patients.Add(new PatientEntity
        {
            PatientId = "P-ORD",
            Name = "Order^Test",
            CreatedAtTicks = now.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "test",
        });
        ctx.Studies.Add(new StudyEntity
        {
            StudyInstanceUid = "1.2.3.ORD",
            PatientId = "P-ORD",
            StudyDateTicks = now.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await ctx.SaveChangesAsync();

        // Add doses in reverse chronological order
        await repo.SaveAsync(new DoseRecord("D-3", "1.2.3.ORD", 30, 300, 3, "HEAD", now.AddHours(2)));
        await repo.SaveAsync(new DoseRecord("D-1", "1.2.3.ORD", 10, 100, 1, "CHEST", now));
        await repo.SaveAsync(new DoseRecord("D-2", "1.2.3.ORD", 20, 200, 2, "ABD", now.AddHours(1)));

        var result = await repo.GetByPatientAsync("P-ORD", null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        // Ordered ascending by RecordedAtTicks
        result.Value[0].DoseId.Should().Be("D-1");
        result.Value[1].DoseId.Should().Be("D-2");
        result.Value[2].DoseId.Should().Be("D-3");
    }

    // ── EfIncidentRepository: Additional edge cases ───────────────────────────

    [Fact]
    public async Task EfIncidentRepository_SaveAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var entity = new IncidentEntity
        {
            IncidentId = "INC-CANCEL",
            SeverityValue = 1,
            Description = "Cancel test",
            OccurredAtTicks = DateTimeOffset.UtcNow.Ticks,
            IsResolved = false
        };

        var act = async () => await repo.SaveAsync(entity, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfIncidentRepository_GetBySeverityAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetBySeverityAsync(1, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Seed an incident
        var entity = new IncidentEntity
        {
            IncidentId = "INC-CT",
            SeverityValue = 1,
            Description = "Cancel test",
            OccurredAtTicks = DateTimeOffset.UtcNow.Ticks,
            IsResolved = false
        };
        await repo.SaveAsync(entity);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.ResolveAsync("INC-CT", "resolution", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_SetsResolvedAtOffset()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        var entity = new IncidentEntity
        {
            IncidentId = "INC-OFF",
            SeverityValue = 2,
            Description = "Offset test",
            OccurredAtTicks = DateTimeOffset.UtcNow.Ticks,
            IsResolved = false
        };
        await repo.SaveAsync(entity);

        var result = await repo.ResolveAsync("INC-OFF", "Fixed");
        result.IsSuccess.Should().BeTrue();

        var resolved = await ctx.Incidents.FindAsync("INC-OFF");
        resolved.Should().NotBeNull();
        resolved!.ResolvedAtTicks.Should().BeGreaterThan(0);
        // ResolvedAtOffsetMinutes can be 0 if system timezone is UTC+0
        resolved.ResolvedAtOffsetMinutes.Should().BeInRange(-840, 840);
    }

    // ── EfUpdateRepository (Data layer): CancellationToken ────────────────────

    [Fact]
    public async Task EfUpdateRepository_CheckForUpdateAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.CheckForUpdateAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfUpdateRepository_RecordInstallationAsync_CancelledToken_Throws()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.RecordInstallationAsync("1.0.0", "2.0.0", "hash", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfCdStudyRepository: Multiple studies with images ─────────────────────

    [Fact]
    public async Task EfCdStudyRepository_MultipleStudies_ImagesSeparatedCorrectly()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Seed patients and studies
        ctx.Patients.Add(new PatientEntity { PatientId = "P1", Name = "P1", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Patients.Add(new PatientEntity { PatientId = "P2", Name = "P2", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "S1", PatientId = "P1", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "S2", PatientId = "P2", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        await ctx.SaveChangesAsync();

        // Add images for both studies
        ctx.Images.Add(new ImageEntity { ImageId = "I1", StudyInstanceUid = "S1", FilePath = "/s1/img1.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "I2", StudyInstanceUid = "S1", FilePath = "/s1/img2.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "I3", StudyInstanceUid = "S1", FilePath = "/s1/img3.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "I4", StudyInstanceUid = "S2", FilePath = "/s2/img1.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "I5", StudyInstanceUid = "S2", FilePath = "/s2/img2.dcm" });
        await ctx.SaveChangesAsync();

        var result1 = await repo.GetFilesForStudyAsync("S1");
        var result2 = await repo.GetFilesForStudyAsync("S2");

        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().HaveCount(3);

        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().HaveCount(2);
    }

    // ── StudyRepository: CancellationToken ────────────────────────────────────

    [Fact]
    public async Task StudyRepository_AddAsync_CancelledToken_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var study = new StudyRecord("1.2.3.CT", "P-CT", DateTimeOffset.UtcNow, "D", "A", "B");
        var act = async () => await repo.AddAsync(study, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StudyRepository_GetByPatientAsync_CancelledToken_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByPatientAsync("P001", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StudyRepository_GetByUidAsync_CancelledToken_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByUidAsync("1.2.3", cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StudyRepository_UpdateAsync_CancelledToken_Throws()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var study = new StudyRecord("1.2.3", "P", DateTimeOffset.UtcNow, "D", "A", "B");
        var act = async () => await repo.UpdateAsync(study, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── UserRepository: AddAsync duplicate username ───────────────────────────

    [Fact]
    public async Task UserRepository_AddAsync_DuplicateUsername_ReturnsAlreadyExists()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);

        var user1 = new UserRecord("U-DUP-1", "samername", "User1", "hash1", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);
        var user2 = new UserRecord("U-DUP-2", "samername", "User2", "hash2", Common.Enums.UserRole.Radiographer,
            0, false, null, null, 0, null);

        await repo.AddAsync(user1);
        var result = await repo.AddAsync(user2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AlreadyExists);
    }
}
