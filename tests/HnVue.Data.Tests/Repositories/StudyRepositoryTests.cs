using HnVue.Common.Models;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="StudyRepository"/> using an in-memory EF Core database.
/// </summary>
public sealed class StudyRepositoryTests
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

    private static StudyRecord CreateSampleStudy(string uid = "1.2.3.4.5", string patientId = "P001") =>
        new(
            StudyInstanceUid: uid,
            PatientId: patientId,
            StudyDate: new DateTimeOffset(2026, 1, 10, 8, 30, 0, TimeSpan.Zero),
            Description: "Chest PA",
            AccessionNumber: "ACC-001",
            BodyPart: "CHEST");

    // ── AddAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewStudy_ReturnsSuccess()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.AddAsync(CreateSampleStudy());

        result.IsSuccess.Should().BeTrue();
        result.Value.StudyInstanceUid.Should().Be("1.2.3.4.5");
    }

    [Fact]
    public async Task AddAsync_PreservesAllFields()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.AddAsync(CreateSampleStudy());

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("P001");
        result.Value.Description.Should().Be("Chest PA");
        result.Value.AccessionNumber.Should().Be("ACC-001");
        result.Value.BodyPart.Should().Be("CHEST");
    }

    [Fact]
    public async Task AddAsync_NullOptionalFields_IsPreserved()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        var study = new StudyRecord("1.2.3.4.6", "P001", DateTimeOffset.UtcNow, null, null, null);

        var result = await repo.AddAsync(study);

        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
        result.Value.AccessionNumber.Should().BeNull();
        result.Value.BodyPart.Should().BeNull();
    }

    // ── GetByPatientAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByPatientAsync_ExistingPatient_ReturnsStudies()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(CreateSampleStudy("1.2.3.4.5", "P001"));
        await repo.AddAsync(CreateSampleStudy("1.2.3.4.6", "P001"));
        await repo.AddAsync(CreateSampleStudy("1.2.3.4.7", "P002"));

        var result = await repo.GetByPatientAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByPatientAsync_NoStudies_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.GetByPatientAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── GetByUidAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByUidAsync_ExistingStudy_ReturnsRecord()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(CreateSampleStudy());

        var result = await repo.GetByUidAsync("1.2.3.4.5");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.StudyInstanceUid.Should().Be("1.2.3.4.5");
    }

    [Fact]
    public async Task GetByUidAsync_NonExistent_ReturnsSuccessWithNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.GetByUidAsync("NONE");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingStudy_PersistsChanges()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(CreateSampleStudy());

        var updated = new StudyRecord("1.2.3.4.5", "P001",
            new DateTimeOffset(2026, 2, 1, 9, 0, 0, TimeSpan.Zero),
            "Hand AP", "ACC-999", "HAND");
        var result = await repo.UpdateAsync(updated);

        result.IsSuccess.Should().BeTrue();

        var found = await repo.GetByUidAsync("1.2.3.4.5");
        found.Value!.Description.Should().Be("Hand AP");
        found.Value.BodyPart.Should().Be("HAND");
    }

    [Fact]
    public async Task UpdateAsync_NonExistentStudy_ReturnsNotFound()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);

        var result = await repo.UpdateAsync(CreateSampleStudy("NONE.UID"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── CancellationToken propagation ─────────────────────────────────────────

    [Fact]
    public async Task GetByPatientAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(CreateSampleStudy());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByPatientAsync("P001", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetByUidAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new StudyRepository(ctx);
        await repo.AddAsync(CreateSampleStudy());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByUidAsync("1.2.3.4.5", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── DbUpdateException paths (SQLite shared connection for PK constraint) ──────

    [Fact]
    public async Task AddAsync_DuplicateStudyUid_ReturnsDbError()
    {
        var study = CreateSampleStudy("1.2.3.DUP");

        using var sharedConn = new SqliteConnection("Data Source=:memory:");
        sharedConn.Open();
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(sharedConn)
            .Options;

        await using (var ctxA = new HnVueDbContext(opts))
        {
            ctxA.Database.EnsureCreated();
            var repoA = new StudyRepository(ctxA);
            await repoA.AddAsync(study);
        }

        await using (var ctxB = new HnVueDbContext(opts))
        {
            var repoB = new StudyRepository(ctxB);
            var result = await repoB.AddAsync(study); // duplicate PK

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DatabaseError);
        }
    }
}
