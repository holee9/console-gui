using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.SystemAdmin;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

/// <summary>
/// Tests for <see cref="EfSystemSettingsRepository"/> using in-memory SQLite.
/// SWR-DA-030: System settings must be loaded at application startup.
/// SWR-DA-031: Settings changes must be persisted atomically.
/// </summary>
[Trait("SWR", "SWR-DA-030")]
[Trait("SWR", "SWR-DA-031")]
[Trait("Category", "SystemAdmin")]
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
                PacsAeTitle = "TEST_AE",
                PacsHost = "192.168.1.100",
                PacsPort = 104,
                LocalAeTitle = "LOCAL_AE",
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
                MaxFailedLogins = 5,
            },
        };

    // ── GetAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_NoSettingsSaved_ReturnsDefaultSettings()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);

        // Act
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Dicom.PacsAeTitle.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAsync_AfterSave_ReturnsSavedSettings()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);
        var settings = CreateSampleSettings();

        // Arrange
        await repo.SaveAsync(settings);

        // Act
        var result = await repo.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("TEST_AE");
        result.Value.Dicom.PacsHost.Should().Be("192.168.1.100");
        result.Value.Dicom.PacsPort.Should().Be(104);
        result.Value.Dicom.LocalAeTitle.Should().Be("LOCAL_AE");
        result.Value.Generator.ComPort.Should().Be("COM3");
        result.Value.Generator.BaudRate.Should().Be(9600);
        result.Value.Generator.TimeoutMs.Should().Be(5000);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(30);
        result.Value.Security.MaxFailedLogins.Should().Be(5);
    }

    // ── SaveAsync ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_NullSettings_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.SaveAsync(null!));
    }

    [Fact]
    public async Task SaveAsync_NewSettings_ReturnsSuccess()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);
        var settings = CreateSampleSettings();

        // Act
        var result = await repo.SaveAsync(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_UpdateExisting_ReturnsSuccess()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);

        // Arrange - Save initial settings
        await repo.SaveAsync(CreateSampleSettings());

        // Act - Update settings
        var updated = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "UPDATED_AE",
                PacsHost = "10.0.0.1",
                PacsPort = 11112,
                LocalAeTitle = "UPD_LOCAL",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM5",
                BaudRate = 115200,
                TimeoutMs = 10000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 60,
                MaxFailedLogins = 10,
            },
        };
        var result = await repo.SaveAsync(updated);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var loaded = await repo.GetAsync();
        loaded.Value.Dicom.PacsAeTitle.Should().Be("UPDATED_AE");
        loaded.Value.Generator.BaudRate.Should().Be(115200);
        loaded.Value.Security.MaxFailedLogins.Should().Be(10);
    }

    [Fact]
    public async Task SaveAsync_MultipleUpdates_OnlyLatestPersists()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfSystemSettingsRepository(ctx);

        // Arrange - Save multiple times
        var settings1 = CreateSampleSettings();
        settings1.Dicom.PacsAeTitle = "FIRST";
        await repo.SaveAsync(settings1);

        var settings2 = CreateSampleSettings();
        settings2.Dicom.PacsAeTitle = "SECOND";
        await repo.SaveAsync(settings2);

        // Act
        var result = await repo.GetAsync();

        // Assert - Latest value persists
        result.Value.Dicom.PacsAeTitle.Should().Be("SECOND");
    }
}
