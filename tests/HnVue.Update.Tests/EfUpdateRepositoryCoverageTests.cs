using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System.IO;

namespace HnVue.Update.Tests;

/// <summary>
/// Coverage tests for EfUpdateRepository (was 0% coverage).
/// Tests CheckForUpdateAsync, GetPackageInfoAsync, ApplyPackageAsync.
/// </summary>
public sealed class EfUpdateRepositoryCoverageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly HnVueDbContext _context;

    public EfUpdateRepositoryCoverageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"EfUpdRepo_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        var dbPath = Path.Combine(_tempDir, "test.db");
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;
        _context = new HnVueDbContext(options);
        _context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }
    }

    private EfUpdateRepository CreateSut() => new(_context);

    private string CreatePackageFile(string fileName, string content = "test package content")
    {
        var path = Path.Combine(_tempDir, fileName);
        File.WriteAllText(path, content);
        return path;
    }

    // ── CheckForUpdateAsync ─────────────────────────────────────────────

    [Fact]
    public async Task CheckForUpdateAsync_EmptyDatabase_ReturnsNull()
    {
        var sut = CreateSut();
        var result = await sut.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_WithHistory_ReturnsLatestInstalled()
    {
        _context.UpdateHistories.AddRange(
            new UpdateHistoryEntity
            {
                FromVersion = "1.0.0", ToVersion = "1.1.0", Status = "Installed",
                Timestamp = DateTime.UtcNow.AddHours(-2), InstalledBy = "test",
                PackageHash = "hash1",
            },
            new UpdateHistoryEntity
            {
                FromVersion = "1.1.0", ToVersion = "1.2.0", Status = "Installed",
                Timestamp = DateTime.UtcNow.AddHours(-1), InstalledBy = "test",
                PackageHash = "hash2",
            });
        await _context.SaveChangesAsync();

        var sut = CreateSut();
        var result = await sut.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.2.0");
    }

    [Fact]
    public async Task CheckForUpdateAsync_OnlyFailedStatus_ReturnsNull()
    {
        _context.UpdateHistories.Add(new UpdateHistoryEntity
        {
            FromVersion = "1.0.0", ToVersion = "1.1.0", Status = "Failed",
            Timestamp = DateTime.UtcNow, InstalledBy = "test",
            PackageHash = "hash",
        });
        await _context.SaveChangesAsync();

        var sut = CreateSut();
        var result = await sut.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── GetPackageInfoAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetPackageInfoAsync_FileNotFound_ReturnsFailure()
    {
        var sut = CreateSut();
        var result = await sut.GetPackageInfoAsync(Path.Combine(_tempDir, "nonexistent.zip"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GetPackageInfoAsync_ValidFile_ReturnsInfo()
    {
        var path = CreatePackageFile("HnVue-2.0.0.zip");

        var sut = CreateSut();
        var result = await sut.GetPackageInfoAsync(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("2.0.0");
        result.Value.Sha256Hash.Should().NotBeEmpty();
        result.Value.PackageUrl.Should().Be(path);
    }

    [Fact]
    public async Task GetPackageInfoAsync_NoVersionInName_DefaultsTo000()
    {
        var path = CreatePackageFile("package.zip");

        var sut = CreateSut();
        var result = await sut.GetPackageInfoAsync(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("0.0.0");
    }

    // ── ApplyPackageAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackageAsync_FileNotFound_ReturnsFailure()
    {
        var sut = CreateSut();
        var result = await sut.ApplyPackageAsync(Path.Combine(_tempDir, "nonexistent.zip"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ApplyPackageAsync_FirstTime_FromVersionIs000()
    {
        var path = CreatePackageFile("HnVue-1.0.0.zip");

        var sut = CreateSut();
        var result = await sut.ApplyPackageAsync(path);

        result.IsSuccess.Should().BeTrue();

        var history = await _context.UpdateHistories.FirstOrDefaultAsync();
        history.Should().NotBeNull();
        history!.FromVersion.Should().Be("0.0.0");
        history.ToVersion.Should().Be("1.0.0");
        history.Status.Should().Be("Installed");
    }

    [Fact]
    public async Task ApplyPackageAsync_UpdateFromPreviousVersion()
    {
        _context.UpdateHistories.Add(new UpdateHistoryEntity
        {
            FromVersion = "1.0.0", ToVersion = "1.1.0", Status = "Installed",
            Timestamp = DateTime.UtcNow.AddDays(-1), InstalledBy = "test",
            PackageHash = "old",
        });
        await _context.SaveChangesAsync();

        var path = CreatePackageFile("HnVue-1.2.0.zip", "new version content");

        var sut = CreateSut();
        var result = await sut.ApplyPackageAsync(path);

        result.IsSuccess.Should().BeTrue();

        var latest = await _context.UpdateHistories
            .OrderByDescending(h => h.Timestamp)
            .FirstAsync();
        latest.FromVersion.Should().Be("1.1.0");
        latest.ToVersion.Should().Be("1.2.0");
    }

    [Fact]
    public async Task ApplyPackageAsync_ComputesCorrectHash()
    {
        var content = "verify this content";
        var path = CreatePackageFile("HnVue-3.0.0.zip", content);

        // Expected SHA-256
        using var stream = File.OpenRead(path);
        var expectedHash = Convert.ToHexString(
            await System.Security.Cryptography.SHA256.HashDataAsync(stream)).ToLowerInvariant();

        var sut = CreateSut();
        await sut.ApplyPackageAsync(path);

        var entry = await _context.UpdateHistories.FirstAsync();
        entry.PackageHash.Should().Be(expectedHash);
    }
}
