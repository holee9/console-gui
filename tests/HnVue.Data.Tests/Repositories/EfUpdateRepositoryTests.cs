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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync(null!, "1.1.0", "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_NullToVersion_ThrowsArgumentNullException()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfUpdateRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.RecordInstallationAsync("1.0.0", null!, "hash"));
    }

    [Fact]
    public async Task RecordInstallationAsync_NullPackageHash_SetsEmptyString()
    {
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
        await using var (ctx, connection) = CreateSqliteContext();
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
}
