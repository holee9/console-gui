using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="PatientRepository"/> using an in-memory EF Core database.
/// </summary>
public sealed class PatientRepositoryTests
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

    private static PatientRecord CreateSamplePatient(string id = "P001") =>
        new(
            PatientId: id,
            Name: "Doe^John",
            DateOfBirth: new DateOnly(1980, 6, 15),
            Sex: "M",
            IsEmergency: false,
            CreatedAt: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy: "user-01");

    // ── AddAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewPatient_ReturnsSuccess()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        var patient = CreateSamplePatient();

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task AddAsync_PreservesAllFields()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        var patient = CreateSamplePatient();

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Doe^John");
        result.Value.DateOfBirth.Should().Be(new DateOnly(1980, 6, 15));
        result.Value.Sex.Should().Be("M");
        result.Value.IsEmergency.Should().BeFalse();
        result.Value.CreatedBy.Should().Be("user-01");
    }

    [Fact]
    public async Task AddAsync_NullDateOfBirth_IsPreserved()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        var patient = new PatientRecord("P002", "Smith^Jane", null, null, true,
            DateTimeOffset.UtcNow, "user-02");

        var result = await repo.AddAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.DateOfBirth.Should().BeNull();
        result.Value.Sex.Should().BeNull();
        result.Value.IsEmergency.Should().BeTrue();
    }

    // ── FindByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task FindByIdAsync_ExistingPatient_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.FindByIdAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task FindByIdAsync_NonExistentPatient_ReturnsSuccessWithNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.FindByIdAsync("NONE");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── SearchAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ByName_ReturnsMatchingPatients()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient("P001"));
        await repo.AddAsync(new PatientRecord("P002", "Smith^Jane", null, "F", false, DateTimeOffset.UtcNow, "user-01"));

        var result = await repo.SearchAsync("doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PatientId.Should().Be("P001");
    }

    [Fact]
    public async Task SearchAsync_ById_ReturnsMatchingPatient()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient("P001"));

        var result = await repo.SearchAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.SearchAsync("ZZZ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingPatient_PersistsChanges()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());

        var updated = new PatientRecord("P001", "Updated^Name", null, "F", true,
            DateTimeOffset.UtcNow, "user-01");
        var result = await repo.UpdateAsync(updated);

        result.IsSuccess.Should().BeTrue();

        var found = await repo.FindByIdAsync("P001");
        found.Value!.Name.Should().Be("Updated^Name");
        found.Value.IsEmergency.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentPatient_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.UpdateAsync(CreateSamplePatient("NONE"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingPatient_SoftDeletesRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());

        var result = await repo.DeleteAsync("P001");

        result.IsSuccess.Should().BeTrue();
        // Soft delete: record should not be findable (filtered by !IsDeleted)
        var found = await repo.FindByIdAsync("P001");
        found.Value.Should().BeNull("soft-deleted records should not be found");
    }

    [Fact]
    public async Task DeleteAsync_NonExistentPatient_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);

        var result = await repo.DeleteAsync("NONE");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound, "non-existent patient should return NotFound");
    }

    // ── CancellationToken propagation ─────────────────────────────────────────

    [Fact]
    public async Task FindByIdAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.FindByIdAsync("P001", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SearchAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repo = new PatientRepository(ctx, auditRepo, null);
        await repo.AddAsync(CreateSamplePatient());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.SearchAsync("P001", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── DbUpdateException paths (SQLite shared connection) ────────────────────

    [Fact]
    public async Task AddAsync_DuplicatePatientId_ReturnsDbError()
    {
        var patient = CreateSamplePatient("P-DUP");

        using var sharedConn = new SqliteConnection("Data Source=:memory:");
        sharedConn.Open();
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(sharedConn)
            .Options;

        await using (var ctxA = new HnVueDbContext(opts))
        {
            ctxA.Database.EnsureCreated();
            var auditRepoA = Substitute.For<IAuditRepository>();
            auditRepoA.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
            var repoA = new PatientRepository(ctxA, auditRepoA, null);
            await repoA.AddAsync(patient);
        }

        await using (var ctxB = new HnVueDbContext(opts))
        {
            var auditRepoB = Substitute.For<IAuditRepository>();
            auditRepoB.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
            var repoB = new PatientRepository(ctxB, auditRepoB, null);
            var result = await repoB.AddAsync(patient); // duplicate PK

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DatabaseError);
        }
    }

    [Fact]
    public async Task DeleteAsync_PatientWithStudy_SoftDeletesSuccessfully()
    {
        // Soft delete: deleting a patient with linked studies should succeed
        // (foreign key restrictions don't apply to soft deletes)
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();
        using var pragmaCmd = conn.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA foreign_keys = ON;";
        pragmaCmd.ExecuteNonQuery();

        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(conn)
            .Options;

        await using var ctx = new HnVueDbContext(opts);
        ctx.Database.EnsureCreated();

        // Add patient and linked study
        var auditRepo = Substitute.For<IAuditRepository>();
        auditRepo.GetLastHashAsync(default).ReturnsForAnyArgs(Result.SuccessNullable<string?>(null));
        var repoP = new PatientRepository(ctx, auditRepo, null);
        await repoP.AddAsync(CreateSamplePatient("P-RESTRICT"));
        ctx.Studies.Add(new Entities.StudyEntity
        {
            StudyInstanceUid = "1.2.3.RESTRICT",
            PatientId = "P-RESTRICT",
            StudyDateTicks = DateTimeOffset.UtcNow.UtcTicks,
            StudyDateOffsetMinutes = 0,
        });
        await ctx.SaveChangesAsync();

        // Detach study so EF does not cascade-delete via tracking
        foreach (var entry in ctx.ChangeTracker.Entries().ToList())
            entry.State = EntityState.Detached;

        // Soft delete should succeed even with linked studies
        var result = await repoP.DeleteAsync("P-RESTRICT");

        result.IsSuccess.Should().BeTrue("soft delete should succeed even with foreign key constraints");

        // Verify patient is soft-deleted
        var found = await repoP.FindByIdAsync("P-RESTRICT");
        found.Value.Should().BeNull("soft-deleted patient should not be found");
    }
}
