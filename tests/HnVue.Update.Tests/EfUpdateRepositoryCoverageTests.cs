using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using HnVue.Update;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Coverage tests for <see cref="EfUpdateRepository"/> (Update module).
/// Tests CheckForUpdateAsync, GetPackageInfoAsync, and ApplyPackageAsync with SQLite.
/// </summary>
[Trait("Category", "Update")]
public sealed class EfUpdateRepositoryCoverageTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly HnVueDbContext _ctx;
    private readonly string _tempDir;

    public EfUpdateRepositoryCoverageTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(_connection)
            .Options;
        _ctx = new HnVueDbContext(options);
        _ctx.Database.EnsureCreated();

        _tempDir = Path.Combine(Path.GetTempPath(), $"EfUpdateRepoTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _connection.Dispose();
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private EfUpdateRepository CreateRepo() => new(_ctx);

    private string CreatePackageFile(string fileName = "HnVue-2.0.0.zip", string content = "package payload")
    {
        string path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    private async Task SeedUpdateHistoryAsync(string fromVersion, string toVersion, string status, string hash)
    {
        _ctx.UpdateHistories.Add(new UpdateHistoryEntity
        {
            Timestamp = DateTime.UtcNow,
            FromVersion = fromVersion,
            ToVersion = toVersion,
            Status = status,
            InstalledBy = "testuser",
            PackageHash = hash
        });
        await _ctx.SaveChangesAsync();
    }

    // ── CheckForUpdateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CheckForUpdateAsync_NoHistory_ReturnsNull()
    {
        var repo = CreateRepo();

        var result = await repo.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithInstalledHistory_ReturnsLatest()
    {
        var repo = CreateRepo();
        await SeedUpdateHistoryAsync("1.0.0", "1.1.0", "Installed", "hash1");

        var result = await repo.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.1.0");
        result.Value.Sha256Hash.Should().Be("hash1");
    }

    [Fact]
    public async Task CheckForUpdateAsync_MultipleInstalled_ReturnsMostRecent()
    {
        var repo = CreateRepo();
        await SeedUpdateHistoryAsync("1.0.0", "1.1.0", "Installed", "hash1");
        await Task.Delay(10);
        await SeedUpdateHistoryAsync("1.1.0", "1.2.0", "Installed", "hash2");

        var result = await repo.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value!.Version.Should().Be("1.2.0");
    }

    [Fact]
    public async Task CheckForUpdateAsync_OnlyNonInstalledStatus_ReturnsNull()
    {
        var repo = CreateRepo();
        await SeedUpdateHistoryAsync("1.0.0", "1.1.0", "Pending", "hash1");
        await SeedUpdateHistoryAsync("1.1.0", "1.2.0", "Failed", "hash2");

        var result = await repo.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var repo = CreateRepo();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.CheckForUpdateAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── GetPackageInfoAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetPackageInfoAsync_ExistingFile_ReturnsSuccess()
    {
        var repo = CreateRepo();
        string path = CreatePackageFile("HnVue-3.0.0.zip", "test content");

        var result = await repo.GetPackageInfoAsync(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("3.0.0");
        result.Value.PackageUrl.Should().Be(path);
        result.Value.Sha256Hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetPackageInfoAsync_NonExistentFile_ReturnsNotFound()
    {
        var repo = CreateRepo();
        string missingPath = Path.Combine(_tempDir, "nonexistent.zip");

        var result = await repo.GetPackageInfoAsync(missingPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GetPackageInfoAsync_NullPath_ThrowsArgumentNullException()
    {
        var repo = CreateRepo();

        var act = async () => await repo.GetPackageInfoAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPackageInfoAsync_FileWithoutDash_ReturnsDefaultVersion()
    {
        var repo = CreateRepo();
        string path = CreatePackageFile("package.zip", "content");

        var result = await repo.GetPackageInfoAsync(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("0.0.0");
    }

    [Fact]
    public async Task GetPackageInfoAsync_ComputesCorrectSha256()
    {
        var repo = CreateRepo();
        string content = "known content for hash check";
        string path = Path.Combine(_tempDir, "HnVue-1.0.0.zip");
        await File.WriteAllTextAsync(path, content);

        var result = await repo.GetPackageInfoAsync(path);

        result.IsSuccess.Should().BeTrue();
        // Verify the hash matches what we'd compute independently
        using var stream = File.OpenRead(path);
        var expectedHash = Convert.ToHexString(
            await System.Security.Cryptography.SHA256.HashDataAsync(stream)).ToLowerInvariant();
        result.Value.Sha256Hash.Should().Be(expectedHash);
    }

    // ── ApplyPackageAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackageAsync_ExistingFile_NoPriorHistory_ReturnsSuccess()
    {
        var repo = CreateRepo();
        string path = CreatePackageFile("HnVue-1.0.0.zip", "new install");

        var result = await repo.ApplyPackageAsync(path);

        result.IsSuccess.Should().BeTrue();

        var history = await _ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.FromVersion.Should().Be("0.0.0");
        history.ToVersion.Should().Be("1.0.0");
        history.Status.Should().Be("Installed");
    }

    [Fact]
    public async Task ApplyPackageAsync_ExistingFile_WithPriorHistory_UsesCurrentVersion()
    {
        var repo = CreateRepo();
        await SeedUpdateHistoryAsync("0.9.0", "1.0.0", "Installed", "oldhash");
        string path = CreatePackageFile("HnVue-1.1.0.zip", "update payload");

        var result = await repo.ApplyPackageAsync(path);

        result.IsSuccess.Should().BeTrue();

        var allHistory = await _ctx.UpdateHistories.OrderByDescending(h => h.Timestamp).ToListAsync();
        allHistory.Should().HaveCount(2);
        var latest = allHistory.First();
        latest.FromVersion.Should().Be("1.0.0");
        latest.ToVersion.Should().Be("1.1.0");
    }

    [Fact]
    public async Task ApplyPackageAsync_NonExistentFile_ReturnsNotFound()
    {
        var repo = CreateRepo();
        string missingPath = Path.Combine(_tempDir, "nonexistent.zip");

        var result = await repo.ApplyPackageAsync(missingPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ApplyPackageAsync_NullPath_ThrowsArgumentNullException()
    {
        var repo = CreateRepo();

        var act = async () => await repo.ApplyPackageAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ApplyPackageAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var repo = CreateRepo();
        string path = CreatePackageFile("HnVue-2.0.0.zip", "payload");
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.ApplyPackageAsync(path, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ApplyPackageAsync_FileWithoutDash_UsesCurrentVersionAsToVersion()
    {
        var repo = CreateRepo();
        await SeedUpdateHistoryAsync("0.9.0", "1.0.0", "Installed", "hash");
        string path = CreatePackageFile("package.zip", "payload");

        var result = await repo.ApplyPackageAsync(path);

        result.IsSuccess.Should().BeTrue();
        var latest = await _ctx.UpdateHistories.OrderByDescending(h => h.Timestamp).FirstOrDefaultAsync();
        latest!.ToVersion.Should().Be("1.0.0"); // Falls back to currentVersion
    }

    [Fact]
    public async Task ApplyPackageAsync_ComputesAndStoresHash()
    {
        var repo = CreateRepo();
        string content = "hash verification content";
        string path = Path.Combine(_tempDir, "HnVue-2.5.0.zip");
        await File.WriteAllTextAsync(path, content);

        await repo.ApplyPackageAsync(path);

        var history = await _ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.PackageHash.Should().NotBeNullOrEmpty();

        // Verify hash is correct
        using var stream = File.OpenRead(path);
        var expectedHash = Convert.ToHexString(
            await System.Security.Cryptography.SHA256.HashDataAsync(stream)).ToLowerInvariant();
        history.PackageHash.Should().Be(expectedHash);
    }

    [Fact]
    public async Task ApplyPackageAsync_SetsInstalledByToCurrentUserName()
    {
        var repo = CreateRepo();
        string path = CreatePackageFile("HnVue-1.5.0.zip", "payload");

        await repo.ApplyPackageAsync(path);

        var history = await _ctx.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.InstalledBy.Should().NotBeNullOrEmpty();
    }
}
