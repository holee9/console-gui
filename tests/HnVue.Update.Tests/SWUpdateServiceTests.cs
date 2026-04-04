using System.IO;
using System.Security.Cryptography;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Update;
using NSubstitute;
using Xunit;

namespace HnVue.Update.Tests;

[Trait("SWR", "SWR-UPD-030")]
public sealed class SWUpdateServiceTests : IDisposable
{
    private readonly IUpdateRepository _repository;
    private readonly BackupService _backupService;
    private readonly SWUpdateService _sut;
    private readonly string _tempRoot;
    private readonly string _appDir;
    private readonly string _backupDir;

    public SWUpdateServiceTests()
    {
        _repository = Substitute.For<IUpdateRepository>();

        _tempRoot = Path.Combine(Path.GetTempPath(), $"SWUpdateTest_{Guid.NewGuid()}");
        _appDir = Path.Combine(_tempRoot, "app");
        _backupDir = Path.Combine(_tempRoot, "backups");
        Directory.CreateDirectory(_appDir);
        Directory.CreateDirectory(_backupDir);
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "fake");

        _backupService = new BackupService(_appDir, _backupDir);
        _sut = new SWUpdateService(_repository, _backupService);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot)) Directory.Delete(_tempRoot, recursive: true);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new SWUpdateService(null!, _backupService);

        act.Should().Throw<ArgumentNullException>().WithParameterName("updateRepository");
    }

    [Fact]
    public void Constructor_NullBackupService_ThrowsArgumentNullException()
    {
        var act = () => new SWUpdateService(_repository, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("backupService");
    }

    // ── CheckUpdateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CheckUpdate_UpdateAvailable_ReturnsUpdateInfo()
    {
        var info = new UpdateInfo("2.0.0", "Fixes", "http://example.com/pkg.zip", new string('A', 64));
        _repository.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<UpdateInfo?>(info));

        var result = await _sut.CheckUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("2.0.0");
    }

    [Fact]
    public async Task CheckUpdate_AlreadyUpToDate_ReturnsSuccessWithNull()
    {
        _repository.CheckForUpdateAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<UpdateInfo?>(null));

        var result = await _sut.CheckUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── ApplyUpdateAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyUpdate_ValidPackage_SucceedsAfterVerification()
    {
        // Create a real package file with known hash
        var pkgPath = Path.Combine(_tempRoot, "update.zip");
        var content = "update package content"u8.ToArray();
        await File.WriteAllBytesAsync(pkgPath, content);
        var hash = Convert.ToHexString(SHA256.HashData(content));

        var info = new UpdateInfo("2.0.0", null, "url", hash);
        _repository.GetPackageInfoAsync(pkgPath, Arg.Any<CancellationToken>())
            .Returns(Result.Success(info));
        _repository.ApplyPackageAsync(pkgPath, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.ApplyUpdateAsync(pkgPath);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).ApplyPackageAsync(pkgPath, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ApplyUpdate_WrongHash_ReturnsSignatureVerificationFailed()
    {
        var pkgPath = Path.Combine(_tempRoot, "update_bad.zip");
        await File.WriteAllTextAsync(pkgPath, "content");

        var info = new UpdateInfo("2.0.0", null, "url", new string('F', 64));
        _repository.GetPackageInfoAsync(pkgPath, Arg.Any<CancellationToken>())
            .Returns(Result.Success(info));

        var result = await _sut.ApplyUpdateAsync(pkgPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.SignatureVerificationFailed);
    }

    [Fact]
    public async Task ApplyUpdate_NonExistentFile_ReturnsUpdatePackageCorrupt()
    {
        var result = await _sut.ApplyUpdateAsync("C:/nonexistent/update.zip");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
    }

    [Fact]
    public async Task ApplyUpdate_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ApplyUpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── RollbackAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Rollback_WithBackupAvailable_RestoresFromLatestBackup()
    {
        // Create a backup first
        await _backupService.CreateBackupAsync();

        var result = await _sut.RollbackAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Rollback_WithNoBackups_ReturnsRollbackFailed()
    {
        var emptyBackupDir = Path.Combine(_tempRoot, "empty_backups");
        var sut = new SWUpdateService(
            _repository,
            new BackupService(_appDir, emptyBackupDir));

        var result = await sut.RollbackAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
    }
}
