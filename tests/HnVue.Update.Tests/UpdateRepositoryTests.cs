using System.IO;
using System.IO.Compression;
using System.Text.Json;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="UpdateRepository"/> package discovery and metadata loading.
/// </summary>
public sealed class UpdateRepositoryTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly string _updatesDir;

    public UpdateRepositoryTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"UpdateRepositoryTests_{Guid.NewGuid():N}");
        _updatesDir = Path.Combine(_tempRoot, "Updates");
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    private UpdateRepository CreateSut() => new(_tempRoot);

    private string CreatePackage(string version, string content = "update payload")
    {
        Directory.CreateDirectory(_updatesDir);
        string path = Path.Combine(_updatesDir, $"HnVue-{version}.zip");

        using var archive = ZipFile.Open(path, ZipArchiveMode.Create);
        var entry = archive.CreateEntry("payload.txt");
        using var writer = new StreamWriter(entry.Open());
        writer.Write(content);

        return path;
    }

    [Fact]
    public async Task CheckForUpdateAsync_MissingUpdatesDirectory_ReturnsSuccessWithNull()
    {
        Result<UpdateInfo?> result = await CreateSut().CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_EmptyUpdatesDirectory_ReturnsSuccessWithNull()
    {
        Directory.CreateDirectory(_updatesDir);

        Result<UpdateInfo?> result = await CreateSut().CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task CheckForUpdateAsync_MultiplePackages_ReturnsNewestVersion()
    {
        CreatePackage("1.2.0");
        string newestPath = CreatePackage("2.5.1");
        CreatePackage("2.4.9");

        Result<UpdateInfo?> result = await CreateSut().CheckForUpdateAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Version.Should().Be("2.5.1");
        result.Value.PackageUrl.Should().Be(newestPath);
        result.Value.Sha256Hash.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPackageInfoAsync_MetadataFilePresent_ReturnsDeserializedMetadata()
    {
        string packagePath = CreatePackage("3.0.0");
        var metadata = new UpdateInfo(
            Version: "3.0.0",
            ReleaseNotes: "Validated release",
            PackageUrl: "https://updates.example/3.0.0.zip",
            Sha256Hash: "ABC123");

        string jsonPath = Path.ChangeExtension(packagePath, ".json");
        await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(metadata));

        Result<UpdateInfo> result = await CreateSut().GetPackageInfoAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("3.0.0");
        result.Value.ReleaseNotes.Should().Be("Validated release");
        result.Value.PackageUrl.Should().Be("https://updates.example/3.0.0.zip");
        result.Value.Sha256Hash.Should().Be("ABC123");
    }

    [Fact]
    public async Task GetPackageInfoAsync_MetadataFileMissing_FallsBackToFilename()
    {
        string packagePath = CreatePackage("4.1.2");

        Result<UpdateInfo> result = await CreateSut().GetPackageInfoAsync(packagePath);

        result.IsSuccess.Should().BeTrue();
        result.Value.Version.Should().Be("4.1.2");
        result.Value.ReleaseNotes.Should().BeNull();
        result.Value.PackageUrl.Should().Be(packagePath);
        result.Value.Sha256Hash.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPackageInfoAsync_MissingPackage_ReturnsNotFound()
    {
        string missingPath = Path.Combine(_updatesDir, "HnVue-9.9.9.zip");

        Result<UpdateInfo> result = await CreateSut().GetPackageInfoAsync(missingPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
        result.ErrorMessage.Should().Contain(missingPath);
    }

    [Fact]
    public async Task GetPackageInfoAsync_InvalidMetadata_ReturnsPackageCorrupt()
    {
        string packagePath = CreatePackage("5.0.0");
        string jsonPath = Path.ChangeExtension(packagePath, ".json");
        await File.WriteAllTextAsync(jsonPath, "{ invalid json");

        Result<UpdateInfo> result = await CreateSut().GetPackageInfoAsync(packagePath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
        result.ErrorMessage.Should().Contain("Failed to read package metadata");
    }

    [Fact]
    public async Task GetPackageInfoAsync_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await CreateSut().GetPackageInfoAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
