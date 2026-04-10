using System.IO;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.SystemAdmin;
using NSubstitute;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

[Trait("SWR", "SWR-SA-010")]
public sealed class SystemAdminServiceTests
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

    public SystemAdminServiceTests()
    {
        _settingsRepo = Substitute.For<ISystemSettingsRepository>();
        _auditRepo = Substitute.For<IAuditRepository>();
        _securityContext = Substitute.For<ISecurityContext>();
        _sut = new SystemAdminService(_settingsRepo, _auditRepo, _securityContext);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullSettingsRepo_ThrowsArgumentNullException()
    {
        var act = () => new SystemAdminService(null!, _auditRepo, _securityContext);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullAuditRepo_ThrowsArgumentNullException()
    {
        var act = () => new SystemAdminService(_settingsRepo, null!, _securityContext);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullSecurityContext_ThrowsArgumentNullException()
    {
        var act = () => new SystemAdminService(_settingsRepo, _auditRepo, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── GetSettingsAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetSettings_DelegatesToRepository()
    {
        var settings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(settings));

        var result = await _sut.GetSettingsAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(settings);
    }

    // ── UpdateSettingsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_ValidSettings_SavesAndReturnsSuccess()
    {
        var oldSettings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UpdateSettingsAsync(ValidSettings());

        result.IsSuccess.Should().BeTrue();
        await _settingsRepo.Received(1).SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_NullSettings_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.UpdateSettingsAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateSettings_InvalidPacsPort_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Dicom.PacsPort = 99999;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_EmptyLocalAeTitle_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Dicom.LocalAeTitle = "";

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_ZeroSessionTimeout_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Security.SessionTimeoutMinutes = 0;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_ZeroMaxFailedLogins_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Security.MaxFailedLogins = 0;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    // ── ExportAuditLogAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ExportAuditLog_ValidPath_WritesFile()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_{Guid.NewGuid()}.csv");
        var entries = (IReadOnlyList<AuditEntry>)new[]
        {
            new AuditEntry(DateTimeOffset.UtcNow, "U1", "LOGIN", "hash1"),
        };
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);

            result.IsSuccess.Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().Contain("LOGIN");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task ExportAuditLog_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ExportAuditLogAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ExportAuditLog_EmptyPath_ReturnsValidationFailure()
    {
        var result = await _sut.ExportAuditLogAsync("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ExportAuditLog_RepositoryFailure_PropagatesFailure()
    {
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AuditEntry>>(
                ErrorCode.DatabaseError, "Query failed"));

        var result = await _sut.ExportAuditLogAsync(Path.GetTempFileName());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── Additional Coverage Tests ───────────────────────────────────────────────

    [Fact]
    public async Task ExportAuditLog_EmptyLog_WritesHeaderOnly()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_empty_{Guid.NewGuid()}.csv");
        var entries = (IReadOnlyList<AuditEntry>)Array.Empty<AuditEntry>();
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);

            result.IsSuccess.Should().BeTrue();
            var lines = await File.ReadAllLinesAsync(tempPath);
            lines.Should().HaveCount(1); // Only header
            lines[0].Should().Be("EntryId,Timestamp,UserId,Action,Details,PreviousHash,CurrentHash");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task ExportAuditLog_SpecialCharactersInDetails_EscapesCorrectly()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_special_{Guid.NewGuid()}.csv");
        var entries = new[]
        {
            new AuditEntry(
                DateTimeOffset.UtcNow,
                "U1",
                "ACTION",
                "hash1",
                "Text with, comma, \"quotes\", and\nnewlines",
                null),
        };
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success((IReadOnlyList<AuditEntry>)entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);

            result.IsSuccess.Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().Contain("\"Text with, comma, \"\"quotes\"\", and\nnewlines\"");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task ExportAuditLog_HashChainIntegrity_IncludesHashes()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_hash_{Guid.NewGuid()}.csv");
        var entries = new[]
        {
            new AuditEntry(
                "1",
                DateTimeOffset.UtcNow,
                "U1",
                "LOGIN",
                "details1",
                null,
                "hash1"),
            new AuditEntry(
                "2",
                DateTimeOffset.UtcNow.AddMinutes(1),
                "U1",
                "LOGOUT",
                "details2",
                "hash1",
                "hash2"),
        };
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success((IReadOnlyList<AuditEntry>)entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);

            result.IsSuccess.Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().Contain("hash1,hash2");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public async Task UpdateSettings_InvalidPacsPort_ReturnsValidationFailure_Theory(int invalidPort)
    {
        var settings = ValidSettings();
        settings.Dicom.PacsPort = invalidPort;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("PACS port must be between 1 and 65535");
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task UpdateSettings_EmptyOrWhitespaceLocalAeTitle_ReturnsValidationFailure(string? aeTitle)
    {
        var settings = ValidSettings();
        settings.Dicom.LocalAeTitle = aeTitle!;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("Local AE Title is required");
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_AeTitleWithSpaces_Accepted()
    {
        var settings = ValidSettings();
        settings.Dicom.LocalAeTitle = "HNVUE  AET";
        var oldSettings = ValidSettings();

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task UpdateSettings_NegativeSessionTimeout_ReturnsValidationFailure(int negativeTimeout)
    {
        var settings = ValidSettings();
        settings.Security.SessionTimeoutMinutes = negativeTimeout;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("Session timeout must be at least 1 minute");
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_AllValidSettings_SavesAndReturnsSuccess()
    {
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS_SERVER",
                PacsHost = "192.168.1.200",
                PacsPort = 104,
                LocalAeTitle = "HNVUE_STATION",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM3",
                BaudRate = 9600,
                TimeoutMs = 5000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLogins = 3,
            },
        };
        var oldSettings = ValidSettings();
        SystemSettings? capturedSettings = null;
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => capturedSettings = x.Arg<SystemSettings>());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsSuccess.Should().BeTrue();
        capturedSettings.Should().NotBeNull();
        capturedSettings?.Dicom.PacsAeTitle.Should().Be("PACS_SERVER");
        capturedSettings?.Dicom.PacsHost.Should().Be("192.168.1.200");
        capturedSettings?.Dicom.PacsPort.Should().Be(104);
        capturedSettings?.Dicom.LocalAeTitle.Should().Be("HNVUE_STATION");
        capturedSettings?.Generator.ComPort.Should().Be("COM3");
        capturedSettings?.Security.SessionTimeoutMinutes.Should().Be(30);
    }

    [Fact]
    public async Task UpdateSettings_RepositorySaveFailure_PropagatesFailure()
    {
        var settings = ValidSettings();
        var oldSettings = ValidSettings();
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "Save failed"));

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
        result.ErrorMessage.Should().Contain("Save failed");
        await _auditRepo.DidNotReceive().AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSettings_RepositoryFailure_PropagatesFailure()
    {
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SystemSettings>(
                ErrorCode.DatabaseError, "Load failed"));

        var result = await _sut.GetSettingsAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
        result.ErrorMessage.Should().Contain("Load failed");
    }

    [Fact]
    public async Task ExportAuditLog_CreatesDirectoryWhenNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"audit_dir_{Guid.NewGuid()}");
        var tempPath = Path.Combine(tempDir, "audit.csv");
        var entries = (IReadOnlyList<AuditEntry>)Array.Empty<AuditEntry>();
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);

            result.IsSuccess.Should().BeTrue();
            Directory.Exists(tempDir).Should().BeTrue();
            File.Exists(tempPath).Should().BeTrue();
        }
        finally
        {
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
        }
    }

    // ── Audit Logging Tests (T-012) ───────────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_Success_CallsAuditRepositoryAppendAsync()
    {
        // Arrange
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 111; // Changed value

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _sut.UpdateSettingsAsync(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e =>
                e.Action == "SettingsChanged" &&
                e.UserId == "system"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_WithAuthenticatedUser_UsesUserIdFromSecurityContext()
    {
        // Arrange
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Security.SessionTimeoutMinutes = 30; // Changed value

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _securityContext.CurrentUserId.Returns("user-123");
        _securityContext.IsAuthenticated.Returns(true);

        // Act
        var result = await _sut.UpdateSettingsAsync(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.UserId == "user-123"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_WithNoAuthenticatedUser_UsesSystemAsUserId()
    {
        // Arrange
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.LocalAeTitle = "NEW_AE"; // Changed value

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _securityContext.CurrentUserId.Returns((string?)null);
        _securityContext.IsAuthenticated.Returns(false);

        // Act
        var result = await _sut.UpdateSettingsAsync(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _auditRepo.Received(1).AppendAsync(
            Arg.Is<AuditEntry>(e => e.UserId == "system"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_Success_IncludesChangedFieldsInAuditDetails()
    {
        // Arrange
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 111;
        newSettings.Security.SessionTimeoutMinutes = 30;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>(null));
        _securityContext.CurrentUserId.Returns("admin-user");

        AuditEntry? capturedEntry = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => capturedEntry = x.Arg<AuditEntry>());

        // Act
        var result = await _sut.UpdateSettingsAsync(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedEntry.Should().NotBeNull();
        capturedEntry!.Action.Should().Be("SettingsChanged");
        capturedEntry.Details.Should().NotBeNullOrEmpty();
        capturedEntry.Details.Should().Contain("Dicom.PacsPort");
        capturedEntry.Details.Should().Contain("104"); // Old value
        capturedEntry.Details.Should().Contain("111"); // New value
        capturedEntry.Details.Should().Contain("Security.SessionTimeoutMinutes");
        capturedEntry.Details.Should().Contain("15"); // Old value
        capturedEntry.Details.Should().Contain("30"); // New value
    }

    [Fact]
    public async Task UpdateSettings_ValidationFailure_DoesNotCreateAuditEntry()
    {
        // Arrange
        var invalidSettings = ValidSettings();
        invalidSettings.Dicom.PacsPort = 99999; // Invalid

        // Act
        var result = await _sut.UpdateSettingsAsync(invalidSettings);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _auditRepo.DidNotReceive().AppendAsync(
            Arg.Any<AuditEntry>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_RepositoryFailure_DoesNotCreateAuditEntry()
    {
        // Arrange
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 111;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "Save failed"));

        // Act
        var result = await _sut.UpdateSettingsAsync(newSettings);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _auditRepo.DidNotReceive().AppendAsync(
            Arg.Any<AuditEntry>(),
            Arg.Any<CancellationToken>());
    }
}
