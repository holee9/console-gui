using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Additional tests for <see cref="SWUpdateService"/> covering UpdateState transitions,
/// rollback, cancellation, and CurrentState property.
/// REQ-COV-002: Extends Update coverage from 75% towards 85%.
/// </summary>
public sealed class SWUpdateServiceAdditionalTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _appDir;
    private readonly string _backupDir;

    public SWUpdateServiceAdditionalTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueSvcAdditional_{Guid.NewGuid():N}");
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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private UpdateOptions BuildOptions(bool requireSignature = false) => new()
    {
        UpdateServerUrl = "https://update.hnvue.com/api/v1",
        CurrentVersion = "1.0.0",
        ApplicationDirectory = _appDir,
        BackupDirectory = _backupDir,
        RequireAuthenticodeSignature = requireSignature
    };

    private SWUpdateService BuildService(UpdateOptions? options = null, IHttpClientFactory? factory = null)
    {
        options ??= BuildOptions();
        factory ??= BuildHttpClientFactory("1.1.0");
        return new SWUpdateService(Options.Create(options), factory);
    }

    private static IHttpClientFactory BuildHttpClientFactory(string version)
    {
        var factory = Substitute.For<IHttpClientFactory>();
        string json = $$"""
            {
              "version": "{{version}}",
              "releaseNotes": "Bug fixes",
              "packageUrl": "https://cdn.hnvue.com/updates/{{version}}.zip",
              "sha256Hash": "abc123"
            }
            """;
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, json);
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
        return factory;
    }

    private string CreatePackage(string content = "fake package")
    {
        string path = Path.Combine(_tempDir, "update.zip");
        File.WriteAllText(path, content);
        return path;
    }

    private static string ComputeHash(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private void WriteHash(string packagePath, string hash)
        => File.WriteAllText(packagePath + ".sha256", hash);

    // ── CurrentState ──────────────────────────────────────────────────────────

    [Fact]
    public void InitialCurrentState_IsCompleted()
    {
        var sut = BuildService();

        sut.CurrentState.Should().Be(UpdateState.Completed);
    }

    [Fact]
    public async Task ApplyUpdateAsync_Success_CurrentStateIsCompleted()
    {
        string pkg = CreatePackage("valid content");
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "binary");
        var sut = BuildService();

        await sut.ApplyUpdateAsync(pkg);

        sut.CurrentState.Should().Be(UpdateState.Completed);
    }

    [Fact]
    public async Task ApplyUpdateAsync_FailedHash_CurrentStateIsRolledBack()
    {
        string pkg = CreatePackage("some content");
        WriteHash(pkg, new string('0', 64)); // wrong hash
        var sut = BuildService();

        await sut.ApplyUpdateAsync(pkg);

        sut.CurrentState.Should().Be(UpdateState.RolledBack);
    }

    // ── ApplyUpdateAsync – cancellation ──────────────────────────────────────

    [Fact]
    public async Task ApplyUpdateAsync_PreCancelledToken_ReturnsCancelled()
    {
        string pkg = CreatePackage("content");
        // Valid hash so it passes hash check
        string hash = ComputeHash(pkg);
        WriteHash(pkg, hash);
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "binary");
        var sut = BuildService();

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        Result result = await sut.ApplyUpdateAsync(pkg, cts.Token);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.OperationCancelled);
    }

    // ── ApplyUpdateAsync – staging marker ────────────────────────────────────

    [Fact]
    public async Task ApplyUpdateAsync_Success_CreatesPendingUpdateMarker()
    {
        string pkg = CreatePackage("valid content");
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "binary");
        var sut = BuildService();

        await sut.ApplyUpdateAsync(pkg);

        string markerPath = Path.Combine(_backupDir, "pending_update.txt");
        File.Exists(markerPath).Should().BeTrue();
        File.ReadAllText(markerPath).Should().Be(pkg);
    }

    [Fact]
    public async Task ApplyUpdateAsync_FailedHash_CleansPendingMarker()
    {
        string pkg = CreatePackage("content");
        WriteHash(pkg, new string('0', 64)); // wrong hash
        var sut = BuildService();

        await sut.ApplyUpdateAsync(pkg);

        string markerPath = Path.Combine(_backupDir, "pending_update.txt");
        // Marker should have been cleaned up on rollback
        File.Exists(markerPath).Should().BeFalse();
    }

    // ── RollbackAsync – state after rollback ─────────────────────────────────

    [Fact]
    public async Task RollbackAsync_AfterSuccessfulApply_RestoresFiles()
    {
        // Arrange: create original app file
        string originalContent = "original binary v1.0";
        string appFile = Path.Combine(_appDir, "app.exe");
        File.WriteAllText(appFile, originalContent);

        string pkg = CreatePackage("update package");
        var sut = BuildService();

        // Apply update (creates backup)
        await sut.ApplyUpdateAsync(pkg);

        // Corrupt the app file to simulate a bad update
        File.WriteAllText(appFile, "CORRUPTED BINARY");

        // Act: rollback
        Result result = await sut.RollbackAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.ReadAllText(appFile).Should().Be(originalContent);
    }

    [Fact]
    public async Task RollbackAsync_NoBackup_ReturnsRollbackFailed()
    {
        var sut = BuildService();

        Result result = await sut.RollbackAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
        result.ErrorMessage.Should().Contain("no backup");
    }

    // ── UpdateOptions.Validate ────────────────────────────────────────────────

    [Fact]
    public void UpdateOptions_EmptyUrl_ThrowsInvalidOperation()
    {
        var options = new UpdateOptions { UpdateServerUrl = string.Empty };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be null*");
    }

    [Fact]
    public void UpdateOptions_HttpUrl_ThrowsInvalidOperation()
    {
        var options = new UpdateOptions { UpdateServerUrl = "http://update.server.com/api" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*HTTPS*");
    }

    [Fact]
    public void UpdateOptions_ValidHttpsUrl_DoesNotThrow()
    {
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.server.com/api",
            RequireAuthenticodeSignature = true
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void UpdateOptions_NonHttpUrl_ThrowsInvalidOperation()
    {
        var options = new UpdateOptions { UpdateServerUrl = "ftp://update.server.com" };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateOptions_WhitespaceUrl_ThrowsInvalidOperation()
    {
        var options = new UpdateOptions { UpdateServerUrl = "   " };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    // ── UpdateOptions property defaults ──────────────────────────────────────

    [Fact]
    public void UpdateOptions_DefaultCurrentVersion_IsOneZeroZero()
    {
        var options = new UpdateOptions();

        options.CurrentVersion.Should().Be("1.0.0");
    }

    [Fact]
    public void UpdateOptions_DefaultRequireAuthenticode_IsTrue()
    {
        var options = new UpdateOptions();

        options.RequireAuthenticodeSignature.Should().BeTrue();
    }

    [Fact]
    public void UpdateOptions_ResolvedBackupDirectory_ReturnsBackupDir()
    {
        var options = new UpdateOptions { BackupDirectory = _backupDir };

        options.ResolvedBackupDirectory.Should().Be(_backupDir);
    }

    [Fact]
    public void UpdateOptions_EmptyBackupDirectory_ReturnsDefault()
    {
        var options = new UpdateOptions { BackupDirectory = string.Empty };

        // Should return a sensible default (AppData\HnVue\backup)
        options.ResolvedBackupDirectory.Should().Contain("HnVue");
    }

    // ── Stub helpers ───────────────────────────────────────────────────────────

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json")
            });
        }
    }
}
