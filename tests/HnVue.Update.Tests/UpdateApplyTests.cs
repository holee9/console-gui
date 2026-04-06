using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Update;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="UpdateRepository.ApplyPackageAsync"/>.
/// SWR-DA-042: staged update with SHA-256 verification, zip extraction, and pending_update.json marker.
/// </summary>
[Trait("SWR", "SWR-DA-042")]
public sealed class UpdateApplyTests : IDisposable
{
    // Each test gets its own isolated temp directory.
    private readonly string _tempRoot;
    private readonly string _updatesDir;

    public UpdateApplyTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"UpdateApply_{Guid.NewGuid()}");
        _updatesDir = Path.Combine(_tempRoot, "Updates");
        Directory.CreateDirectory(_tempRoot);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a valid zip archive at <paramref name="zipPath"/> containing a text entry.
    /// Returns the SHA-256 hex digest of the created archive.
    /// </summary>
    private static string CreateTestZip(
        string zipPath,
        string entryName = "update.txt",
        string content = "fake update content")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(zipPath)!);

        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry(entryName);
            using var writer = new StreamWriter(entry.Open());
            writer.Write(content);
        }

        byte[] bytes = File.ReadAllBytes(zipPath);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    /// <summary>Creates a companion <c>.sha256</c> sidecar file.</summary>
    private static void WriteSidecarHash(string zipPath, string hash)
        => File.WriteAllText(zipPath + ".sha256", hash, Encoding.UTF8);

    /// <summary>
    /// Creates an <see cref="UpdateRepository"/> rooted at <see cref="_tempRoot"/>,
    /// so all Updates/ sub-directories go to the isolated temp location.
    /// </summary>
    private UpdateRepository CreateSut() => new(_tempRoot);

    // ── file not found ────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_FileNotFound_ReturnsNotFound()
    {
        var sut = CreateSut();
        string missingPath = Path.Combine(_tempRoot, "nonexistent.zip");

        Result result = await sut.ApplyPackageAsync(missingPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
        result.ErrorMessage.Should().Contain(missingPath);
    }

    // ── hash verification ─────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_ValidZipWithCorrectHash_ReturnsSuccess()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        string hash = CreateTestZip(zipPath);
        WriteSidecarHash(zipPath, hash);

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyPackage_WrongHash_ReturnsPackageCorrupt()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        CreateTestZip(zipPath);
        WriteSidecarHash(zipPath, new string('a', 64)); // deliberately wrong hash

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
        result.ErrorMessage.Should().Contain("hash mismatch");
    }

    [Fact]
    public async Task ApplyPackage_NoSidecarFile_SkipsHashCheckAndSucceeds()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        CreateTestZip(zipPath);
        // No .sha256 sidecar — hash check must be skipped entirely.

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsSuccess.Should().BeTrue();
    }

    // ── corrupted zip ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_CorruptedZip_ReturnsPackageCorrupt()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        await File.WriteAllBytesAsync(zipPath, Encoding.UTF8.GetBytes("NOT A ZIP FILE"));

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.UpdatePackageCorrupt);
    }

    // ── staging directory ─────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_ValidZip_ExtractsContentsToStagingDirectory()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        CreateTestZip(zipPath, entryName: "update.txt", content: "hello from update");

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsSuccess.Should().BeTrue();

        string stagingDir = Path.Combine(_updatesDir, "Staging");
        Directory.Exists(stagingDir).Should().BeTrue("staging directory should be created");
        File.Exists(Path.Combine(stagingDir, "update.txt")).Should().BeTrue("zip contents should be extracted");
    }

    // ── pending_update.json marker ────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_ValidZip_WritesPendingUpdateMarker()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        CreateTestZip(zipPath);

        Result result = await CreateSut().ApplyPackageAsync(zipPath);

        result.IsSuccess.Should().BeTrue();

        string markerPath = Path.Combine(_updatesDir, "pending_update.json");
        File.Exists(markerPath).Should().BeTrue("pending_update.json marker should be written");

        string json = await File.ReadAllTextAsync(markerPath);
        json.Should().Contain("stagingPath");
        json.Should().Contain("packagePath");
        json.Should().Contain("stagedAt");
    }

    // ── null argument ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await CreateSut().ApplyPackageAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── cancellation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ApplyPackage_CancelledBeforeHashRead_ThrowsOperationCancelledException()
    {
        string zipPath = Path.Combine(_tempRoot, "HnVue-2.0.0.zip");
        string hash = CreateTestZip(zipPath);
        WriteSidecarHash(zipPath, hash);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await CreateSut().ApplyPackageAsync(zipPath, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
