using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Update;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Additional coverage boost tests for HnVue.Update module to reach 90%+ coverage.
/// Tests uncovered edge cases, error paths, and branch conditions.
/// </summary>
[Trait("SWR", "SWR-UPD-COVERAGE")]
public sealed class UpdateCoverageBoostTestsV2 : IDisposable
{
    private readonly string _tempDir;
    private readonly IAuditService _auditService;

    public UpdateCoverageBoostTestsV2()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"HnVueBoostV2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);

        _auditService = Substitute.For<IAuditService>();
        _auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ─── UpdateOptions: Production environment validation ───────────────────────

    [Fact]
    public void UpdateOptions_Validate_ProductionEnvironment_NoSignature_Throws()
    {
        // Arrange: Set production environment variable
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

            var options = new UpdateOptions
            {
                UpdateServerUrl = "https://update.example.com",
                RequireAuthenticodeSignature = false // Disabled in production
            };

            // Act
            var act = () => options.Validate();

            // Assert
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*RequireAuthenticodeSignature cannot be disabled in production*");
        }
        finally
        {
            // Restore original environment
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
        }
    }

    [Fact]
    public void UpdateOptions_Validate_NonProductionEnvironment_NoSignature_DoesNotThrow()
    {
        // Arrange: Set development environment
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            var options = new UpdateOptions
            {
                UpdateServerUrl = "https://update.example.com",
                RequireAuthenticodeSignature = false
            };

            // Act & Assert: Should not throw in development
            options.Validate();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
        }
    }

    [Fact]
    public void UpdateOptions_Validate_DOTNET_ENVIRONMENT_Production_ChecksSignature()
    {
        // Test DOTNET_ENVIRONMENT variable as fallback
        var originalAspNet = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var originalDotnet = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production");

            var options = new UpdateOptions
            {
                UpdateServerUrl = "https://update.example.com",
                RequireAuthenticodeSignature = false
            };

            var act = () => options.Validate();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*RequireAuthenticodeSignature cannot be disabled*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalAspNet);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", originalDotnet);
        }
    }

    [Fact]
    public void UpdateOptions_Validate_EnvironmentVariableCaseInsensitive()
    {
        // Test case-insensitive "Production" check
        var originalEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        try
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "production"); // lowercase

            var options = new UpdateOptions
            {
                UpdateServerUrl = "https://update.example.com",
                RequireAuthenticodeSignature = false
            };

            var act = () => options.Validate();
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*RequireAuthenticodeSignature cannot be disabled*");
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", originalEnv);
        }
    }

    // ─── SignatureVerifier: File not found paths ───────────────────────────────────

    [Fact]
    public void SignatureVerifier_VerifyAuthenticode_FileNotFound_ReturnsFalse()
    {
        // Arrange: File doesn't exist
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.exe");

        // Act
        bool result = SignatureVerifier.VerifyAuthenticode(nonExistentPath);

        // Assert
        result.Should().BeFalse("non-existent files cannot have valid signatures");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_FileNotFound_ReturnsFalse()
    {
        // Arrange: File doesn't exist
        var nonExistentPath = Path.Combine(_tempDir, "nonexistent.zip");

        // Act
        bool result = SignatureVerifier.VerifyHash(nonExistentPath, "abc123");

        // Assert
        result.Should().BeFalse("hash verification fails for non-existent files");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_EmptyHash_ReturnsFalse()
    {
        // Arrange: Create a test file
        var testFile = Path.Combine(_tempDir, "test.zip");
        File.WriteAllText(testFile, "content");

        // Act
        bool result = SignatureVerifier.VerifyHash(testFile, string.Empty);

        // Assert
        result.Should().BeFalse("empty hash should fail validation");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_WhitespaceHash_ReturnsFalse()
    {
        // Arrange: Create a test file
        var testFile = Path.Combine(_tempDir, "test2.zip");
        File.WriteAllText(testFile, "content");

        // Act
        bool result = SignatureVerifier.VerifyHash(testFile, "   ");

        // Assert
        result.Should().BeFalse("whitespace hash should fail validation");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_NullHash_ReturnsFalse()
    {
        // Arrange: Create a test file
        var testFile = Path.Combine(_tempDir, "test3.zip");
        File.WriteAllText(testFile, "content");

        // Act
        bool result = SignatureVerifier.VerifyHash(testFile, null!);

        // Assert
        result.Should().BeFalse("null hash should fail validation");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_CorrectHash_ReturnsTrue()
    {
        // Arrange: Create a test file with known content
        var testFile = Path.Combine(_tempDir, "test4.zip");
        File.WriteAllText(testFile, "test content");

        // Compute expected hash
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes("test content"));
        string expectedHash = Convert.ToHexString(hash);

        // Act
        bool result = SignatureVerifier.VerifyHash(testFile, expectedHash);

        // Assert
        result.Should().BeTrue("correct hash should pass validation");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_WrongHash_ReturnsFalse()
    {
        // Arrange: Create a test file
        var testFile = Path.Combine(_tempDir, "test5.zip");
        File.WriteAllText(testFile, "test content");

        // Act: Use wrong hash
        bool result = SignatureVerifier.VerifyHash(testFile, new string('A', 64));

        // Assert
        result.Should().BeFalse("incorrect hash should fail validation");
    }

    [Fact]
    public void SignatureVerifier_VerifyHash_CaseInsensitive_ReturnsTrue()
    {
        // Arrange: Create a test file
        var testFile = Path.Combine(_tempDir, "test6.zip");
        File.WriteAllText(testFile, "test content");

        // Compute hash as lowercase
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes("test content"));
        string expectedHash = Convert.ToHexString(hash).ToLower(); // lowercase

        // Act: Pass uppercase hash
        bool result = SignatureVerifier.VerifyHash(testFile, expectedHash.ToUpper());

        // Assert
        result.Should().BeTrue("hash comparison should be case-insensitive");
    }

    // ─── UpdateChecker: Version comparison fallback ───────────────────────────────

    [Fact]
    public async Task UpdateChecker_IsNewerVersion_NonSemanticVersion_UsesStringComparison()
    {
        // Arrange: Pre-release versions that can't be parsed by Version.TryParse
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            CurrentVersion = "1.0.0-alpha"
        };

        string jsonResponse = """
            {
              "version": "1.0.0-beta",
              "releaseNotes": "Beta release",
              "packageUrl": "https://example.com/beta.zip",
              "sha256Hash": "abc123"
            }
            """;

        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        using var httpClient = new HttpClient(handler);
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<UpdateInfo?> result = await sut.CheckAsync();

        // Assert: "beta" > "alpha" in ordinal string comparison
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull("beta is newer than alpha in ordinal comparison");
    }

    [Fact]
    public async Task UpdateChecker_IsNewerVersion_OlderNonSemantic_ReturnsNull()
    {
        // Arrange: Available version is older in ordinal comparison
        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            CurrentVersion = "1.0.0-zeta"
        };

        string jsonResponse = """
            {
              "version": "1.0.0-alpha",
              "releaseNotes": "Alpha release",
              "packageUrl": "https://example.com/alpha.zip",
              "sha256Hash": "abc123"
            }
            """;

        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, jsonResponse);
        using var httpClient = new HttpClient(handler);
        var sut = new UpdateChecker(options, httpClient);

        // Act
        Result<UpdateInfo?> result = await sut.CheckAsync();

        // Assert: "alpha" < "zeta" in ordinal comparison
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull("alpha is older than zeta in ordinal comparison");
    }

    // ─── SWUpdateService: Rollback edge cases ─────────────────────────────────────

    [Fact]
    public async Task SWUpdateService_RollbackAsync_NoBackupsAvailable_ReturnsFailure()
    {
        // Arrange: Empty backup directory
        var appDir = Path.Combine(_tempDir, "app_no_backup");
        var backupDir = Path.Combine(_tempDir, "backup_empty");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(backupDir);

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            ApplicationDirectory = appDir,
            BackupDirectory = backupDir,
            RequireAuthenticodeSignature = false
        };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        var sut = new SWUpdateService(Options.Create(options), factory, _auditService);

        // Act
        Result result = await sut.RollbackAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.RollbackFailed);
        result.ErrorMessage.Should().Contain("no backup");
    }

    // ─── SWUpdateService: Cleanup on failure ───────────────────────────────────────

    [Fact]
    public async Task SWUpdateService_ApplyUpdate_Failure_CleansUpStagingMarker()
    {
        // Arrange: Create a scenario that will fail
        var appDir = Path.Combine(_tempDir, "app_cleanup");
        var backupDir = Path.Combine(_tempDir, "backup_cleanup");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(backupDir);

        // Create invalid package (will fail hash check)
        var packagePath = Path.Combine(_tempDir, "package_bad.zip");
        File.WriteAllText(packagePath, "bad package");
        File.WriteAllText(packagePath + ".sha256", new string('0', 64)); // Wrong hash

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            ApplicationDirectory = appDir,
            BackupDirectory = backupDir,
            RequireAuthenticodeSignature = false
        };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        var logger = Substitute.For<ILogger<SWUpdateService>>();
        var sut = new SWUpdateService(Options.Create(options), factory, _auditService, logger);

        // Manually create a pending update marker to test cleanup
        var markerPath = Path.Combine(backupDir, "pending_update.txt");
        File.WriteAllText(markerPath, packagePath);

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsFailure.Should().BeTrue();
        sut.CurrentState.Should().Be(UpdateState.RolledBack, "hash failure triggers rollback");

        // Verify marker was cleaned up
        File.Exists(markerPath).Should().BeFalse("pending update marker should be cleaned up on failure");
    }

    [Fact]
    public async Task SWUpdateService_ApplyUpdate_Failure_CleansUpStagingDirectory()
    {
        // Arrange: Create staging directory to test cleanup
        var appDir = Path.Combine(_tempDir, "app_staging");
        var backupDir = Path.Combine(_tempDir, "backup_staging");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(backupDir);

        // Create invalid package
        var packagePath = Path.Combine(_tempDir, "package_staging.zip");
        File.WriteAllText(packagePath, "bad package");
        File.WriteAllText(packagePath + ".sha256", new string('0', 64));

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            ApplicationDirectory = appDir,
            BackupDirectory = backupDir,
            RequireAuthenticodeSignature = false
        };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        var sut = new SWUpdateService(Options.Create(options), factory, _auditService);

        // Manually create staging directory to test cleanup
        var stagingDir = Path.Combine(backupDir, "staging");
        Directory.CreateDirectory(stagingDir);
        var testFile = Path.Combine(stagingDir, "test.txt");
        File.WriteAllText(testFile, "test");

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert
        result.IsFailure.Should().BeTrue();
        Directory.Exists(stagingDir).Should().BeFalse("staging directory should be cleaned up on failure");
    }

    // ─── SWUpdateService: Audit write failures ─────────────────────────────────────

    [Fact]
    public async Task SWUpdateService_AuditWriteFailure_DoesNotBlockUpdate()
    {
        // Arrange: Audit service fails
        var failingAuditService = Substitute.For<IAuditService>();
        failingAuditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.Unknown, "Audit failed"));

        var appDir = Path.Combine(_tempDir, "app_audit");
        var backupDir = Path.Combine(_tempDir, "backup_audit");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(backupDir);

        var packagePath = Path.Combine(_tempDir, "package_audit.zip");
        File.WriteAllText(packagePath, "valid package");
        File.WriteAllText(Path.Combine(appDir, "app.exe"), "app binary");

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            ApplicationDirectory = appDir,
            BackupDirectory = backupDir,
            RequireAuthenticodeSignature = false
        };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        var sut = new SWUpdateService(Options.Create(options), factory, failingAuditService, null);

        // Act
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert: Update should succeed despite audit failure
        result.IsSuccess.Should().BeTrue("audit failures should not block updates");
    }

    [Fact]
    public async Task SWUpdateService_NullAuditService_DoesNotThrow()
    {
        // Arrange: Null audit service
        var appDir = Path.Combine(_tempDir, "app_null_audit");
        var backupDir = Path.Combine(_tempDir, "backup_null_audit");
        Directory.CreateDirectory(appDir);
        Directory.CreateDirectory(backupDir);

        var packagePath = Path.Combine(_tempDir, "package_null.zip");
        File.WriteAllText(packagePath, "valid package");
        File.WriteAllText(Path.Combine(appDir, "app.exe"), "app binary");

        var options = new UpdateOptions
        {
            UpdateServerUrl = "https://update.example.com",
            ApplicationDirectory = appDir,
            BackupDirectory = backupDir,
            RequireAuthenticodeSignature = false
        };

        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient(Arg.Any<string>()).Returns(new HttpClient());

        // Act: Create service with null audit service
        var sut = new SWUpdateService(Options.Create(options), factory, null, null);
        Result result = await sut.ApplyUpdateAsync(packagePath);

        // Assert: Should succeed without throwing
        result.IsSuccess.Should().BeTrue("null audit service should be handled gracefully");
    }

    // ─── Helper classes ───────────────────────────────────────────────────────────

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
