using System.IO;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.SystemAdmin;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

/// <summary>
/// S13-R2 coverage gap tests for HnVue.SystemAdmin module (85%+ target).
/// </summary>
public sealed class SystemAdminS13R2CoverageTests
{
    private readonly ISystemSettingsRepository _settingsRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ISecurityContext _securityContext;
    private readonly SystemAdminService _sut;

    private static SystemSettings ValidSettings() => new()
    {
        Dicom = new DicomSettings
        {
            PacsAeTitle = "PACS",
            PacsHost = "192.168.1.100",
            PacsPort = 104,
            LocalAeTitle = "HNVUE",
        },
        Generator = new GeneratorSettings(),
        Security = new SecuritySettings
        {
            SessionTimeoutMinutes = 15,
            MaxFailedLogins = 5,
        },
    };

    public SystemAdminS13R2CoverageTests()
    {
        _settingsRepo = Substitute.For<ISystemSettingsRepository>();
        _auditRepo = Substitute.For<IAuditRepository>();
        _securityContext = Substitute.For<ISecurityContext>();
        _sut = new SystemAdminService(_settingsRepo, _auditRepo, _securityContext);
    }

    private void SetupAuditChain()
    {
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
    }

    private void SetupQueryAsync(IReadOnlyList<AuditEntry> entries)
    {
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(entries)));
    }

    // ── ValidateSettings (through UpdateSettingsAsync) ────────────────────────

    [Theory]
    [InlineData(0, "PACS port must be between 1 and 65535")]
    [InlineData(-1, "PACS port must be between 1 and 65535")]
    [InlineData(65536, "PACS port must be between 1 and 65535")]
    public async Task UpdateSettingsAsync_InvalidPacsPort_ReturnsValidationFailure(
        int port, string expectedMessage)
    {
        var settings = ValidSettings();
        settings.Dicom.PacsPort = port;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain(expectedMessage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateSettingsAsync_EmptyAeTitle_ReturnsValidationFailure(string? aeTitle)
    {
        var settings = ValidSettings();
        settings.Dicom.LocalAeTitle = aeTitle!;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("Local AE Title");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task UpdateSettingsAsync_InvalidSessionTimeout_ReturnsValidationFailure(int minutes)
    {
        var settings = ValidSettings();
        settings.Security.SessionTimeoutMinutes = minutes;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Session timeout");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateSettingsAsync_InvalidMaxFailedLogins_ReturnsValidationFailure(int max)
    {
        var settings = ValidSettings();
        settings.Security.MaxFailedLogins = max;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.ErrorMessage.Should().Contain("Max failed logins");
    }

    [Fact]
    public async Task UpdateSettingsAsync_Port1_Succeeds()
    {
        var settings = ValidSettings();
        settings.Dicom.PacsPort = 1;
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(ValidSettings()));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateSettingsAsync_Port65535_Succeeds()
    {
        var settings = ValidSettings();
        settings.Dicom.PacsPort = 65535;
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(ValidSettings()));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsSuccess.Should().BeTrue();
    }

    // ── CsvEscape (through ExportAuditLogAsync) ───────────────────────────────

    [Fact]
    public async Task ExportAuditLogAsync_CommaInDetails_EscapesCorrectly()
    {
        var entries = new List<AuditEntry>
        {
            new(EntryId: "1", Timestamp: DateTimeOffset.UtcNow, UserId: "admin",
                Action: "SettingsChanged", Details: "Changed host from 192.168.1.1, to 10.0.0.1",
                PreviousHash: null, CurrentHash: "abc123"),
        };
        SetupQueryAsync(entries);

        using var tempFile = new TempFile();
        var result = await _sut.ExportAuditLogAsync(tempFile.Path);

        result.IsSuccess.Should().BeTrue();
        var csv = await File.ReadAllTextAsync(tempFile.Path);
        csv.Should().Contain("\"Changed host from 192.168.1.1, to 10.0.0.1\"");
    }

    [Fact]
    public async Task ExportAuditLogAsync_QuoteInDetails_EscapesCorrectly()
    {
        var entries = new List<AuditEntry>
        {
            new(EntryId: "1", Timestamp: DateTimeOffset.UtcNow, UserId: "admin",
                Action: "Test", Details: "Value was \"quoted\"",
                PreviousHash: null, CurrentHash: "hash1"),
        };
        SetupQueryAsync(entries);

        using var tempFile = new TempFile();
        var result = await _sut.ExportAuditLogAsync(tempFile.Path);

        result.IsSuccess.Should().BeTrue();
        var csv = await File.ReadAllTextAsync(tempFile.Path);
        csv.Should().Contain("\"Value was \"\"quoted\"\"\"");
    }

    [Fact]
    public async Task ExportAuditLogAsync_NewlineInDetails_EscapesCorrectly()
    {
        var entries = new List<AuditEntry>
        {
            new(EntryId: "1", Timestamp: DateTimeOffset.UtcNow, UserId: "admin",
                Action: "Test", Details: "Line1\nLine2",
                PreviousHash: null, CurrentHash: "hash1"),
        };
        SetupQueryAsync(entries);

        using var tempFile = new TempFile();
        var result = await _sut.ExportAuditLogAsync(tempFile.Path);

        result.IsSuccess.Should().BeTrue();
        var csv = await File.ReadAllTextAsync(tempFile.Path);
        csv.Should().Contain("\"Line1\nLine2\"");
    }

    [Fact]
    public async Task ExportAuditLogAsync_NullHashes_RenderAsEmpty()
    {
        var entries = new List<AuditEntry>
        {
            new(EntryId: "1", Timestamp: DateTimeOffset.UtcNow, UserId: "admin",
                Action: "First", Details: "Initial",
                PreviousHash: null, CurrentHash: null),
        };
        SetupQueryAsync(entries);

        using var tempFile = new TempFile();
        var result = await _sut.ExportAuditLogAsync(tempFile.Path);

        result.IsSuccess.Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(tempFile.Path);
        lines.Should().HaveCount(2);
        lines[1].Should().EndWith(",");
    }

    // ── Cache Behavior ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetSettings_CachesResult_SecondCallUsesCache()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(settings));

        var first = await _sut.GetSettingsAsync();
        var second = await _sut.GetSettingsAsync();

        first.IsSuccess.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        await _settingsRepo.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSettings_FailureNotCached_RepositoryCalledTwice()
    {
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(
                Result.Failure<SystemSettings>(ErrorCode.DatabaseError, "DB error"),
                Result.Success(ValidSettings()));

        var first = await _sut.GetSettingsAsync();
        var second = await _sut.GetSettingsAsync();

        first.IsFailure.Should().BeTrue();
        second.IsSuccess.Should().BeTrue();
        await _settingsRepo.Received(2).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_InvalidatesCache_NextGetCallsRepository()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        SetupAuditChain();

        await _sut.GetSettingsAsync();

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        await _sut.UpdateSettingsAsync(newSettings);

        await _sut.GetSettingsAsync();

        // 3 calls: initial cache load + UpdateSettings old-settings + post-invalidation load
        await _settingsRepo.Received(3).GetAsync(Arg.Any<CancellationToken>());
    }

    // ── Audit Trail ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_DetectsDicomPortChange_InAuditDetails()
    {
        var oldSettings = ValidSettings();
        oldSettings.Dicom.PacsPort = 104;
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<string?>("prevhash"));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 11112;
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e =>
                e.Details.Contains("PacsPort") &&
                e.Details.Contains("104") &&
                e.Details.Contains("11112") &&
                e.PreviousHash == "prevhash"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_DetectsGeneratorChanges_InAuditDetails()
    {
        var oldSettings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var newSettings = ValidSettings();
        newSettings.Generator.ComPort = "COM3";
        newSettings.Generator.BaudRate = 115200;
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e =>
                e.Details.Contains("ComPort") &&
                e.Details.Contains("BaudRate")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_DetectsSecurityChanges_InAuditDetails()
    {
        var oldSettings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var newSettings = ValidSettings();
        newSettings.Security.SessionTimeoutMinutes = 30;
        newSettings.Security.MaxFailedLogins = 10;
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e =>
                e.Details.Contains("SessionTimeoutMinutes") &&
                e.Details.Contains("MaxFailedLogins")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_NoChanges_StillLogsAudit()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        await _sut.UpdateSettingsAsync(settings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.Details.Contains("No changes detected")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_AuditHashChain_UsesPreviousHash()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<string?>("chainhash"));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>()).Returns(Result.Success());

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.PreviousHash == "chainhash" && !string.IsNullOrEmpty(e.CurrentHash)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_GetLastHashFailure_PropagatesError()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.DatabaseError, "hash lookup failed"));

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        var result = await _sut.UpdateSettingsAsync(newSettings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task UpdateSettings_AuditAppendFailure_PropagatesError()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.Success<string?>("hash"));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "append failed"));

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        var result = await _sut.UpdateSettingsAsync(newSettings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task UpdateSettings_AuthenticatedUser_UsesUserId()
    {
        _securityContext.IsAuthenticated.Returns(true);
        _securityContext.CurrentUserId.Returns("user42");
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.UserId == "user42"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_UnauthenticatedUser_UsesSystem()
    {
        _securityContext.IsAuthenticated.Returns(false);
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        SetupAuditChain();

        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";
        await _sut.UpdateSettingsAsync(newSettings);

        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.UserId == "system"),
            Arg.Any<CancellationToken>());
    }

    // ── ExportAuditLogAsync Edge Cases ────────────────────────────────────────

    [Fact]
    public async Task ExportAuditLogAsync_CreatesDirectory_WhenNotExists()
    {
        SetupQueryAsync(new List<AuditEntry>());

        var dir = Path.Combine(Path.GetTempPath(), $"HnVueExport_{Guid.NewGuid():N}", "nested");
        try
        {
            var result = await _sut.ExportAuditLogAsync(Path.Combine(dir, "audit.csv"));

            result.IsSuccess.Should().BeTrue();
            File.Exists(Path.Combine(dir, "audit.csv")).Should().BeTrue();
        }
        finally
        {
            try
            {
                var parent = Path.GetDirectoryName(Path.GetDirectoryName(dir)!)!;
                if (Directory.Exists(parent))
                {
                    Directory.Delete(parent, recursive: true);
                }
            }
            catch (IOException)
            {
                // File lock on Windows — ignore
            }
        }
    }

    [Fact]
    public async Task ExportAuditLogAsync_EmptyLog_WritesHeaderOnly()
    {
        SetupQueryAsync(new List<AuditEntry>());

        using var tempFile = new TempFile();
        var result = await _sut.ExportAuditLogAsync(tempFile.Path);

        result.IsSuccess.Should().BeTrue();
        var lines = await File.ReadAllLinesAsync(tempFile.Path);
        lines.Should().HaveCount(1);
        lines[0].Should().StartWith("EntryId,");
    }

    [Fact]
    public async Task ExportAuditLogAsync_WhitespacePath_ReturnsValidationFailure()
    {
        var result = await _sut.ExportAuditLogAsync("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── SystemSettingsRepository Edge Cases ───────────────────────────────────

    [Fact]
    public async Task SystemSettingsRepository_FirstRun_ReturnsDefaults()
    {
        using var tempDir = new TempDir();
        var sut = new TestableSystemSettingsRepository(tempDir.Path);

        var result = await sut.GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task SystemSettingsRepository_SaveAndLoad_RoundTrip()
    {
        using var tempDir = new TempDir();
        var sut = new TestableSystemSettingsRepository(tempDir.Path);
        var settings = ValidSettings();

        var saveResult = await sut.SaveAsync(settings);
        saveResult.IsSuccess.Should().BeTrue();

        var loadResult = await sut.GetAsync();
        loadResult.IsSuccess.Should().BeTrue();
        loadResult.Value.Dicom.PacsAeTitle.Should().Be("PACS");
        loadResult.Value.Dicom.PacsPort.Should().Be(104);
        loadResult.Value.Security.SessionTimeoutMinutes.Should().Be(15);
    }

    [Fact]
    public async Task SystemSettingsRepository_SaveOverwrites_PreviousValue()
    {
        using var tempDir = new TempDir();
        var sut = new TestableSystemSettingsRepository(tempDir.Path);

        var settings1 = ValidSettings();
        settings1.Dicom.PacsHost = "host1";
        await sut.SaveAsync(settings1);

        var settings2 = ValidSettings();
        settings2.Dicom.PacsHost = "host2";
        await sut.SaveAsync(settings2);

        var result = await sut.GetAsync();
        result.Value.Dicom.PacsHost.Should().Be("host2");
    }

    [Fact]
    public async Task SystemSettingsRepository_CorruptJson_ReturnsFailure()
    {
        using var tempDir = new TempDir();
        var path = Path.Combine(tempDir.Path, "settings.json");
        Directory.CreateDirectory(tempDir.Path);
        await File.WriteAllTextAsync(path, "{ invalid json }}}");

        var sut = new TestableSystemSettingsRepository(tempDir.Path);
        var result = await sut.GetAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.FileOperationFailed);
    }

    // ── EfSystemSettingsRepository Edge Cases ─────────────────────────────────

    [Fact]
    public async Task EfSystemSettingsRepository_GetAsync_NoData_ReturnsDefaults()
    {
        using var ctx = CreateSqliteContext();
        var sut = new EfSystemSettingsRepository(ctx.Context);

        var result = await sut.GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Dicom.Should().NotBeNull();
    }

    [Fact]
    public async Task EfSystemSettingsRepository_SaveThenGet_RoundTrip()
    {
        using var ctx = CreateSqliteContext();
        var sut = new EfSystemSettingsRepository(ctx.Context);
        var settings = ValidSettings();

        await sut.SaveAsync(settings);
        var result = await sut.GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("PACS");
        result.Value.Dicom.PacsPort.Should().Be(104);
        result.Value.Generator.BaudRate.Should().Be(9600);
        result.Value.Security.MaxFailedLogins.Should().Be(5);
    }

    [Fact]
    public async Task EfSystemSettingsRepository_UpdateExisting_RetainsLatest()
    {
        using var ctx = CreateSqliteContext();
        var sut = new EfSystemSettingsRepository(ctx.Context);

        var settings1 = ValidSettings();
        settings1.Dicom.PacsHost = "host1";
        await sut.SaveAsync(settings1);

        var settings2 = ValidSettings();
        settings2.Dicom.PacsHost = "host2";
        await sut.SaveAsync(settings2);

        var result = await sut.GetAsync();
        result.Value.Dicom.PacsHost.Should().Be("host2");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static SqliteContext CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var context = new HnVueDbContext(options);
        context.Database.EnsureCreated();
        return new SqliteContext(context, connection);
    }

    private sealed class SqliteContext : IDisposable
    {
        public HnVueDbContext Context { get; }
        private readonly SqliteConnection _connection;

        public SqliteContext(HnVueDbContext context, SqliteConnection connection)
        {
            Context = context;
            _connection = connection;
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Close();
            _connection.Dispose();
        }
    }

    private sealed class TempFile : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"test_{Guid.NewGuid():N}.csv");

        public void Dispose()
        {
            if (File.Exists(Path))
            {
                File.Delete(Path);
            }
        }
    }

    private sealed class TempDir : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"HnVueSettings_{Guid.NewGuid():N}");

        public TempDir() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class TestableSystemSettingsRepository(string basePath) : SystemSettingsRepository
    {
        protected override string GetStorePath() => System.IO.Path.Combine(basePath, "settings.json");
    }
}
