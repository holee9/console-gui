using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.SystemAdmin.Tests;

/// <summary>
/// Comprehensive tests for <see cref="SystemSettingsRepository"/>.
/// Tests file-based JSON persistence at %AppData%\HnVue\settings.json.
/// </summary>
/// <remarks>
/// SWR-DA-030: System settings must be loaded at application startup.
/// SWR-DA-031: Settings changes must be persisted atomically.
/// </remarks>
[Trait("SWR", "SWR-DA-030")]
[Trait("SWR", "SWR-DA-031")]
public sealed class SystemSettingsRepositoryTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _settingsPath;

    public SystemSettingsRepositoryTests()
    {
        // Create isolated temp directory for each test
        _tempRoot = Path.Combine(Path.GetTempPath(), $"SystemSettingsRepo_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempRoot);
        _settingsPath = Path.Combine(_tempRoot, "settings.json");
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Temp files may be locked on Windows; ignore cleanup failures
            Debug.WriteLine($"Failed to cleanup temp directory: {_tempRoot} - {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a testable repository that uses the temp directory instead of %AppData%.
    /// </summary>
    private TestableSystemSettingsRepository CreateSut()
        => new(_settingsPath);

    // ─── GetAsync Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_FileDoesNotExist_ReturnsDefaultSystemSettings()
    {
        // Arrange
        var sut = CreateSut();
        // File does not exist

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Dicom.Should().NotBeNull();
        result.Value.Generator.Should().NotBeNull();
        result.Value.Security.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAsync_FileExistsWithValidJson_ReturnsDeserializedSettings()
    {
        // Arrange
        var expectedSettings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "PACS_TEST",
                PacsHost = "192.168.1.50",
                PacsPort = 104,
                LocalAeTitle = "HNVUE_TEST",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM1",
                BaudRate = 9600,
                TimeoutMs = 5000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 30,
                MaxFailedLogins = 3,
            },
        };

        var json = JsonSerializer.Serialize(expectedSettings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);

        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("PACS_TEST");
        result.Value.Dicom.PacsHost.Should().Be("192.168.1.50");
        result.Value.Dicom.PacsPort.Should().Be(104);
        result.Value.Dicom.LocalAeTitle.Should().Be("HNVUE_TEST");
        result.Value.Generator.ComPort.Should().Be("COM1");
        result.Value.Generator.BaudRate.Should().Be(9600);
        result.Value.Generator.TimeoutMs.Should().Be(5000);
        result.Value.Security.SessionTimeoutMinutes.Should().Be(30);
        result.Value.Security.MaxFailedLogins.Should().Be(3);
    }

    [Fact]
    public async Task GetAsync_FileExistsWithInvalidJson_ReturnsFileOperationFailed()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsPath, "{ invalid json content", Encoding.UTF8);
        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.FileOperationFailed);
        result.ErrorMessage.Should().Contain("Failed to load system settings");
    }

    [Fact]
    public async Task GetAsync_FileExistsWithEmptyJson_ReturnsDefaultSettings()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsPath, "{}", Encoding.UTF8);
        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SystemSettings());
    }

    [Fact]
    public async Task GetAsync_FileExistsWithPartialJson_ReturnsPartialSettings()
    {
        // Arrange
        var partialJson = """
            {
                "Dicom": {
                    "PacsAeTitle": "PARTIAL",
                    "PacsHost": "10.0.0.1",
                    "PacsPort": 11112,
                    "LocalAeTitle": "STATION1"
                }
            }
            """;
        await File.WriteAllTextAsync(_settingsPath, partialJson, Encoding.UTF8);
        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("PARTIAL");
        result.Value.Dicom.PacsHost.Should().Be("10.0.0.1");
        result.Value.Dicom.PacsPort.Should().Be(11112);
        result.Value.Dicom.LocalAeTitle.Should().Be("STATION1");
        // Other properties should have default values
        result.Value.Generator.ComPort.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid json!@#$")]
    [InlineData("{ \"Dicom\": missing close brace")]
    public async Task GetAsync_InvalidJsonContent_ReturnsFileOperationFailed(string invalidContent)
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsPath, invalidContent, Encoding.UTF8);
        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.FileOperationFailed);
    }

    // ─── SaveAsync Tests ────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidSettings_WritesToFileAndSucceeds()
    {
        // Arrange
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "SAVE_TEST",
                PacsHost = "10.20.30.40",
                PacsPort = 104,
                LocalAeTitle = "SAVE_STATION",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM2",
                BaudRate = 115200,
                TimeoutMs = 3000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 60,
                MaxFailedLogins = 10,
            },
        };
        var sut = CreateSut();

        // Act
        Result result = await sut.SaveAsync(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(_settingsPath).Should().BeTrue();

        var json = await File.ReadAllTextAsync(_settingsPath);
        var deserialized = JsonSerializer.Deserialize<SystemSettings>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        deserialized!.Dicom.PacsAeTitle.Should().Be("SAVE_TEST");
        deserialized.Dicom.PacsHost.Should().Be("10.20.30.40");
        deserialized.Dicom.PacsPort.Should().Be(104);
        deserialized.Dicom.LocalAeTitle.Should().Be("SAVE_STATION");
        deserialized.Generator.ComPort.Should().Be("COM2");
        deserialized.Generator.BaudRate.Should().Be(115200);
        deserialized.Generator.TimeoutMs.Should().Be(3000);
        deserialized.Security.SessionTimeoutMinutes.Should().Be(60);
        deserialized.Security.MaxFailedLogins.Should().Be(10);
    }

    [Fact]
    public async Task SaveAsync_NullSettings_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var act = async () => await sut.SaveAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_AtomicWrite_TempFileThenMove_OverwritesExisting()
    {
        // Arrange
        var originalSettings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsAeTitle = "ORIGINAL" }
        };
        var updatedSettings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsAeTitle = "UPDATED" }
        };

        // Write original settings
        var json = JsonSerializer.Serialize(originalSettings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsPath, json);

        var sut = CreateSut();
        var tempPath = _settingsPath + ".tmp";

        // Act
        Result result = await sut.SaveAsync(updatedSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(_settingsPath).Should().BeTrue();
        File.Exists(tempPath).Should().BeFalse("Temp file should be cleaned up after move");

        var finalJson = await File.ReadAllTextAsync(_settingsPath);
        var finalSettings = JsonSerializer.Deserialize<SystemSettings>(finalJson, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        finalSettings!.Dicom.PacsAeTitle.Should().Be("UPDATED");
    }

    [Fact]
    public async Task SaveAsync_CreatesDirectoryWhenNotExists()
    {
        // Arrange
        var nestedPath = Path.Combine(_tempRoot, "SubDir", "Nested", "settings.json");
        var sut = new TestableSystemSettingsRepository(nestedPath);
        var settings = new SystemSettings();

        // Act
        Result result = await sut.SaveAsync(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(nestedPath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_ThenGetAsync_RoundTripPreservesData()
    {
        // Arrange
        var originalSettings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "ROUNDTRIP",
                PacsHost = "172.16.0.1",
                PacsPort = 11112,
                LocalAeTitle = "RT_STATION",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM3",
                BaudRate = 19200,
                TimeoutMs = 10000,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 120,
                MaxFailedLogins = 5,
            },
        };
        var sut = CreateSut();

        // Act
        var saveResult = await sut.SaveAsync(originalSettings);
        var getResult = await sut.GetAsync();

        // Assert
        saveResult.IsSuccess.Should().BeTrue();
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().BeEquivalentTo(originalSettings);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(65535)]
    public async Task SaveAsync_ValidPortRanges_PreservesCorrectly(int port)
    {
        // Arrange
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsPort = port }
        };
        var sut = CreateSut();

        // Act
        await sut.SaveAsync(settings);
        var result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsPort.Should().Be(port);
    }

    [Fact]
    public async Task SaveAsync_MultipleSaves_OnlyLatestFileExists()
    {
        // Arrange
        var settings1 = new SystemSettings { Dicom = new DicomSettings { PacsAeTitle = "V1" } };
        var settings2 = new SystemSettings { Dicom = new DicomSettings { PacsAeTitle = "V2" } };
        var settings3 = new SystemSettings { Dicom = new DicomSettings { PacsAeTitle = "V3" } };
        var sut = CreateSut();

        // Act
        await sut.SaveAsync(settings1);
        await sut.SaveAsync(settings2);
        await sut.SaveAsync(settings3);

        // Assert
        var result = await sut.GetAsync();
        result.IsSuccess.Should().BeTrue();
        result.Value.Dicom.PacsAeTitle.Should().Be("V3");
        File.Exists(_settingsPath + ".tmp").Should().BeFalse("Temp files should be cleaned up");
    }

    // ─── Error Handling Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsPath, "{}");
        var sut = CreateSut();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await sut.GetAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task SaveAsync_CancellationToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var sut = CreateSut();
        var settings = new SystemSettings();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await sut.SaveAsync(settings, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ─── JSON Formatting Tests ─────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_WritesIndentedJson()
    {
        // Arrange
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsAeTitle = "FORMAT_TEST" }
        };
        var sut = CreateSut();

        // Act
        await sut.SaveAsync(settings);
        var json = await File.ReadAllTextAsync(_settingsPath);

        // Assert
        // Indented JSON should contain newlines and indentation
        json.Should().Contain("\n");
        json.Should().Contain("  "); // Indentation spaces
    }

    [Fact]
    public async Task SaveAsync_PreservesAllProperties_WithWebDefaults()
    {
        // Arrange
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = "FULL_TEST",
                PacsHost = "test.host.com",
                PacsPort = 104,
                LocalAeTitle = "LOCAL_AE",
            },
            Generator = new GeneratorSettings
            {
                ComPort = "COM4",
                BaudRate = 38400,
                TimeoutMs = 7500,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = 45,
                MaxFailedLogins = 7,
            },
        };
        var sut = CreateSut();

        // Act
        await sut.SaveAsync(settings);
        var result = await sut.GetAsync();

        // Assert
        result.Value.Should().BeEquivalentTo(settings);
    }

    // ─── Edge Cases ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_EmptySettingsObject_WritesValidJson()
    {
        // Arrange
        var settings = new SystemSettings();
        var sut = CreateSut();

        // Act
        await sut.SaveAsync(settings);
        var result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(settings);
    }

    [Fact]
    public async Task GetAsync_FileWithNullContent_ReturnsDefaultSettings()
    {
        // Arrange
        await File.WriteAllTextAsync(_settingsPath, "null", Encoding.UTF8);
        var sut = CreateSut();

        // Act
        Result<SystemSettings> result = await sut.GetAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new SystemSettings());
    }

    [Fact]
    public async Task SaveAsync_OverwriteExistingFile_TempFileMovedAtomically()
    {
        // Arrange
        var originalJson = """
            {
                "Dicom": {
                    "PacsAeTitle": "OLD"
                }
            }
            """;
        await File.WriteAllTextAsync(_settingsPath, originalJson);

        var newSettings = new SystemSettings
        {
            Dicom = new DicomSettings { PacsAeTitle = "NEW" }
        };
        var sut = CreateSut();

        // Act
        var result = await sut.SaveAsync(newSettings);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var finalJson = await File.ReadAllTextAsync(_settingsPath);
        finalJson.Should().Contain("NEW");
        finalJson.Should().NotContain("OLD");
    }

    [Fact]
    public async Task SaveAsync_LargeSettingsObject_WritesSuccessfully()
    {
        // Arrange
        var settings = new SystemSettings
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = new string('A', 1000), // Large string
                PacsHost = new string('B', 1000),
            }
        };
        var sut = CreateSut();

        // Act
        var result = await sut.SaveAsync(settings);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var getResult = await sut.GetAsync();
        getResult.Value.Dicom.PacsAeTitle.Should().HaveLength(1000);
        getResult.Value.Dicom.PacsHost.Should().HaveLength(1000);
    }
}

/// <summary>
/// Testable subclass of SystemSettingsRepository that overrides the hardcoded path.
/// This allows us to test with isolated temp directories instead of %AppData%.
/// </summary>
internal sealed class TestableSystemSettingsRepository(string testPath) : SystemSettingsRepository
{
    protected override string GetStorePath() => testPath;
}
