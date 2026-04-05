using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="BackupManager"/> backup and restore operations.
/// Uses temporary directories to avoid polluting the filesystem.
/// </summary>
public sealed class BackupManagerTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _appDir;
    private readonly string _backupDir;

    public BackupManagerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueBackupTests_{Guid.NewGuid():N}");
        _appDir = Path.Combine(_tempDir, "app");
        _backupDir = Path.Combine(_tempDir, "backup");
        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_backupDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private UpdateOptions BuildOptions() => new()
    {
        ApplicationDirectory = _appDir,
        BackupDirectory = _backupDir
    };

    // ── CreateBackupAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBackupAsync_CreatesBackupDirectory()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "fake binary");
        var sut = new BackupManager(BuildOptions());

        // Act
        Result<string> result = await sut.CreateBackupAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        Directory.Exists(result.Value).Should().BeTrue("backup directory must be created");
        result.Value.Should().StartWith(_backupDir);
    }

    [Fact]
    public async Task CreateBackupAsync_CopiesFilesToBackup()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "binary content");
        File.WriteAllText(Path.Combine(_appDir, "config.json"), "{\"key\":\"value\"}");
        var sut = new BackupManager(BuildOptions());

        // Act
        Result<string> result = await sut.CreateBackupAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(Path.Combine(result.Value, "app.exe")).Should().BeTrue();
        File.Exists(Path.Combine(result.Value, "config.json")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateBackupAsync_BackupDirectoryNameContainsTimestamp()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "dummy.dll"), "dll content");
        var sut = new BackupManager(BuildOptions());

        // Act
        Result<string> result = await sut.CreateBackupAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        Path.GetFileName(result.Value).Should().StartWith("backup_",
            "backup directory must be named backup_{timestamp}");
    }

    [Fact]
    public async Task CreateBackupAsync_EmptyAppDir_CreatesEmptyBackup()
    {
        // Arrange
        var sut = new BackupManager(BuildOptions());

        // Act
        Result<string> result = await sut.CreateBackupAsync();

        // Assert
        result.IsSuccess.Should().BeTrue("empty source directory is a valid backup scenario");
        Directory.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task CreateBackupAsync_Cancelled_ReturnsFailure()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "large.bin"), new string('x', 1000));
        var sut = new BackupManager(BuildOptions());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        Result<string> result = await sut.CreateBackupAsync(cts.Token);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // ── RestoreFromBackupAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task RestoreFromBackupAsync_RestoresFiles()
    {
        // Arrange: create backup manually
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "v1 binary");
        var sut = new BackupManager(BuildOptions());
        Result<string> backup = await sut.CreateBackupAsync();
        backup.IsSuccess.Should().BeTrue();

        // Simulate a failed update that corrupts the app dir
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "CORRUPT");

        // Act
        Result restoreResult = await sut.RestoreFromBackupAsync();

        // Assert
        restoreResult.IsSuccess.Should().BeTrue();
        File.ReadAllText(Path.Combine(_appDir, "app.exe")).Should().Be("v1 binary");
    }

    [Fact]
    public async Task RestoreFromBackupAsync_NoBackup_ReturnsRollbackFailed()
    {
        // Arrange: no backup created
        var sut = new BackupManager(BuildOptions());

        // Act
        Result result = await sut.RestoreFromBackupAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
    }

    [Fact]
    public async Task RestoreFromBackupAsync_SelectsMostRecentBackup()
    {
        // Arrange: create two backups with different content
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "v1 binary");
        var sut = new BackupManager(BuildOptions());

        Result<string> firstBackup = await sut.CreateBackupAsync();
        firstBackup.IsSuccess.Should().BeTrue();

        // Wait 1 second to ensure different timestamp
        await Task.Delay(TimeSpan.FromSeconds(1.1));

        // Overwrite with v2
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "v2 binary");
        Result<string> secondBackup = await sut.CreateBackupAsync();
        secondBackup.IsSuccess.Should().BeTrue();

        // Simulate corruption
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "CORRUPT");

        // Act
        Result restoreResult = await sut.RestoreFromBackupAsync();

        // Assert
        restoreResult.IsSuccess.Should().BeTrue();
        // Should restore from most recent backup (v2)
        File.ReadAllText(Path.Combine(_appDir, "app.exe")).Should().Be("v2 binary",
            "restore must use the most recent backup");
    }

    // ── GetLatestBackupPath ────────────────────────────────────────────────────

    [Fact]
    public void GetLatestBackupPath_NoBackups_ReturnsNull()
    {
        // Arrange
        var sut = new BackupManager(BuildOptions());

        // Act
        string? result = sut.GetLatestBackupPath();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestBackupPath_HasBackup_ReturnsPath()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "content");
        var sut = new BackupManager(BuildOptions());
        await sut.CreateBackupAsync();

        // Act
        string? result = sut.GetLatestBackupPath();

        // Assert
        result.Should().NotBeNull();
        Directory.Exists(result).Should().BeTrue();
    }

    [Fact]
    public void GetLatestBackupPath_BackupDirDoesNotExist_ReturnsNull()
    {
        // Arrange: point to a non-existent backup directory
        var options = new UpdateOptions
        {
            ApplicationDirectory = _appDir,
            BackupDirectory = Path.Combine(_tempDir, "nonexistent_backup")
        };
        var sut = new BackupManager(options);

        // Act
        string? result = sut.GetLatestBackupPath();

        // Assert
        result.Should().BeNull();
    }
}
