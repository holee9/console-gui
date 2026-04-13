using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfSystemSettingsRepository"/> using an in-memory EF Core database.
/// REQ-COORD-005: SPEC-COORDINATOR-001 EF Core system settings persistence.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfSystemSettingsRepositoryTests
{
    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    private static SystemSettings CreateSampleSettings() =>
        new()
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS",
                PacsHost = "192.168.1.100",
                PacsPort = 104,
                LocalAeTitle = "HNVUE"
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM1",
                BaudRate = 9600,
                TimeoutMs = 5000
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLogins = 3
            }
        };

    // ── GetAsync ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ExistingSettings_ReturnsSettings()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);

        // Arrange
        var settings = CreateSampleSettings();
        await repo.SaveAsync(settings);

        // Act
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("PACS");
        result.Value.Dicom.PacsHost.Should().Be("192.168.1.100");
        result.Value.Dicom.PacsPort.Should().Be(104);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(30);
    }

    [Fact]
    public async Task GetAsync_NoSettings_ReturnsDefaults()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);

        // Act
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.LocalAeTitle.Should().Be("HNVUE");
        result.Value.Security.SessionTimeoutMinutes.Should().Be(15);
        result.Value.Security.MaxFailedLogins.Should().Be(5);
    }

    // ── SaveAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_NewSettings_InsertsRecord()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);
        var settings = CreateSampleSettings();

        // Act
        var result = await repo.SaveAsync(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify insertion
        var saved = await ctx.SystemSettings.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.PacsAeTitle.Should().Be("PACS");
    }

    [Fact]
    public async Task SaveAsync_ExistingSettings_UpdatesRecord()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);

        // Arrange - Save initial settings
        var initialSettings = CreateSampleSettings();
        await repo.SaveAsync(initialSettings);

        // Act - Update settings
        var updatedSettings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS-UPDATED",
                PacsHost = "192.168.1.200",
                PacsPort = 111,
                LocalAeTitle = "HNVUE-V2"
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM2",
                BaudRate = 19200,
                TimeoutMs = 10000
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 60,
                MaxFailedLogins = 5
            }
        };
        var result = await repo.SaveAsync(updatedSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify update - should still be only one record
        var count = await ctx.SystemSettings.CountAsync();
        count.Should().Be(1);

        var saved = await ctx.SystemSettings.FirstOrDefaultAsync();
        saved.Should().NotBeNull();
        saved!.PacsAeTitle.Should().Be("PACS-UPDATED");
    }

    [Fact]
    public async Task SaveAsync_NullSettings_ThrowsArgumentNullException()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SaveAsync(null!));
    }

    [Fact]
    public async Task SaveAsync_PreservesAllSettings()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);
        var settings = CreateSampleSettings();

        // Act
        await repo.SaveAsync(settings);
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("PACS");
        result.Value.Dicom.PacsHost.Should().Be("192.168.1.100");
        result.Value.Dicom.PacsPort.Should().Be(104);
        result.Value.Dicom.LocalAeTitle.Should().Be("HNVUE");
        result.Value.Generator.ComPort.Should().Be("COM1");
        result.Value.Generator.BaudRate.Should().Be(9600);
        result.Value.Generator.TimeoutMs.Should().Be(5000);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(30);
        result.Value.Security.MaxFailedLogins.Should().Be(3);
    }

    [Fact]
    public async Task RoundTrip_DefaultSettingsToDatabaseAndBack()
    {
        await using var (ctx, connection) = CreateSqliteContext();
        var repo = new EfSystemSettingsRepository(ctx);

        // Arrange - Get defaults
        var defaultsResult = await repo.GetAsync();
        var defaults = defaultsResult.Value;

        // Act - Save and retrieve
        await repo.SaveAsync(defaults);
        var retrievedResult = await repo.GetAsync();

        // Assert
        retrievedResult.IsSuccess.Should().BeTrue();
        retrievedResult.Value.Dicom.LocalAeTitle.Should().Be(defaults.Dicom.LocalAeTitle);
        retrievedResult.Value.Security.SessionTimeoutMinutes.Should().Be(defaults.Security.SessionTimeoutMinutes);
    }
}
