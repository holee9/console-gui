using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Update;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// S13-R2 coverage gap tests for HnVue.Update module (Safety-Critical: 90%+ target).
/// </summary>
public sealed class UpdateS13R2CoverageTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _appDir;
    private readonly string _backupDir;
    private readonly IAuditService _auditService;

    public UpdateS13R2CoverageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueS13R2_{Guid.NewGuid():N}");
        _appDir = Path.Combine(_tempDir, "app");
        _backupDir = Path.Combine(_tempDir, "backup");
        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_backupDir);

        _auditService = Substitute.For<IAuditService>();
        _auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private UpdateOptions BuildOptions(bool requireSignature = false) => new()
    {
        UpdateServerUrl = "https://update.hnvue.com/api/v1",
        CurrentVersion = "1.0.0",
        ApplicationDirectory = _appDir,
        BackupDirectory = _backupDir,
        RequireAuthenticodeSignature = requireSignature
    };

    private SWUpdateService BuildService(UpdateOptions? options = null)
    {
        options ??= BuildOptions();
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());
        return new SWUpdateService(Options.Create(options), factory, _auditService);
    }

    private string CreateFakePackage(string content = "fake package bytes")
    {
        string packagePath = Path.Combine(_tempDir, "update.zip");
        File.WriteAllText(packagePath, content);
        return packagePath;
    }

    private string CreateZipPackage(params (string name, string content)[] entries)
    {
        string packagePath = Path.Combine(_tempDir, $"pkg_{Guid.NewGuid():N}.zip");
        using var stream = File.Create(packagePath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            var entry = archive.CreateEntry(name);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }
        return packagePath;
    }

    private static string ComputeSha256Hex(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    // ── SWUpdateService State Transitions ─────────────────────────────────────

    [Fact]
    public async Task ApplyUpdateAsync_HashMismatch_TransitionsToRolledBack()
    {
        var packagePath = CreateFakePackage("original");
        var wrongHash = "0000000000000000000000000000000000000000000000000000000000000000";
        File.WriteAllText(packagePath + ".sha256", wrongHash);

        var sut = BuildService();

        var result = await sut.ApplyUpdateAsync(packagePath);

        result.IsFailure.Should().BeTrue();
        sut.CurrentState.Should().Be(UpdateState.RolledBack);
    }

    [Fact]
    public async Task ApplyUpdateAsync_ValidSidecarHashFormat_Succeeds()
    {
        var packagePath = CreateFakePackage("valid content");
        var hash = ComputeSha256Hex(packagePath);
        // sha256sum format: "hash  filename"
        File.WriteAllText(packagePath + ".sha256", $"{hash}  update.zip");

        var sut = BuildService();

        var result = await sut.ApplyUpdateAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(UpdateState.Completed);
    }

    [Fact]
    public async Task ApplyUpdateAsync_SidecarHashTabDelimited_Succeeds()
    {
        var packagePath = CreateFakePackage("tab content");
        var hash = ComputeSha256Hex(packagePath);
        // Alternative sha256sum format: "hash\tfilename"
        File.WriteAllText(packagePath + ".sha256", $"{hash}\tupdate.zip\n");

        var sut = BuildService();

        var result = await sut.ApplyUpdateAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyUpdateAsync_NoSidecar_SkipsHashVerification()
    {
        var packagePath = CreateFakePackage("no sidecar");

        var sut = BuildService();

        var result = await sut.ApplyUpdateAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        sut.CurrentState.Should().Be(UpdateState.Completed);
    }

    [Fact]
    public async Task ApplyUpdateAsync_EmptySidecar_HashMismatchFails()
    {
        // Empty sidecar produces empty hash → VerifyHash returns false (whitespace)
        var packagePath = CreateFakePackage("empty sidecar");
        File.WriteAllText(packagePath + ".sha256", "");

        var sut = BuildService();

        var result = await sut.ApplyUpdateAsync(packagePath);

        result.IsFailure.Should().BeTrue();
        sut.CurrentState.Should().Be(UpdateState.RolledBack);
    }

    [Fact]
    public async Task ApplyUpdateAsync_StagesPendingUpdateMarker()
    {
        var packagePath = CreateFakePackage("marker test");

        var sut = BuildService();
        await sut.ApplyUpdateAsync(packagePath);

        var markerPath = Path.Combine(_backupDir, "pending_update.txt");
        File.Exists(markerPath).Should().BeTrue();
        (await File.ReadAllTextAsync(markerPath)).Should().Be(packagePath);
    }

    [Fact]
    public async Task ApplyUpdateAsync_WritesAuditOnSuccess()
    {
        var packagePath = CreateFakePackage("audit test");

        var sut = BuildService();
        await sut.ApplyUpdateAsync(packagePath);

        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e => e.Action == "UPDATE_STAGED"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RollbackAsync_NoBackup_ReturnsFailure()
    {
        var sut = BuildService();

        var result = await sut.RollbackAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
    }

    [Fact]
    public async Task RollbackAsync_WithBackup_Succeeds()
    {
        // First apply an update to create a backup
        var packagePath = CreateFakePackage("rollback test");
        var sut = BuildService();
        await sut.ApplyUpdateAsync(packagePath);

        // Now rollback
        var result = await sut.RollbackAsync();

        result.IsSuccess.Should().BeTrue();
    }

    // ── UpdateRepository Tests ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateRepository_ApplyPackageAsync_CorruptZip_ReturnsFailure()
    {
        var baseDir = Path.Combine(_tempDir, "repo_corrupt");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = Path.Combine(baseDir, "HnVue-2.0.0.zip");
        File.WriteAllText(packagePath, "not a zip file");

        var result = await sut.ApplyPackageAsync(packagePath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
    }

    [Fact]
    public async Task UpdateRepository_ApplyPackageAsync_ValidZip_StagesSuccessfully()
    {
        var baseDir = Path.Combine(_tempDir, "repo_valid");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = CreateZipPackage(
            ("app.exe", "binary content"),
            ("config.json", "{\"key\":\"value\"}"));

        var result = await sut.ApplyPackageAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        Directory.Exists(Path.Combine(baseDir, "Updates", "Staging")).Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRepository_ApplyPackageAsync_HashSidecarMismatch_ReturnsFailure()
    {
        var baseDir = Path.Combine(_tempDir, "repo_hash");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = CreateZipPackage(("file.txt", "content"));
        File.WriteAllText(packagePath + ".sha256", "0000000000000000000000000000000000000000000000000000000000000000");

        var result = await sut.ApplyPackageAsync(packagePath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
    }

    [Fact]
    public async Task UpdateRepository_ApplyPackageAsync_ValidHashSidecar_Succeeds()
    {
        var baseDir = Path.Combine(_tempDir, "repo_hashok");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = CreateZipPackage(("file.txt", "content"));
        var hash = ComputeSha256Hex(packagePath);
        File.WriteAllText(packagePath + ".sha256", hash);

        var result = await sut.ApplyPackageAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateRepository_ApplyPackageAsync_NonExistentFile_ReturnsNotFound()
    {
        var baseDir = Path.Combine(_tempDir, "repo_missing");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var result = await sut.ApplyPackageAsync(Path.Combine(_tempDir, "nonexistent.zip"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task UpdateRepository_CheckForUpdateAsync_MultipleVersions_ReturnsNewest()
    {
        var baseDir = Path.Combine(_tempDir, "repo_multi");
        var updatesDir = Path.Combine(baseDir, "Updates");
        Directory.CreateDirectory(updatesDir);
        var sut = new UpdateRepository(baseDir);

        // Create multiple version packages
        File.WriteAllText(Path.Combine(updatesDir, "HnVue-1.0.0.zip"), "old");
        File.WriteAllText(Path.Combine(updatesDir, "HnVue-3.0.0.zip"), "newest");
        File.WriteAllText(Path.Combine(updatesDir, "HnVue-2.0.0.zip"), "mid");

        var result = await sut.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("3.0.0");
    }

    [Fact]
    public async Task UpdateRepository_CheckForUpdateAsync_NoUpdatesDir_ReturnsNull()
    {
        var baseDir = Path.Combine(_tempDir, "repo_empty");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var result = await sut.CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task UpdateRepository_GetPackageInfoAsync_WithMetadataJson_ReturnsFullInfo()
    {
        var baseDir = Path.Combine(_tempDir, "repo_meta");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = Path.Combine(baseDir, "HnVue-2.1.0.zip");
        File.WriteAllText(packagePath, "pkg");
        var metaPath = Path.ChangeExtension(packagePath, ".json");
        File.WriteAllText(metaPath, """{"Version":"2.1.0","ReleaseNotes":"Bug fixes","PackageUrl":"test","Sha256Hash":"abc"}""");

        var result = await sut.GetPackageInfoAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("2.1.0");
        result.Value.ReleaseNotes.Should().Be("Bug fixes");
    }

    [Fact]
    public async Task UpdateRepository_GetPackageInfoAsync_NoMetadata_FallsBackToFilename()
    {
        var baseDir = Path.Combine(_tempDir, "repo_nometa");
        Directory.CreateDirectory(baseDir);
        var sut = new UpdateRepository(baseDir);

        var packagePath = Path.Combine(baseDir, "HnVue-2.5.0.zip");
        File.WriteAllText(packagePath, "pkg");

        var result = await sut.GetPackageInfoAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("2.5.0");
        result.Value.ReleaseNotes.Should().BeNull();
    }

    // ── BackupService Tests ───────────────────────────────────────────────────

    [Fact]
    public void BackupService_ListBackups_EmptyDirectory_ReturnsEmpty()
    {
        var emptyBackupDir = Path.Combine(_tempDir, "empty_backups");
        Directory.CreateDirectory(emptyBackupDir);
        var sut = new BackupService(_appDir, emptyBackupDir);

        var result = sut.ListBackups();

        result.Should().BeEmpty();
    }

    [Fact]
    public void BackupService_ListBackups_NonExistentDirectory_ReturnsEmpty()
    {
        var sut = new BackupService(_appDir, Path.Combine(_tempDir, "no_such_dir"));

        var result = sut.ListBackups();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task BackupService_ListBackups_MultipleBackups_ReturnsNewestFirst()
    {
        var backupDir = Path.Combine(_tempDir, "multi_backups");
        Directory.CreateDirectory(backupDir);
        // Create backup directories with timestamps
        Directory.CreateDirectory(Path.Combine(backupDir, "backup_20260101_100000"));
        Directory.CreateDirectory(Path.Combine(backupDir, "backup_20260102_120000"));
        Directory.CreateDirectory(Path.Combine(backupDir, "backup_20260101_080000"));

        var sut = new BackupService(_appDir, backupDir);
        var result = sut.ListBackups();

        result.Should().HaveCount(3);
        result[0].Should().Contain("backup_20260102_120000");
    }

    [Fact]
    public async Task BackupService_RestoreAsync_NonExistentBackup_ReturnsNotFound()
    {
        var sut = new BackupService(_appDir, _backupDir);

        var result = await sut.RestoreAsync(Path.Combine(_tempDir, "no_backup"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BackupService_CreateBackupAsync_NonExistentAppDir_ReturnsFailure()
    {
        var sut = new BackupService(Path.Combine(_tempDir, "no_app"), _backupDir);

        var result = await sut.CreateBackupAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BackupService_RestoreAsync_ValidBackup_RestoresFiles()
    {
        // Create source files
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "binary");
        Directory.CreateDirectory(Path.Combine(_appDir, "sub"));
        File.WriteAllText(Path.Combine(_appDir, "sub", "config.json"), "{}");

        var backupDir = Path.Combine(_tempDir, "restore_backups");
        Directory.CreateDirectory(backupDir);
        var sut = new BackupService(_appDir, backupDir);

        var backupResult = await sut.CreateBackupAsync();
        backupResult.IsSuccess.Should().BeTrue();

        // Modify app dir
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "modified");

        // Restore
        var restoreResult = await sut.RestoreAsync(backupResult.Value);
        restoreResult.IsSuccess.Should().BeTrue();
        (await File.ReadAllTextAsync(Path.Combine(_appDir, "app.exe"))).Should().Be("binary");
    }

    // ── BackupManager Tests ───────────────────────────────────────────────────

    [Fact]
    public void BackupManager_GetLatestBackupPath_NoBackups_ReturnsNull()
    {
        var options = BuildOptions();
        var sut = new BackupManager(options);

        var result = sut.GetLatestBackupPath();

        result.Should().BeNull();
    }

    [Fact]
    public async Task BackupManager_CreateBackupAsync_EmptyAppDir_Succeeds()
    {
        var emptyAppDir = Path.Combine(_tempDir, "empty_app");
        Directory.CreateDirectory(emptyAppDir);
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.hnvue.com",
            ApplicationDirectory = emptyAppDir,
            BackupDirectory = _backupDir,
        };
        var sut = new BackupManager(options);

        var result = await sut.CreateBackupAsync();

        result.IsSuccess.Should().BeTrue();
        Directory.Exists(result.Value).Should().BeTrue();
    }

    [Fact]
    public async Task BackupManager_CreateAndRestore_RoundTrip()
    {
        // Create files in app dir
        File.WriteAllText(Path.Combine(_appDir, "test.txt"), "hello");

        var options = BuildOptions();
        var sut = new BackupManager(options);

        var backupResult = await sut.CreateBackupAsync();
        backupResult.IsSuccess.Should().BeTrue();

        // Verify backup contains the file
        var backedUpFile = Path.Combine(backupResult.Value, "test.txt");
        File.Exists(backedUpFile).Should().BeTrue();
        (await File.ReadAllTextAsync(backedUpFile)).Should().Be("hello");

        // Restore
        var restoreResult = await sut.RestoreFromBackupAsync();
        restoreResult.IsSuccess.Should().BeTrue();
    }

    // ── UpdateOptions Validation ──────────────────────────────────────────────

    [Fact]
    public void UpdateOptions_Validate_HttpUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "http://update.hnvue.com/api" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*HTTPS*");
    }

    [Fact]
    public void UpdateOptions_Validate_NonHttpUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.hnvue.com/api" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*valid HTTPS*");
    }

    [Fact]
    public void UpdateOptions_Validate_EmptyUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*cannot be null*");
    }

    [Fact]
    public void UpdateOptions_Validate_WhitespaceUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "   " };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateOptions_ResolvedBackupDirectory_Default_UsesAppData()
    {
        var options = new UpdateOptions();

        var result = options.ResolvedBackupDirectory;

        result.Should().Contain("HnVue").And.Contain("backup");
    }

    [Fact]
    public void UpdateOptions_ResolvedApplicationDirectory_Default_UsesBaseDirectory()
    {
        var options = new UpdateOptions();

        var result = options.ResolvedApplicationDirectory;

        result.Should().Be(AppContext.BaseDirectory);
    }

    [Fact]
    public void UpdateOptions_ResolvedBackupDirectory_CustomValue_UsesCustom()
    {
        var options = new UpdateOptions { BackupDirectory = "/custom/backup" };

        options.ResolvedBackupDirectory.Should().Be("/custom/backup");
    }

    [Fact]
    public void UpdateOptions_ResolvedApplicationDirectory_CustomValue_UsesCustom()
    {
        var options = new UpdateOptions { ApplicationDirectory = "/custom/app" };

        options.ResolvedApplicationDirectory.Should().Be("/custom/app");
    }

    // ── UpdateState Coverage ──────────────────────────────────────────────────

    [Theory]
    [InlineData(UpdateState.InProgress)]
    [InlineData(UpdateState.Staged)]
    [InlineData(UpdateState.Completed)]
    [InlineData(UpdateState.Failed)]
    [InlineData(UpdateState.RolledBack)]
    public void UpdateState_AllValues_AreDefined(UpdateState state)
    {
        ((int)state).Should().BeGreaterThanOrEqualTo(0);
        Enum.IsDefined(state).Should().BeTrue();
    }
}
