using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

[Trait("SWR", "SWR-UPD-020")]
public sealed class BackupServiceTests : IDisposable
{
    private readonly string _appDir;
    private readonly string _backupDir;
    private readonly BackupService _sut;

    public BackupServiceTests()
    {
        var root = Path.Combine(Path.GetTempPath(), $"BackupTest_{Guid.NewGuid()}");
        _appDir = Path.Combine(root, "app");
        _backupDir = Path.Combine(root, "backups");

        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_backupDir);

        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "fake binary");
        File.WriteAllText(Path.Combine(_appDir, "config.json"), "{}");

        _sut = new BackupService(_appDir, _backupDir);
    }

    public void Dispose()
    {
        var root = Path.GetDirectoryName(_appDir)!;
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullAppDir_ThrowsArgumentNullException()
    {
        var act = () => new BackupService(null!, _backupDir);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullBackupDir_ThrowsArgumentNullException()
    {
        var act = () => new BackupService(_appDir, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── CreateBackupAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBackup_AppDirExists_CreatesBackupAndReturnsPath()
    {
        var result = await _sut.CreateBackupAsync();

        result.IsSuccess.Should().BeTrue();
        Directory.Exists(result.Value).Should().BeTrue();
        File.Exists(Path.Combine(result.Value, "app.exe")).Should().BeTrue();
    }

    [Fact]
    public async Task CreateBackup_NonExistentAppDir_ReturnsValidationFailure()
    {
        var sut = new BackupService("C:/nonexistent/app", _backupDir);

        var result = await sut.CreateBackupAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── RestoreAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Restore_ValidBackup_RestoresFiles()
    {
        var backupResult = await _sut.CreateBackupAsync();
        backupResult.IsSuccess.Should().BeTrue();

        // Delete a file from app dir to simulate update corruption
        File.Delete(Path.Combine(_appDir, "config.json"));

        var restoreResult = await _sut.RestoreAsync(backupResult.Value);

        restoreResult.IsSuccess.Should().BeTrue();
        File.Exists(Path.Combine(_appDir, "config.json")).Should().BeTrue();
    }

    [Fact]
    public async Task Restore_NonExistentBackup_ReturnsNotFound()
    {
        var result = await _sut.RestoreAsync("C:/nonexistent/backup");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task Restore_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RestoreAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── ListBackups ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ListBackups_AfterTwoBackups_ReturnsTwoInDescendingOrder()
    {
        await _sut.CreateBackupAsync();
        await Task.Delay(1100); // ensure different timestamps
        await _sut.CreateBackupAsync();

        var backups = _sut.ListBackups();

        backups.Should().HaveCount(2);
        // Newest first (descending sort)
        string.Compare(backups[0], backups[1], StringComparison.Ordinal).Should().BeGreaterThan(0);
    }

    [Fact]
    public void ListBackups_NoBackups_ReturnsEmpty()
    {
        var emptyDir = Path.Combine(Path.GetTempPath(), $"NoBackups_{Guid.NewGuid()}");
        var sut = new BackupService(_appDir, emptyDir);

        var backups = sut.ListBackups();

        backups.Should().BeEmpty();
    }

    // ── CreateBackupAsync — cancellation ─────────────────────────────────────

    [Fact]
    public async Task CreateBackup_CancelledToken_ThrowsOperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.CreateBackupAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── RestoreAsync — cancellation ───────────────────────────────────────────

    [Fact]
    public async Task Restore_CancelledToken_ThrowsOperationCanceledException()
    {
        var backupResult = await _sut.CreateBackupAsync();
        backupResult.IsSuccess.Should().BeTrue();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.RestoreAsync(backupResult.Value, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── CreateBackupAsync — subdirectory recursion ────────────────────────────

    [Fact]
    public async Task CreateBackup_WithSubdirectories_CopiesNestedFilesRecursively()
    {
        // Create a nested subdirectory in the app directory
        var subDir = Path.Combine(_appDir, "modules");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "plugin.dll"), "nested binary");

        var result = await _sut.CreateBackupAsync();

        result.IsSuccess.Should().BeTrue();
        var nestedBackupFile = Path.Combine(result.Value, "modules", "plugin.dll");
        File.Exists(nestedBackupFile).Should().BeTrue();
    }
}
