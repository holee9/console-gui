using System.IO;
using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.SystemAdmin;
using NSubstitute;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

/// <summary>
/// Coverage boost tests for SystemAdmin module: cache behavior, Generator/Dicom change audit,
/// audit hash chain edge cases, EfSystemSettingsRepository edge cases.
/// </summary>
public sealed class SystemAdminCoverageBoostTests
{
    private readonly ISystemSettingsRepository _settingsRepo;
    private readonly IAuditRepository _auditRepo;
    private readonly ISecurityContext _securityContext;
    private readonly SystemAdminService _sut;

    private static SystemSettings ValidSettings() => new()
    {
        Dicom = new DicomSettings
        {
            PacsAeTitle = "PACS", PacsHost = "192.168.1.100", PacsPort = 104, LocalAeTitle = "HNVUE",
        },
        Generator = new GeneratorSettings { ComPort = "COM1", BaudRate = 9600, TimeoutMs = 5000 },
        Security = new SecuritySettings { SessionTimeoutMinutes = 15, MaxFailedLogins = 5 },
    };

    public SystemAdminCoverageBoostTests()
    {
        _settingsRepo = Substitute.For<ISystemSettingsRepository>();
        _auditRepo = Substitute.For<IAuditRepository>();
        _securityContext = Substitute.For<ISecurityContext>();
        _sut = new SystemAdminService(_settingsRepo, _auditRepo, _securityContext);
    }

    // ── Cache behavior ────────────────────────────────────────────────────────

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
        // Repository should only be called once due to 5-min cache
        await _settingsRepo.Received(1).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetSettings_RepositoryFailure_DoesNotCache()
    {
        var callCount = 0;
        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                return callCount == 1
                    ? Result.Failure<SystemSettings>(ErrorCode.DatabaseError, "fail")
                    : Result.Success(ValidSettings());
            });

        var first = await _sut.GetSettingsAsync();
        first.IsFailure.Should().BeTrue();

        var second = await _sut.GetSettingsAsync();
        second.IsSuccess.Should().BeTrue();
        // Both calls should hit the repository since failure is not cached
        await _settingsRepo.Received(2).GetAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateSettings_InvalidatesCache()
    {
        var settings = ValidSettings();
        var updatedSettings = ValidSettings();
        updatedSettings.Dicom.PacsPort = 200;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Success(settings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<string?>("prev-hash"));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // First call populates cache
        await _sut.GetSettingsAsync();

        // Update should invalidate cache
        await _sut.UpdateSettingsAsync(updatedSettings);

        // Next GetSettings should call repository again
        await _sut.GetSettingsAsync();

        await _settingsRepo.Received(3).GetAsync(Arg.Any<CancellationToken>());
    }

    // ── Generator settings change audit ──────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_GeneratorComPortChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Generator.ComPort = "COM5";

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>("hash"));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Generator.ComPort");
        captured.Details.Should().Contain("COM1");
        captured.Details.Should().Contain("COM5");
    }

    [Fact]
    public async Task UpdateSettings_GeneratorBaudRateChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Generator.BaudRate = 115200;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Generator.BaudRate");
    }

    [Fact]
    public async Task UpdateSettings_GeneratorTimeoutMsChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Generator.TimeoutMs = 10000;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Generator.TimeoutMs");
    }

    // ── DICOM settings change audit ──────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_PacsHostChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsHost = "10.0.0.1";

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Dicom.PacsHost");
    }

    [Fact]
    public async Task UpdateSettings_PacsAeTitleChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsAeTitle = "NEW_PACS";

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Dicom.PacsAeTitle");
    }

    [Fact]
    public async Task UpdateSettings_LocalAeTitleChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.LocalAeTitle = "NEW_LOCAL";

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Dicom.LocalAeTitle");
    }

    // ── No changes audit path ────────────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_NoActualChanges_AuditDetails_SaysNoChanges()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings(); // identical

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("No changes detected");
    }

    // ── Audit hash chain edge cases ──────────────────────────────────────────

    [Fact]
    public async Task UpdateSettings_AuditAppendFailure_PropagatesFailure()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 200;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>("hash"));
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "Audit write failed"));

        var result = await _sut.UpdateSettingsAsync(newSettings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task UpdateSettings_GetLastHashFailure_PropagatesFailure()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Dicom.PacsPort = 200;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string?>(ErrorCode.DatabaseError, "Hash query failed"));

        var result = await _sut.UpdateSettingsAsync(newSettings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── GetSettings failure on old settings load ─────────────────────────────

    [Fact]
    public async Task UpdateSettings_GetOldSettingsFailure_PropagatesFailure()
    {
        var newSettings = ValidSettings();

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<SystemSettings>(ErrorCode.DatabaseError, "Cannot load"));

        var result = await _sut.UpdateSettingsAsync(newSettings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── ExportAuditLog: entries with special characters ───────────────────────

    [Fact]
    public async Task ExportAuditLog_EntryWithNullHashes_WritesEmptyFields()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_null_{Guid.NewGuid()}.csv");
        var entries = new[]
        {
            new AuditEntry("1", DateTimeOffset.UtcNow, "U1", "ACTION", "details", null, null),
        };
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success((IReadOnlyList<AuditEntry>)entries));

        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);
            result.IsSuccess.Should().BeTrue();
            var content = await File.ReadAllTextAsync(tempPath);
            content.Should().Contain("ACTION");
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task ExportAuditLog_QueryFails_ReturnsFailure()
    {
        _auditRepo.QueryAsync(Arg.Any<AuditQueryFilter>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<AuditEntry>>(ErrorCode.DatabaseError, "Query failed"));

        var tempPath = Path.Combine(Path.GetTempPath(), $"audit_query_fail_{Guid.NewGuid()}.csv");
        try
        {
            var result = await _sut.ExportAuditLogAsync(tempPath);
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DatabaseError);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    // ── UpdateSettings: MaxFailedLogins change ───────────────────────────────

    [Fact]
    public async Task UpdateSettings_MaxFailedLoginsChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Security.MaxFailedLogins = 10;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Security.MaxFailedLogins");
        captured.Details.Should().Contain("5");   // old
        captured.Details.Should().Contain("10");  // new
    }

    // ── UpdateSettings: Security SessionTimeoutMinutes change ────────────────

    [Fact]
    public async Task UpdateSettings_SessionTimeoutChange_CapturedInAudit()
    {
        var oldSettings = ValidSettings();
        var newSettings = ValidSettings();
        newSettings.Security.SessionTimeoutMinutes = 60;

        _settingsRepo.GetAsync(Arg.Any<CancellationToken>()).Returns(Result.Success(oldSettings));
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>()).Returns(Result.Success());
        _auditRepo.GetLastHashAsync(Arg.Any<CancellationToken>()).Returns(Result.SuccessNullable<string?>(null));
        _securityContext.IsAuthenticated.Returns(false);

        AuditEntry? captured = null;
        _auditRepo.AppendAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success())
            .AndDoes(x => captured = x.Arg<AuditEntry>());

        await _sut.UpdateSettingsAsync(newSettings);

        captured.Should().NotBeNull();
        captured!.Details.Should().Contain("Security.SessionTimeoutMinutes");
        captured.Details.Should().Contain("15");  // old
        captured.Details.Should().Contain("60");  // new
    }
}
