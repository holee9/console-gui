using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Linq;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfUpdateRepository"/> using an in-memory EF Core database.
/// REQ-COORD-004: SPEC-COORDINATOR-001 EF Core update history persistence.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfUpdateRepositoryTests
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

    // ── CheckForUpdateAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckForUpdateAsync_NoHistory_ReturnsNull()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithInstalledUpdate_ReturnsUpdateInfo()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Arrange
        await repo.RecordInstallationAsync("1.0.0", "1.1.0", "abc123");

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.1.0");
        result.Value.Sha256Hash.Should().Be("abc123");
    }

    [Fact]
    public async Task CheckForUpdateAsync_MultipleUpdates_ReturnsLatest()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Arrange - Record multiple updates
        await repo.RecordInstallationAsync("1.0.0", "1.1.0", "hash1");
        await Task.Delay(10); // Ensure timestamp difference
        await repo.RecordInstallationAsync("1.1.0", "1.2.0", "hash2");
        await Task.Delay(10);
        await repo.RecordInstallationAsync("1.2.0", "1.3.0", "hash3");

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.3.0");
        result.Value.Sha256Hash.Should().Be("hash3");
    }

    [Fact]
    public async Task CheckForUpdateAsync_OnlyInstalledStatus_ReturnsLatest()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Arrange
        ctx.UpdateHistories.Add(new Data.Entities.UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-10),
            FromVersion = "1.0.0",
            ToVersion = "1.1.0",
            Status = "Pending",
            PackageHash = "pending-hash"
        });
        ctx.UpdateHistories.Add(new Data.Entities.UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-5),
            FromVersion = "1.1.0",
            ToVersion = "1.2.0",
            Status = "Installed",
            PackageHash = "installed-hash"
        });
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be("1.2.0");
        result.Value.Sha256Hash.Should().Be("installed-hash");
    }

    // ── RecordInstallationAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task RecordInstallationAsync_ValidParameters_ReturnsSuccess()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act
        var result = await repo.RecordInstallationAsync("1.0.0", "1.1.0", "abc123");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify record
        var history = await ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.FromVersion.Should().Be("1.0.0");
        history.ToVersion.Should().Be("1.1.0");
        history.Status.Should().Be("Installed");
        history.PackageHash.Should().Be("abc123");
    }

    [Fact]
    public async Task RecordInstallationAsync_NullFromVersion_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync(null!, "1.1.0", "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_NullToVersion_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync("1.0.0", null!, "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_NullPackageHash_SetsEmptyString()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act
        var result = await repo.RecordInstallationAsync("1.0.0", "1.1.0", null);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var history = await ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.PackageHash.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordInstallationAsync_SetsInstalledByToCurrentUserName()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act
        await repo.RecordInstallationAsync("1.0.0", "1.1.0", "hash");

        // Assert
        var history = await ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.InstalledBy.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RecordInstallationAsync_MultipleInstallations_PreservesAllHistory()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act
        await repo.RecordInstallationAsync("1.0.0", "1.1.0", "hash1");
        await repo.RecordInstallationAsync("1.1.0", "1.2.0", "hash2");
        await repo.RecordInstallationAsync("1.2.0", "1.3.0", "hash3");

        // Assert
        var count = await ctx.UpdateHistories.CountAsync();
        count.Should().Be(3);

        var allHistory = await ctx.UpdateHistories.ToListAsync();
        allHistory.Any(h => h.ToVersion == "1.1.0").Should().BeTrue();
        allHistory.Any(h => h.ToVersion == "1.2.0").Should().BeTrue();
        allHistory.Any(h => h.ToVersion == "1.3.0").Should().BeTrue();
    }

    [Fact]
    public async Task RoundTrip_InstallationAndCheck_ReturnsCorrectUpdateInfo()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Arrange - Record installation
        await repo.RecordInstallationAsync("1.0.0", "2.0.0", "release-hash");

        // Act - Check for update
        var result = await repo.CheckForUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("2.0.0");
        result.Value.ReleaseNotes.Should().Contain("1.0.0");
        result.Value.Sha256Hash.Should().Be("release-hash");
    }

    [Fact]
    public async Task CheckForUpdateAsync_CancelledToken_ThrowsOperationCanceledException()
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
    public async Task RecordInstallationAsync_CancelledToken_ThrowsOperationCanceledException()
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

    [Fact]
    public async Task RecordInstallationAsync_EmptyFromVersion_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert - Empty string is treated as null
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync("", "2.0.0", "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_EmptyToVersion_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert - Empty string is treated as null
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync("1.0.0", "", "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_SetsTimestampToUtcNow()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        var before = DateTime.UtcNow;

        await repo.RecordInstallationAsync("1.0.0", "2.0.0", "hash");

        var after = DateTime.UtcNow.AddSeconds(1);

        var history = await ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.Timestamp.Should().BeOnOrAfter(before);
        history.Timestamp.Should().BeBefore(after);
    }

    [Fact]
    public async Task CheckForUpdateAsync_MixedStatuses_ReturnsLatestInstalled()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfUpdateRepository(ctx);

        // Arrange - Mix of Pending and Installed records
        ctx.UpdateHistories.Add(new Data.Entities.UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow.AddHours(-3),
            FromVersion = "1.0.0", ToVersion = "1.1.0",
            Status = "Pending", InstalledBy = "test", PackageHash = "h1"
        });
        ctx.UpdateHistories.Add(new Data.Entities.UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow.AddHours(-2),
            FromVersion = "1.1.0", ToVersion = "1.2.0",
            Status = "Installed", InstalledBy = "test", PackageHash = "h2"
        });
        ctx.UpdateHistories.Add(new Data.Entities.UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow.AddHours(-1),
            FromVersion = "1.2.0", ToVersion = "1.3.0",
            Status = "Pending", InstalledBy = "test", PackageHash = "h3"
        });
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.CheckForUpdateAsync();

        // Assert - Only "Installed" status counts
        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be("1.2.0");
        result.Value.Sha256Hash.Should().Be("h2");
    }
}
