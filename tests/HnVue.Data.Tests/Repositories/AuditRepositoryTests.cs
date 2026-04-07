using HnVue.Common.Models;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="AuditRepository"/> using an in-memory EF Core database.
/// </summary>
public sealed class AuditRepositoryTests
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

    private static AuditEntry CreateEntry(
        string action = "LOGIN",
        string userId = "U001",
        string? previousHash = null,
        DateTimeOffset? timestamp = null) =>
        new(
            EntryId: Guid.NewGuid().ToString(),
            Timestamp: timestamp ?? DateTimeOffset.UtcNow,
            UserId: userId,
            Action: action,
            Details: null,
            PreviousHash: previousHash,
            CurrentHash: Guid.NewGuid().ToString("N"));

    // ── AppendAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AppendAsync_ValidEntry_ReturnsSuccess()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var result = await repo.AppendAsync(CreateEntry());

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task AppendAsync_PreservesAllFields()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        var entryId = Guid.NewGuid().ToString();
        var timestamp = new DateTimeOffset(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        var entry = new AuditEntry(entryId, timestamp, "U001", "EXPOSE",
            "{\"dose\": 0.5}", "prevHash123", "currHash456");

        await repo.AppendAsync(entry);

        var filter = new AuditQueryFilter(UserId: "U001");
        var query = await repo.QueryAsync(filter);
        var stored = query.Value[0];
        stored.EntryId.Should().Be(entryId);
        stored.UserId.Should().Be("U001");
        stored.Action.Should().Be("EXPOSE");
        stored.PreviousHash.Should().Be("prevHash123");
        stored.CurrentHash.Should().Be("currHash456");
        stored.Timestamp.UtcTicks.Should().Be(timestamp.UtcTicks);
    }

    // ── GetLastHashAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetLastHashAsync_EmptyLog_ReturnsNull()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var result = await repo.GetLastHashAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetLastHashAsync_WithEntries_ReturnsMostRecentHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var t1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero);

        var first = new AuditEntry(Guid.NewGuid().ToString(), t1, "U001", "LOGIN",
            null, null, "hash-first");
        var second = new AuditEntry(Guid.NewGuid().ToString(), t2, "U001", "LOGOUT",
            null, "hash-first", "hash-second");

        await repo.AppendAsync(first);
        await repo.AppendAsync(second);

        var result = await repo.GetLastHashAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hash-second");
    }

    // ── QueryAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_NoFilter_ReturnsAllEntries()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        await repo.AppendAsync(CreateEntry("LOGIN", "U001"));
        await repo.AppendAsync(CreateEntry("EXPOSE", "U002"));

        var result = await repo.QueryAsync(new AuditQueryFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryAsync_FilterByUserId_ReturnsMatchingEntries()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        await repo.AppendAsync(CreateEntry("LOGIN", "U001"));
        await repo.AppendAsync(CreateEntry("EXPOSE", "U001"));
        await repo.AppendAsync(CreateEntry("LOGIN", "U002"));

        var result = await repo.QueryAsync(new AuditQueryFilter(UserId: "U001"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(e => e.UserId.Should().Be("U001"));
    }

    [Fact]
    public async Task QueryAsync_FilterByDateRange_ReturnsMatchingEntries()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var t1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        await repo.AppendAsync(CreateEntry(timestamp: t1));
        await repo.AppendAsync(CreateEntry(timestamp: t2));
        await repo.AppendAsync(CreateEntry(timestamp: t3));

        var from = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 1, 20, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.QueryAsync(new AuditQueryFilter(FromDate: from, ToDate: to));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryAsync_MaxResults_LimitsReturnedEntries()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        for (var i = 0; i < 10; i++)
            await repo.AppendAsync(CreateEntry());

        var result = await repo.QueryAsync(new AuditQueryFilter(MaxResults: 3));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task QueryAsync_EmptyLog_ReturnsEmptyList()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var result = await repo.QueryAsync(new AuditQueryFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_FilterByFromDateOnly_ReturnsEntriesOnOrAfter()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var t1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        await repo.AppendAsync(CreateEntry(timestamp: t1));
        await repo.AppendAsync(CreateEntry(timestamp: t2));

        var from = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.QueryAsync(new AuditQueryFilter(FromDate: from));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Timestamp.UtcTicks.Should().Be(t2.UtcTicks);
    }

    [Fact]
    public async Task QueryAsync_FilterByToDateOnly_ReturnsEntriesOnOrBefore()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var t1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        await repo.AppendAsync(CreateEntry(timestamp: t1));
        await repo.AppendAsync(CreateEntry(timestamp: t2));

        var to = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var result = await repo.QueryAsync(new AuditQueryFilter(ToDate: to));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Timestamp.UtcTicks.Should().Be(t1.UtcTicks);
    }

    [Fact]
    public async Task GetLastHashAsync_SingleEntry_ReturnsItsHash()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        var entry = new AuditEntry(Guid.NewGuid().ToString(),
            new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
            "U001", "LOGIN", null, null, "only-hash");

        await repo.AppendAsync(entry);

        var result = await repo.GetLastHashAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("only-hash");
    }

    [Fact]
    public async Task QueryAsync_OrderedByTimestamp_ReturnedInAscendingOrder()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);

        var t1 = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var t2 = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var t3 = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);

        // Insert out of order
        await repo.AppendAsync(CreateEntry(timestamp: t3));
        await repo.AppendAsync(CreateEntry(timestamp: t1));
        await repo.AppendAsync(CreateEntry(timestamp: t2));

        var result = await repo.QueryAsync(new AuditQueryFilter());

        result.IsSuccess.Should().BeTrue();
        result.Value[0].Timestamp.UtcTicks.Should().Be(t1.UtcTicks);
        result.Value[1].Timestamp.UtcTicks.Should().Be(t2.UtcTicks);
        result.Value[2].Timestamp.UtcTicks.Should().Be(t3.UtcTicks);
    }

    // ── CancellationToken propagation ─────────────────────────────────────────

    [Fact]
    public async Task GetLastHashAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        await repo.AppendAsync(CreateEntry());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetLastHashAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task QueryAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        await using var ctx = TestDbContextFactory.Create();
        var repo = new AuditRepository(ctx);
        await repo.AppendAsync(CreateEntry());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.QueryAsync(new AuditQueryFilter(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── DbUpdateException paths (SQLite to enforce PK constraint) ─────────────

    [Fact]
    public async Task AppendAsync_DuplicateEntryId_ReturnsDbError()
    {
        var entryId = Guid.NewGuid().ToString();
        var first = new AuditEntry(entryId, DateTimeOffset.UtcNow, "U001", "LOGIN",
            null, null, "hash-a");
        var duplicate = new AuditEntry(entryId, DateTimeOffset.UtcNow, "U001", "LOGOUT",
            null, "hash-a", "hash-b");

        // Use a shared SQLite connection so both contexts see the same data
        using var sharedConn = new SqliteConnection("Data Source=:memory:");
        sharedConn.Open();
        var opts = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(sharedConn)
            .Options;

        await using (var ctxA = new HnVueDbContext(opts))
        {
            ctxA.Database.EnsureCreated();
            var repoA = new AuditRepository(ctxA);
            await repoA.AppendAsync(first);
        }

        await using (var ctxB = new HnVueDbContext(opts))
        {
            var repoB = new AuditRepository(ctxB);
            var result = await repoB.AppendAsync(duplicate); // same PK — SQLite PK constraint fails

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DatabaseError);
        }
    }
}
