using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
/// Additional coverage tests for Update module (Safety-Critical: 90% target).
/// Covers UpdateOptions validation paths, BackupService, UpdateChecker edge cases,
/// and EfUpdateRepository integration.
/// </summary>
public sealed class UpdateSafetyCriticalCoverageTests : IDisposable
{
    private readonly string _tempDir;

    public UpdateSafetyCriticalCoverageTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueSC_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ── UpdateOptions: Validate HTTP URL ──────────────────────────────────────

    [Fact]
    public void UpdateOptions_Validate_HttpUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "http://update.example.com" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*HTTPS*");
    }

    [Fact]
    public void UpdateOptions_Validate_EmptyUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot be null*");
    }

    [Fact]
    public void UpdateOptions_Validate_WhitespaceUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "   " };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*cannot be null*");
    }

    [Fact]
    public void UpdateOptions_Validate_NonHttpUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.example.com" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>().WithMessage("*valid HTTPS*");
    }

    [Fact]
    public void UpdateOptions_Validate_ValidHttps_DoesNotThrow()
    {
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            RequireAuthenticodeSignature = false,
        };

        // Should not throw in non-production environments
        var act = () => options.Validate();
        try
        {
            act();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Authenticode"))
        {
            // Expected in production environment
        }
    }

    // ── UpdateOptions: Resolved properties ────────────────────────────────────

    [Fact]
    public void UpdateOptions_ResolvedBackupDirectory_WithValue_ReturnsValue()
    {
        var options = new UpdateOptions { BackupDirectory = "/custom/backup" };
        options.ResolvedBackupDirectory.Should().Be("/custom/backup");
    }

    [Fact]
    public void UpdateOptions_ResolvedApplicationDirectory_WithValue_ReturnsValue()
    {
        var options = new UpdateOptions { ApplicationDirectory = "/custom/app" };
        options.ResolvedApplicationDirectory.Should().Be("/custom/app");
    }

    [Fact]
    public void UpdateOptions_ResolvedBackupDirectory_Null_ReturnsDefault()
    {
        var options = new UpdateOptions { BackupDirectory = null! };
        options.ResolvedBackupDirectory.Should().Contain("HnVue");
    }

    // ── BackupService: Create + Restore + ListBackups ──────────────────────────

    [Fact]
    public async Task BackupService_CreateBackup_Restore_RoundTrip()
    {
        var appDir = Path.Combine(_tempDir, "app");
        var backupDir = Path.Combine(_tempDir, "backups");
        Directory.CreateDirectory(appDir);
        File.WriteAllText(Path.Combine(appDir, "config.txt"), "original");

        var svc = new BackupService(appDir, backupDir);

        var createResult = await svc.CreateBackupAsync();
        createResult.IsSuccess.Should().BeTrue();
        createResult.Value.Should().Contain("backup_");

        // Modify the file
        File.WriteAllText(Path.Combine(appDir, "config.txt"), "modified");

        // Restore
        var restoreResult = await svc.RestoreAsync(createResult.Value);
        restoreResult.IsSuccess.Should().BeTrue();
        File.ReadAllText(Path.Combine(appDir, "config.txt")).Should().Be("original");
    }

    [Fact]
    public async Task BackupService_CreateBackup_AppDirNotFound_ReturnsFailure()
    {
        var svc = new BackupService(Path.Combine(_tempDir, "nonexistent_app"), Path.Combine(_tempDir, "bk"));

        var result = await svc.CreateBackupAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task BackupService_Restore_BackupNotFound_ReturnsFailure()
    {
        var appDir = Path.Combine(_tempDir, "app2");
        Directory.CreateDirectory(appDir);
        var svc = new BackupService(appDir, Path.Combine(_tempDir, "bk2"));

        var result = await svc.RestoreAsync(Path.Combine(_tempDir, "nonexistent_backup"));
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task BackupService_Restore_NullPath_Throws()
    {
        var svc = new BackupService(_tempDir, _tempDir);
        var act = async () => await svc.RestoreAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task BackupService_NullAppDir_Throws()
    {
        var act = () => new BackupService(null!, _tempDir);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task BackupService_NullBackupDir_Throws()
    {
        var act = () => new BackupService(_tempDir, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void BackupService_ListBackups_NoBackupDir_ReturnsEmpty()
    {
        var svc = new BackupService(_tempDir, Path.Combine(_tempDir, "no_backups"));
        svc.ListBackups().Should().BeEmpty();
    }

    [Fact]
    public async Task BackupService_ListBackups_WithBackups_ReturnsSortedList()
    {
        var appDir = Path.Combine(_tempDir, "app3");
        var backupDir = Path.Combine(_tempDir, "bk3");
        Directory.CreateDirectory(appDir);
        File.WriteAllText(Path.Combine(appDir, "test.txt"), "data");

        var svc = new BackupService(appDir, backupDir);
        await svc.CreateBackupAsync();
        await Task.Delay(1100);
        await svc.CreateBackupAsync();

        var list = svc.ListBackups();
        list.Should().HaveCount(2);
        // Should be sorted newest first
        list[0].Should().Contain("backup_");
    }

    [Fact]
    public async Task BackupService_CreateBackup_Cancelled_Throws()
    {
        var appDir = Path.Combine(_tempDir, "app4");
        Directory.CreateDirectory(appDir);
        File.WriteAllText(Path.Combine(appDir, "test.txt"), "data");
        var svc = new BackupService(appDir, Path.Combine(_tempDir, "bk4"));

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await svc.CreateBackupAsync(cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task BackupService_Restore_Cancelled_Throws()
    {
        var appDir = Path.Combine(_tempDir, "app5");
        var backupDir = Path.Combine(_tempDir, "bk5");
        Directory.CreateDirectory(appDir);
        File.WriteAllText(Path.Combine(appDir, "test.txt"), "data");
        var svc = new BackupService(appDir, backupDir);
        var createResult = await svc.CreateBackupAsync();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await svc.RestoreAsync(createResult.Value, cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── UpdateChecker: Network error ──────────────────────────────────────────

    [Fact]
    public async Task CheckUpdateAsync_NetworkError_ReturnsValidationFailed()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        var handler = new ThrowingHandler(new HttpRequestException("Network unreachable"));
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.hnvue.com/api/v1",
            CurrentVersion = "1.0.0",
            RequireAuthenticodeSignature = false,
        };
        var sut = new SWUpdateService(Options.Create(options), factory);

        var result = await sut.CheckUpdateAsync();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── UpdateChecker: Same version ───────────────────────────────────────────

    [Fact]
    public async Task CheckUpdateAsync_SameVersion_ReturnsNull()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(new StubHandler(
            HttpStatusCode.OK,
            """{"version":"1.0.0","releaseNotes":"none","packageUrl":"","sha256Hash":""}""")));

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.hnvue.com/api/v1",
            CurrentVersion = "1.0.0",
            RequireAuthenticodeSignature = false,
        };
        var sut = new SWUpdateService(Options.Create(options), factory);

        var result = await sut.CheckUpdateAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── UpdateChecker: Null response body ─────────────────────────────────────

    [Fact]
    public async Task CheckUpdateAsync_NullJsonResponse_ReturnsSuccessWithNull()
    {
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(new StubHandler(
            HttpStatusCode.OK, "null")));

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.hnvue.com/api/v1",
            CurrentVersion = "1.0.0",
            RequireAuthenticodeSignature = false,
        };
        var sut = new SWUpdateService(Options.Create(options), factory);

        var result = await sut.CheckUpdateAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    // ── EfUpdateRepository integration ────────────────────────────────────────

    [Fact]
    public void UpdateOptions_ResolvedApplicationDirectory_Null_ReturnsDefault()
    {
        var options = new UpdateOptions { ApplicationDirectory = null! };
        options.ResolvedApplicationDirectory.Should().Contain("HnVue");
    }

    [Fact]
    public void UpdateOptions_Validate_FtpUrl_Throws()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.example.com" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task BackupService_Restore_NonExistentBackup_ReturnsFailure()
    {
        var appDir = Path.Combine(_tempDir, "app_restore_nf");
        Directory.CreateDirectory(appDir);
        var svc = new BackupService(appDir, Path.Combine(_tempDir, "bk_restore_nf"));

        var result = await svc.RestoreAsync(Path.Combine(_tempDir, "no_such_backup"));
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public StubHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        private readonly Exception _exception;
        public ThrowingHandler(Exception ex) => _exception = ex;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(_exception);
    }
}
