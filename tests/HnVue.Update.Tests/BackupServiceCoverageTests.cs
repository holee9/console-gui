using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Coverage tests for <see cref="BackupService"/>.
/// Targets constructor validation, backup creation, restore, and listing.
/// </summary>
public sealed class BackupServiceCoverageTests
{
    private static string CreateTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), $"hnvue_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        return dir;
    }

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullAppDir_ThrowsArgumentNullException()
    {
        var act = () => new BackupService(null!, "/tmp/backup");

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullBackupDir_ThrowsArgumentNullException()
    {
        var act = () => new BackupService("/tmp/app", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── CreateBackupAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBackupAsync_NonExistentAppDir_ReturnsValidationFailed()
    {
        var backupDir = CreateTempDir();
        try
        {
            var sut = new BackupService("C:/nonexistent_app_dir_xyz", backupDir);
            var result = await sut.CreateBackupAsync();

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ValidationFailed);
        }
        finally
        {
            Directory.Delete(backupDir, true);
        }
    }

    [Fact]
    public async Task CreateBackupAsync_ValidDir_ReturnsBackupPath()
    {
        var appDir = CreateTempDir();
        var backupDir = CreateTempDir();
        try
        {
            // Create a file in app dir
            await File.WriteAllTextAsync(Path.Combine(appDir, "test.txt"), "hello");

            var sut = new BackupService(appDir, backupDir);
            var result = await sut.CreateBackupAsync();

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Contain("backup_");
            Directory.Exists(result.Value).Should().BeTrue();
        }
        finally
        {
            Directory.Delete(appDir, true);
            Directory.Delete(backupDir, true);
        }
    }

    // ── RestoreAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task RestoreAsync_NullBackupPath_ThrowsArgumentNullException()
    {
        var sut = new BackupService("C:/app", "C:/backup");
        var act = async () => await sut.RestoreAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RestoreAsync_NonExistentBackupPath_ReturnsNotFound()
    {
        var appDir = CreateTempDir();
        try
        {
            var sut = new BackupService(appDir, "C:/backup");
            var result = await sut.RestoreAsync("C:/nonexistent_backup_xyz");

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.NotFound);
        }
        finally
        {
            Directory.Delete(appDir, true);
        }
    }

    [Fact]
    public async Task RestoreAsync_ValidBackup_RestoresSuccessfully()
    {
        var appDir = CreateTempDir();
        var backupDir = CreateTempDir();
        try
        {
            // Create a file in app dir and back it up
            await File.WriteAllTextAsync(Path.Combine(appDir, "original.txt"), "original");
            var sut = new BackupService(appDir, backupDir);
            var backupResult = await sut.CreateBackupAsync();

            // Modify the original
            await File.WriteAllTextAsync(Path.Combine(appDir, "original.txt"), "modified");

            // Restore
            var restoreResult = await sut.RestoreAsync(backupResult.Value);

            restoreResult.IsSuccess.Should().BeTrue();
            var content = await File.ReadAllTextAsync(Path.Combine(appDir, "original.txt"));
            content.Should().Be("original");
        }
        finally
        {
            Directory.Delete(appDir, true);
            Directory.Delete(backupDir, true);
        }
    }

    // ── ListBackups ──────────────────────────────────────────────────────────

    [Fact]
    public void ListBackups_NoBackups_ReturnsEmptyList()
    {
        var backupDir = CreateTempDir();
        try
        {
            var sut = new BackupService("C:/app", backupDir);
            var result = sut.ListBackups();

            result.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(backupDir, true);
        }
    }

    [Fact]
    public async Task ListBackups_WithBackups_ReturnsSortedList()
    {
        var appDir = CreateTempDir();
        var backupDir = CreateTempDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(appDir, "test.txt"), "data");
            var sut = new BackupService(appDir, backupDir);

            await sut.CreateBackupAsync();
            await Task.Delay(1100); // Ensure different timestamps
            await sut.CreateBackupAsync();

            var result = sut.ListBackups();

            result.Should().HaveCount(2);
            result[0].Should().Contain("backup_"); // Newest first
        }
        finally
        {
            Directory.Delete(appDir, true);
            Directory.Delete(backupDir, true);
        }
    }

    [Fact]
    public void ListBackups_NonExistentBackupDir_ReturnsEmptyList()
    {
        var sut = new BackupService("C:/app", "C:/nonexistent_backup_dir_xyz");
        var result = sut.ListBackups();

        result.Should().BeEmpty();
    }
}
