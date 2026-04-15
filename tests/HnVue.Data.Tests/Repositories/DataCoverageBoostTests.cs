using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Additional coverage tests for Data repositories targeting 85%+ coverage.
/// Covers: UserRepository.AddAsync, CancellationToken paths for Incident/Dose/CdStudy,
/// HnVueDbContextFactory, StudyRepository CancellationToken for Add/Update.
/// </summary>
[Trait("Category", "Data")]
public sealed class DataCoverageBoostTests
{
    // ── UserRepository.AddAsync ────────────────────────────────────────────────

    [Fact]
    public async Task UserRepository_AddAsync_ValidUser_ReturnsSuccess()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new UserRepository(ctx);
        var user = new UserRecord(
            UserId: "U-NEW",
            Username: "newuser",
            DisplayName: "New User",
            PasswordHash: "$2b$12$hash",
            Role: UserRole.Radiographer,
            FailedLoginCount: 0,
            IsLocked: false,
            LastLoginAt: null,
            QuickPinHash: null,
            QuickPinFailedCount: 0,
            QuickPinLockedUntil: null);

        var result = await repo.AddAsync(user);

        result.IsSuccess.Should().BeTrue();
        var found = await repo.GetByUsernameAsync("newuser");
        found.IsSuccess.Should().BeTrue();
        found.Value.UserId.Should().Be("U-NEW");
    }

    [Fact]
    public async Task UserRepository_AddAsync_DuplicateUsername_ReturnsAlreadyExists()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new UserRepository(ctx);

        var user = new UserRecord(
            UserId: "U-DUP1", Username: "dupuser", DisplayName: "Dup1",
            PasswordHash: "hash", Role: UserRole.Radiographer,
            FailedLoginCount: 0, IsLocked: false, LastLoginAt: null,
            QuickPinHash: null, QuickPinFailedCount: 0, QuickPinLockedUntil: null);
        await repo.AddAsync(user);

        var user2 = new UserRecord(
            UserId: "U-DUP2", Username: "dupuser", DisplayName: "Dup2",
            PasswordHash: "hash", Role: UserRole.Radiographer,
            FailedLoginCount: 0, IsLocked: false, LastLoginAt: null,
            QuickPinHash: null, QuickPinFailedCount: 0, QuickPinLockedUntil: null);
        var result = await repo.AddAsync(user2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AlreadyExists);
    }

    // ── UserRepository CancellationToken for update methods ────────────────────

    [Fact]
    public async Task UserRepository_UpdateFailedLoginCountAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = new UserEntity
        {
            UserId = "U001", Username = "testuser", DisplayName = "Test",
            PasswordHash = "hash", RoleValue = 0, FailedLoginCount = 0, IsLocked = false
        };
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.UpdateFailedLoginCountAsync("U001", 1, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UserRepository_SetLockedAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = new UserEntity
        {
            UserId = "U001", Username = "testuser", DisplayName = "Test",
            PasswordHash = "hash", RoleValue = 0, FailedLoginCount = 0, IsLocked = false
        };
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.SetLockedAsync("U001", true, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UserRepository_UpdatePasswordHashAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = new UserEntity
        {
            UserId = "U001", Username = "testuser", DisplayName = "Test",
            PasswordHash = "hash", RoleValue = 0, FailedLoginCount = 0, IsLocked = false
        };
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.UpdatePasswordHashAsync("U001", "newhash", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UserRepository_SetQuickPinHashAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = new UserEntity
        {
            UserId = "U001", Username = "testuser", DisplayName = "Test",
            PasswordHash = "hash", RoleValue = 0, FailedLoginCount = 0, IsLocked = false
        };
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.SetQuickPinHashAsync("U001", "pinhash", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UserRepository_UpdateQuickPinFailureAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var entity = new UserEntity
        {
            UserId = "U001", Username = "testuser", DisplayName = "Test",
            PasswordHash = "hash", RoleValue = 0, FailedLoginCount = 0, IsLocked = false
        };
        ctx.Users.Add(entity);
        await ctx.SaveChangesAsync();
        var repo = new UserRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.UpdateQuickPinFailureAsync("U001", 1, DateTimeOffset.UtcNow, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfIncidentRepository CancellationToken ─────────────────────────────────

    [Fact]
    public async Task EfIncidentRepository_GetBySeverityAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetBySeverityAsync(3, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfIncidentRepository_ResolveAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        var incident = new IncidentEntity
        {
            IncidentId = "INC-001", SeverityValue = 3,
            Description = "Test", OccurredAtTicks = DateTimeOffset.UtcNow.Ticks, IsResolved = false
        };
        ctx.Incidents.Add(incident);
        await ctx.SaveChangesAsync();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.ResolveAsync("INC-001", "Fixed", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfDoseRepository CancellationToken ─────────────────────────────────────

    [Fact]
    public async Task EfDoseRepository_GetByStudyAsync_CancelledToken_ThrowsOperationCanceledException()
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
    public async Task EfDoseRepository_GetByPatientAsync_CancelledToken_ThrowsOperationCanceledException()
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
    public async Task EfDoseRepository_SaveAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        var dose = new DoseRecord(
            "DOSE-001", "STUDY-001", 100.5, 50.2, 2.5, "CHEST",
            new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.Zero));

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.SaveAsync(dose, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfCdStudyRepository CancellationToken ───────────────────────────────────

    [Fact]
    public async Task EfCdStudyRepository_GetFilesForStudyAsync_CancelledToken_ThrowsOperationCanceledException()
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

    // ── EfCdStudyRepository additional coverage ────────────────────────────────

    [Fact]
    public async Task EfCdStudyRepository_GetFilesForStudyAsync_MultipleStudies_ReturnsCorrectPathsPerStudy()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfCdStudyRepository(ctx);

        // Arrange: Add patients and studies for FK constraints
        ctx.Patients.Add(new PatientEntity { PatientId = "P001", Name = "Test^Patient", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Patients.Add(new PatientEntity { PatientId = "P002", Name = "Test^Patient2", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "STUDY-A", PatientId = "P001", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "STUDY-B", PatientId = "P002", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Images.Add(new ImageEntity { ImageId = "IMG-A1", StudyInstanceUid = "STUDY-A", FilePath = "/a/img1.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "IMG-A2", StudyInstanceUid = "STUDY-A", FilePath = "/a/img2.dcm" });
        ctx.Images.Add(new ImageEntity { ImageId = "IMG-B1", StudyInstanceUid = "STUDY-B", FilePath = "/b/img1.dcm" });
        await ctx.SaveChangesAsync();

        var resultA = await repo.GetFilesForStudyAsync("STUDY-A");
        var resultB = await repo.GetFilesForStudyAsync("STUDY-B");

        resultA.IsSuccess.Should().BeTrue();
        resultA.Value.Should().HaveCount(2);
        resultB.IsSuccess.Should().BeTrue();
        resultB.Value.Should().HaveCount(1);
        resultB.Value[0].Should().Be("/b/img1.dcm");
    }

    // ── StudyRepository CancellationToken for Add/Update ──────────────────────

    [Fact]
    public async Task StudyRepository_AddAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        var study = new StudyRecord("1.2.3.4.5", "P001", DateTimeOffset.UtcNow, null, null, null);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.AddAsync(study, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task StudyRepository_UpdateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(new StudyRecord("1.2.3.4.5", "P001", DateTimeOffset.UtcNow, "Chest", null, null));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var updated = new StudyRecord("1.2.3.4.5", "P001", DateTimeOffset.UtcNow, "Updated", null, null);
        var act = async () => await repo.UpdateAsync(updated, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfDoseRepository additional: GetByPatientAsync with until filter ───────

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_WithUntilFilter_ReturnsFilteredRecords()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        ctx.Patients.Add(new PatientEntity { PatientId = "P001", Name = "Test^Patient", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "STUDY-001", PatientId = "P001", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        await ctx.SaveChangesAsync();

        var earlyDate = new DateTimeOffset(2026, 4, 10, 8, 0, 0, TimeSpan.Zero);
        var lateDate = new DateTimeOffset(2026, 4, 15, 8, 0, 0, TimeSpan.Zero);
        await repo.SaveAsync(new DoseRecord("DOSE-EARLY", "STUDY-001", 100, 50, 2.5, "CHEST", earlyDate));
        await repo.SaveAsync(new DoseRecord("DOSE-LATE", "STUDY-001", 200, 100, 5.0, "ABDOMEN", lateDate));

        // Filter: only records before April 13
        var until = new DateTimeOffset(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.GetByPatientAsync("P001", null, until);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("DOSE-EARLY");
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_WithBothDateFilters_ReturnsMatchingOnly()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        ctx.Patients.Add(new PatientEntity { PatientId = "P001", Name = "Test^Patient", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "STUDY-001", PatientId = "P001", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        await ctx.SaveChangesAsync();

        var d1 = new DateTimeOffset(2026, 4, 10, 8, 0, 0, TimeSpan.Zero);
        var d2 = new DateTimeOffset(2026, 4, 12, 8, 0, 0, TimeSpan.Zero);
        var d3 = new DateTimeOffset(2026, 4, 14, 8, 0, 0, TimeSpan.Zero);
        await repo.SaveAsync(new DoseRecord("DOSE-1", "STUDY-001", 100, 50, 2.5, "CHEST", d1));
        await repo.SaveAsync(new DoseRecord("DOSE-2", "STUDY-001", 200, 100, 5.0, "HAND", d2));
        await repo.SaveAsync(new DoseRecord("DOSE-3", "STUDY-001", 300, 150, 7.5, "FOOT", d3));

        // Filter: between April 11 and April 13
        var from = new DateTimeOffset(2026, 4, 11, 0, 0, 0, TimeSpan.Zero);
        var until = new DateTimeOffset(2026, 4, 13, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.GetByPatientAsync("P001", from, until);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].DoseId.Should().Be("DOSE-2");
    }

    [Fact]
    public async Task EfDoseRepository_GetByPatientAsync_NoStudiesForPatient_ReturnsEmpty()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        // No patients or studies added
        var result = await repo.GetByPatientAsync("NONEXISTENT", null, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── EfDoseRepository additional: SaveAsync preserves all fields ────────────

    [Fact]
    public async Task EfDoseRepository_SaveAsync_PreservesAllFields()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfDoseRepository(ctx);

        ctx.Patients.Add(new PatientEntity { PatientId = "P001", Name = "Test^Patient", CreatedAtTicks = DateTimeOffset.UtcNow.Ticks });
        ctx.Studies.Add(new StudyEntity { StudyInstanceUid = "STUDY-001", PatientId = "P001", StudyDateTicks = DateTimeOffset.UtcNow.Ticks });
        await ctx.SaveChangesAsync();

        var recordedAt = new DateTimeOffset(2026, 4, 12, 10, 30, 0, TimeSpan.FromHours(9));
        var dose = new DoseRecord("DOSE-FULL", "STUDY-001", 123.45, 67.89, 3.14, "HAND", recordedAt);
        await repo.SaveAsync(dose);

        var result = await repo.GetByStudyAsync("STUDY-001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.DoseId.Should().Be("DOSE-FULL");
        result.Value.StudyInstanceUid.Should().Be("STUDY-001");
        result.Value.Dap.Should().Be(123.45);
        result.Value.Ei.Should().Be(67.89);
        result.Value.EffectiveDose.Should().Be(3.14);
        result.Value.BodyPart.Should().Be("HAND");
    }

    // ── EfUpdateRepository (Data layer) CancellationToken ─────────────────────

    [Fact]
    public async Task EfUpdateRepository_CheckForUpdateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new Data.Repositories.EfUpdateRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.CheckForUpdateAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task EfUpdateRepository_RecordInstallationAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new Data.Repositories.EfUpdateRepository(ctx);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.RecordInstallationAsync("1.0.0", "1.1.0", "hash", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfIncidentRepository SaveAsync with CancellationToken ─────────────────

    [Fact]
    public async Task EfIncidentRepository_SaveAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);
        var incident = new IncidentEntity
        {
            IncidentId = "INC-CT", SeverityValue = 1,
            Description = "Test", OccurredAtTicks = DateTimeOffset.UtcNow.Ticks, IsResolved = false
        };
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.SaveAsync(incident, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EfIncidentRepository additional coverage ──────────────────────────────

    [Fact]
    public async Task EfIncidentRepository_GetBySeverityAsync_EmptyDatabase_ReturnsEmptyList()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        var result = await repo.GetBySeverityAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── Helper ──────────────────────────────────────────────────────────────────

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
}
