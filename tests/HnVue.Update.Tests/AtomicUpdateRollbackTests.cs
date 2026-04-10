using System.IO;
using System.Net.Http;
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
/// Tests for atomic update rollback mechanism (T-009).
/// Verifies that staging failures trigger rollback and partial updates are cleaned up.
/// </summary>
public sealed class AtomicUpdateRollbackTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _appDir;
    private readonly string _backupDir;
    private readonly IAuditService _auditService;

    public AtomicUpdateRollbackTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"AtomicRollbackTests_{Guid.NewGuid():N}");
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

    private UpdateOptions BuildOptions() => new()
    {
        UpdateServerUrl = "https://update.hnvue.com/api/v1",
        CurrentVersion = "1.0.0",
        ApplicationDirectory = _appDir,
        BackupDirectory = _backupDir,
        RequireAuthenticodeSignature = false
    };

    private SWUpdateService BuildService(UpdateOptions? options = null)
    {
        options ??= BuildOptions();
        var optionsWrapper = Options.Create(options);

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        var httpClient = new System.Net.Http.HttpClient();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        return new SWUpdateService(optionsWrapper, httpClientFactory, _auditService);
    }

    private string CreateFakePackage(string content = "fake package")
    {
        string packagePath = Path.Combine(_tempDir, "update.zip");
        File.WriteAllText(packagePath, content);
        return packagePath;
    }

    /// <summary>
    /// GREEN Test: When update fails after backup creation,
    /// the system should rollback to the previous state and clean up partial files.
    /// </summary>
    [Fact]
    public async Task ApplyUpdateAsync_StagingFailureAfterBackup_RollsBackAndCleansUp()
    {
        // Arrange
        string packagePath = CreateFakePackage("corrupt content");

        // Write a sidecar hash that doesn't match to trigger failure
        File.WriteAllText(packagePath + ".sha256", "wronghash");

        string originalContent = "original app v1.0";
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), originalContent);

        var service = BuildService();

        // Act: Try to apply update that will fail at hash verification
        Result result = await service.ApplyUpdateAsync(packagePath);

        // Assert: Update should fail
        result.IsFailure.Should().BeTrue("corrupt package should cause update to fail");
        service.CurrentState.Should().Be(UpdateState.RolledBack,
            "failed update should result in RolledBack state");

        // Assert: Application should be in original state (not corrupted)
        string currentContent = File.ReadAllText(Path.Combine(_appDir, "app.exe"));
        currentContent.Should().Be(originalContent, "app should be restored to original state after failed update");

        // Assert: Partial update files should be cleaned up
        string markerPath = Path.Combine(_backupDir, "pending_update.txt");
        File.Exists(markerPath).Should().BeFalse("partial update marker should be cleaned up on rollback");
    }

    /// <summary>
    /// GREEN Test: Update state transitions should follow the correct sequence.
    /// </summary>
    [Fact]
    public async Task ApplyUpdateAsync_StateTransitionsFollowCorrectSequence()
    {
        // Arrange
        string packagePath = CreateFakePackage("valid content");
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "original");
        var service = BuildService();

        // Act: Successful update
        Result result = await service.ApplyUpdateAsync(packagePath);

        // Assert: State should be Completed
        service.CurrentState.Should().Be(UpdateState.Completed,
            "successful update should result in Completed state");
        result.IsSuccess.Should().BeTrue("valid package should succeed");
    }

    /// <summary>
    /// GREEN Test: Partial update cleanup should remove all staged files.
    /// </summary>
    [Fact]
    public async Task CleanupPartialUpdate_RemovesAllStagedFiles()
    {
        // Arrange
        string packagePath = CreateFakePackage("content");
        var service = BuildService();

        // Simulate partial update state
        string stagingDir = Path.Combine(_backupDir, "staging");
        Directory.CreateDirectory(stagingDir);
        File.WriteAllText(Path.Combine(stagingDir, "partial1.dat"), "data1");
        File.WriteAllText(Path.Combine(stagingDir, "partial2.dat"), "data2");

        // Act: Trigger a failed update which calls CleanupPartialUpdate
        // Create a sidecar hash that doesn't match to trigger failure
        File.WriteAllText(packagePath + ".sha256", "wronghash");
        Result result = await service.ApplyUpdateAsync(packagePath);

        // Assert: All staged files should be removed
        Directory.Exists(stagingDir).Should().BeFalse("staging directory should be cleaned up");
    }
}
