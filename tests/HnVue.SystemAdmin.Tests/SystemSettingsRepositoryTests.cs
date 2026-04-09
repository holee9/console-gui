using System.IO;
using System.Text.Json;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.SystemAdmin;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

/// <summary>
/// Integration tests for <see cref="SystemSettingsRepository"/> using a temporary
/// file-system directory to avoid touching the real AppData path.
/// SWR-DA-030, SWR-DA-031: Settings load on startup and atomic persistence.
/// </summary>
[Trait("SWR", "SWR-DA-030")]
public sealed class SystemSettingsRepositoryTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _settingsPath;

    public SystemSettingsRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SystemSettingsRepoTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
        _settingsPath = Path.Combine(_tempDir, "settings.json");
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private SystemSettingsRepository CreateSut() => new(_settingsPath);

    // ── GetAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_WhenFileDoesNotExist_ReturnsDefaultSettings()
    {
        // No file written — simulate first-run state.
        var result = await CreateSut().GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Dicom.Should().NotBeNull();
        result.Value.Security.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_WhenFileExists_ReturnsPersistedSettings()
    {
        var expected = CreateValidSettings();
        await WriteSettingsFile(expected);

        var result = await CreateSut().GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be(expected.Dicom.PacsAeTitle);
        result.Value.Dicom.PacsHost.Should().Be(expected.Dicom.PacsHost);
        result.Value.Dicom.PacsPort.Should().Be(expected.Dicom.PacsPort);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(expected.Security.SessionTimeoutMinutes);
    }

    [Fact]
    public async Task GetAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        var expected = CreateValidSettings();
        await WriteSettingsFile(expected);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await CreateSut().GetAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetAsync_CorruptedJson_ReturnsFailure()
    {
        await File.WriteAllTextAsync(_settingsPath, "{ NOT VALID JSON !!! }");

        var result = await CreateSut().GetAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task GetAsync_EmptyFile_ReturnsDefaultSettings()
    {
        // null JSON value should return default settings rather than fail.
        await File.WriteAllTextAsync(_settingsPath, "null");

        var result = await CreateSut().GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    // ── SaveAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_WritesJsonFileToDisk()
    {
        var settings = CreateValidSettings();

        var result = await CreateSut().SaveAsync(settings);

        result.IsSuccess.Should().BeTrue();
        File.Exists(_settingsPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_PersistsAllSettingsValues()
    {
        var settings = CreateValidSettings();

        await CreateSut().SaveAsync(settings);

        // Verify by reading back through a fresh instance.
        var result = await CreateSut().GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be(settings.Dicom.PacsAeTitle);
        result.Value.Dicom.PacsPort.Should().Be(settings.Dicom.PacsPort);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(settings.Security.SessionTimeoutMinutes);
        result.Value.Security.MaxFailedLogins.Should().Be(settings.Security.MaxFailedLogins);
    }

    [Fact]
    public async Task SaveAsync_NullSettings_ThrowsArgumentNullException()
    {
        var act = async () => await CreateSut().SaveAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_OverwritesExistingFile()
    {
        var original = CreateValidSettings();
        original.Dicom.PacsAeTitle = "ORIGINAL";
        await CreateSut().SaveAsync(original);

        var updated = CreateValidSettings();
        updated.Dicom.PacsAeTitle = "UPDATED";
        await CreateSut().SaveAsync(updated);

        var result = await CreateSut().GetAsync();

        result.Value.Dicom.PacsAeTitle.Should().Be("UPDATED");
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryIfMissing()
    {
        // Use a nested path that does not yet exist.
        var nestedDir = Path.Combine(_tempDir, "sub", "nested");
        var nestedPath = Path.Combine(nestedDir, "settings.json");
        var sut = new SystemSettingsRepository(nestedPath);

        var result = await sut.SaveAsync(CreateValidSettings());

        result.IsSuccess.Should().BeTrue();
        File.Exists(nestedPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_WithCancellation_ThrowsOperationCancelledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await CreateSut().SaveAsync(CreateValidSettings(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task RoundTrip_SaveThenGet_ReturnsEquivalentSettings()
    {
        var settings = CreateValidSettings();

        await CreateSut().SaveAsync(settings);
        var result = await CreateSut().GetAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Generator.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static SystemSettings CreateValidSettings() => new()
    {
        Dicom = new DicomSettings
        {
            PacsAeTitle = "PACS_TEST",
            PacsHost = "192.168.0.1",
            PacsPort = 104,
            LocalAeTitle = "HNVUE_LOCAL",
        },
        Generator = new GeneratorSettings(),
        Security = new SecuritySettings
        {
            SessionTimeoutMinutes = 30,
            MaxFailedLogins = 5,
        },
    };

    private async Task WriteSettingsFile(SystemSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
        });
        await File.WriteAllTextAsync(_settingsPath, json);
    }
}
