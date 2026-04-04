using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
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
/// Integration-level tests for <see cref="SWUpdateService"/>.
/// Uses temp directories for file I/O and mocks for external dependencies.
/// </summary>
public sealed class SWUpdateServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _appDir;
    private readonly string _backupDir;
    private readonly IAuditService _auditService;

    public SWUpdateServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueSvcTests_{Guid.NewGuid():N}");
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

    // ── Helpers ────────────────────────────────────────────────────────────────

    private UpdateOptions BuildOptions(bool requireSignature = false) => new()
    {
        UpdateServerUrl = "https://update.hnvue.com/api/v1",
        CurrentVersion = "1.0.0",
        ApplicationDirectory = _appDir,
        BackupDirectory = _backupDir,
        RequireAuthenticodeSignature = requireSignature
    };

    private SWUpdateService BuildService(
        UpdateOptions? options = null,
        IHttpClientFactory? httpClientFactory = null)
    {
        options ??= BuildOptions();
        var optionsWrapper = Options.Create(options);
        httpClientFactory ??= BuildHttpClientFactory("1.1.0");
        return new SWUpdateService(optionsWrapper, httpClientFactory, _auditService);
    }

    private static IHttpClientFactory BuildHttpClientFactory(
        string serverVersion,
        bool networkError = false,
        bool invalidJson = false)
    {
        var factory = Substitute.For<IHttpClientFactory>();

        HttpMessageHandler handler;
        if (networkError)
        {
            handler = new ThrowingHttpMessageHandler(new HttpRequestException("connection refused"));
        }
        else if (invalidJson)
        {
            handler = new StubHttpMessageHandler(HttpStatusCode.OK, "{ bad json");
        }
        else
        {
            string json = $$"""
                {
                  "version": "{{serverVersion}}",
                  "releaseNotes": "Release notes",
                  "packageUrl": "https://cdn.hnvue.com/updates/{{serverVersion}}.zip",
                  "sha256Hash": "abc123"
                }
                """;
            handler = new StubHttpMessageHandler(HttpStatusCode.OK, json);
        }

        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient(handler));
        return factory;
    }

    private string CreateFakePackage(string content = "fake package bytes")
    {
        string packagePath = Path.Combine(_tempDir, "update.zip");
        File.WriteAllText(packagePath, content);
        return packagePath;
    }

    private static string ComputeSha256Hex(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private void WriteSidecarHash(string packagePath, string hash)
    {
        File.WriteAllText(packagePath + ".sha256", hash);
    }

    // ── CheckUpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task CheckUpdateAsync_NewerVersionAvailable_ReturnsUpdateInfo()
    {
        // Arrange
        var sut = BuildService(BuildOptions(), BuildHttpClientFactory("1.5.0"));

        // Act
        Result<UpdateInfo?> result = await sut.CheckUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("1.5.0");
    }

    [Fact]
    public async Task CheckUpdateAsync_SameVersion_ReturnsSuccessWithNull()
    {
        // Arrange
        var options = BuildOptions();
        options.CurrentVersion = "1.1.0";
        var sut = BuildService(options, BuildHttpClientFactory("1.1.0"));

        // Act
        Result<UpdateInfo?> result = await sut.CheckUpdateAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckUpdateAsync_NetworkError_ReturnsFailure()
    {
        // Arrange
        var sut = BuildService(null, BuildHttpClientFactory("1.1.0", networkError: true));

        // Act
        Result<UpdateInfo?> result = await sut.CheckUpdateAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── ApplyUpdateAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyUpdateAsync_NoHashSidecar_SkipsHashCheck_ReturnsSuccess()
    {
        // Arrange: no .sha256 sidecar — hash check is skipped
        string packagePath = CreateFakePackage();
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService();

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsSuccess.Should().BeTrue("when no sidecar hash is present, hash verification is skipped");
    }

    [Fact]
    public async Task ApplyUpdateAsync_ValidHash_ReturnsSuccess()
    {
        // Arrange
        string packagePath = CreateFakePackage("valid package content");
        string correctHash = ComputeSha256Hex(packagePath);
        WriteSidecarHash(packagePath, correctHash);
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService();

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyUpdateAsync_InvalidHash_ReturnsUpdatePackageCorrupt()
    {
        // Arrange
        string packagePath = CreateFakePackage("original content");
        // Write wrong hash
        WriteSidecarHash(packagePath, new string('0', 64));
        var sut = BuildService();

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
        result.ErrorMessage.Should().Contain("integrity check failed");
    }

    [Fact]
    public async Task ApplyUpdateAsync_InvalidSignature_SignatureRequired_ReturnsSignatureVerificationFailed()
    {
        // Arrange: RequireAuthenticodeSignature = true, file is not a real signed binary
        string packagePath = CreateFakePackage("not a signed exe");
        string correctHash = ComputeSha256Hex(packagePath);
        WriteSidecarHash(packagePath, correctHash);
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService(BuildOptions(requireSignature: true));

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.SignatureVerificationFailed);
    }

    [Fact]
    public async Task ApplyUpdateAsync_SignatureNotRequired_SkipsAuthenticodeCheck()
    {
        // Arrange: RequireAuthenticodeSignature = false — unsigned file is acceptable
        string packagePath = CreateFakePackage("unsigned package");
        string correctHash = ComputeSha256Hex(packagePath);
        WriteSidecarHash(packagePath, correctHash);
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService(BuildOptions(requireSignature: false));

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsSuccess.Should().BeTrue("Authenticode check is skipped when not required");
    }

    [Fact]
    public async Task ApplyUpdateAsync_ValidPackage_CreatesBackup()
    {
        // Arrange
        string packagePath = CreateFakePackage("valid content");
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService();

        // Act
        await sut.ApplyUpdateAsync(packagePath);

        // Assert
        string[] backupDirs = Directory.GetDirectories(_backupDir, "backup_*");
        backupDirs.Should().NotBeEmpty("backup must be created before staging the update");
    }

    [Fact]
    public async Task ApplyUpdateAsync_ValidPackage_WritesAuditEntry()
    {
        // Arrange
        string packagePath = CreateFakePackage("valid content");
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "app binary");
        var sut = BuildService();

        // Act
        await sut.ApplyUpdateAsync(packagePath);

        // Assert
        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e => e.Action == "UPDATE_STAGED"),
            Arg.Any<CancellationToken>());
    }

    // ── RollbackAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task RollbackAsync_NoBackup_ReturnsRollbackFailed()
    {
        // Arrange: no backup exists
        var sut = BuildService();

        // Act
        Result result = await sut.RollbackAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
    }

    [Fact]
    public async Task RollbackAsync_HasBackup_ReturnsSuccess()
    {
        // Arrange: create a backup first
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "v1 binary");
        string packagePath = CreateFakePackage("valid content");
        var sut = BuildService();
        await sut.ApplyUpdateAsync(packagePath); // This creates a backup

        // Corrupt the app
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "CORRUPTED");

        // Act
        Result result = await sut.RollbackAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.ReadAllText(Path.Combine(_appDir, "app.exe")).Should().Be("v1 binary");
    }

    [Fact]
    public async Task RollbackAsync_HasBackup_WritesAuditEntry()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_appDir, "app.exe"), "v1");
        string packagePath = CreateFakePackage("pkg");
        var sut = BuildService();
        await sut.ApplyUpdateAsync(packagePath);

        // Act
        await sut.RollbackAsync();

        // Assert
        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e => e.Action == "UPDATE_ROLLED_BACK"),
            Arg.Any<CancellationToken>());
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

    private sealed class ThrowingHttpMessageHandler : HttpMessageHandler
    {
        private readonly Exception _exception;

        public ThrowingHttpMessageHandler(Exception ex) => _exception = ex;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromException<HttpResponseMessage>(_exception);
    }
}
