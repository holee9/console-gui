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
        _sut = new SystemAdminService(_settingsRepo, _auditRepo);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullSettingsRepo_ThrowsArgumentNullException()
    {
        var act = () => new SystemAdminService(null!, _auditRepo);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_NullAuditRepo_ThrowsArgumentNullException()
    {
        var act = () => new SystemAdminService(_settingsRepo, null!);

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
        _settingsRepo.SaveAsync(Arg.Any<SystemSettings>(), Arg.Any<CancellationToken>())
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
    }

    [Fact]
    public async Task UpdateSettings_EmptyLocalAeTitle_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Dicom.LocalAeTitle = "";

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task UpdateSettings_ZeroSessionTimeout_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Security.SessionTimeoutMinutes = 0;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task UpdateSettings_ZeroMaxFailedLogins_ReturnsValidationFailure()
    {
        var settings = ValidSettings();
        settings.Security.MaxFailedLogins = 0;

        var result = await _sut.UpdateSettingsAsync(settings);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
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
}
