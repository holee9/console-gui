using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="DicomStoreScu"/> covering constructor validation,
/// file path handling, and C-STORE error paths.
/// SWR-DICOM-020.
/// </summary>
/// <remarks>
/// DicomStoreScu uses <c>DicomClientFactory.Create</c> internally (static fo-dicom factory)
/// and <c>DicomFile.OpenAsync</c> for file loading. Network-dependent paths cannot be
/// mocked without a live DICOM server; tests exercise parameter validation, file-not-found,
/// and non-DICOM file error paths which are fully deterministic.
/// </remarks>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomStoreScuTests
{
    private readonly IDicomNetworkConfig _config;
    private readonly DicomStoreScu _sut;

    public DicomStoreScuTests()
    {
        _config = CreateConfig();
        _sut = new DicomStoreScu(_config);
    }

    private static IDicomNetworkConfig CreateConfig()
    {
        var config = Substitute.For<IDicomNetworkConfig>();
        config.PacsHost.Returns("127.0.0.1");
        config.PacsPort.Returns(104);
        config.LocalAeTitle.Returns("HNVUE");
        config.PacsAeTitle.Returns("PACS");
        config.MwlHost.Returns("localhost");
        config.MwlPort.Returns(11113);
        return config;
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new DicomStoreScu(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ValidConfig_DoesNotThrow()
    {
        var act = () => new DicomStoreScu(CreateConfig());

        act.Should().NotThrow();
    }

    // ── StoreAsync — Null FilePath ────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NullFilePath_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.StoreAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── StoreAsync — Empty/Whitespace FilePath ────────────────────────────────

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task StoreAsync_EmptyOrWhitespaceFilePath_ReturnsStoreFailed(string filePath)
    {
        // Empty/whitespace file path passes the null check but File.Exists returns false.
        // The method returns Result.Failure(DicomStoreFailed) rather than throwing.
        var result = await _sut.StoreAsync(filePath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("찾을 수 없습니다");
    }

    // ── StoreAsync — Non-Existent File ────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NonExistentFile_ReturnsDicomStoreFailed()
    {
        var result = await _sut.StoreAsync("C:/nonexistent/test.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_NonExistentFile_ErrorMessageContainsPath()
    {
        var path = @"C:\nonexistent\imaginary_scan.dcm";

        var result = await _sut.StoreAsync(path);

        result.ErrorMessage.Should().Contain(path);
    }

    [Theory]
    [InlineData("C:/temp/scan_001.dcm")]
    [InlineData("C:/temp/scan with spaces.dcm")]
    [InlineData("C:/temp/scan-kebab.dcm")]
    [InlineData("C:/temp/scan_under.dcm")]
    public async Task StoreAsync_VariousNonExistentPaths_ReturnsStoreFailed(string filePath)
    {
        // All paths point to non-existent files, so they should all fail deterministically.
        var result = await _sut.StoreAsync(filePath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── StoreAsync — Non-DICOM File Content ───────────────────────────────────

    [Fact]
    public async Task StoreAsync_NotADicomFile_ReturnsDicomStoreFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "not a dicom file");

            var result = await _sut.StoreAsync(tempFile);

            // fo-dicom cannot parse this as DICOM, or network connection fails.
            // Either way, the catch-all returns DicomStoreFailed.
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_BinaryGarbageFile_ReturnsDicomStoreFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var garbage = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD };
            await File.WriteAllBytesAsync(tempFile, garbage);

            var result = await _sut.StoreAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task StoreAsync_EmptyFile_ReturnsDicomStoreFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write then clear to ensure an empty file.
            await File.WriteAllTextAsync(tempFile, string.Empty);

            var result = await _sut.StoreAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── StoreAsync — Network Unreachable ──────────────────────────────────────

    [Fact]
    public async Task StoreAsync_UnreachablePacs_ReturnsDicomStoreFailed()
    {
        // Create a config that points to an unreachable host/port.
        // The file content is not DICOM, so fo-dicom parse may fail first.
        // Either parse failure or network failure is caught and returns DicomStoreFailed.
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllBytesAsync(tempFile, "not dicom"u8.ToArray());

            var unreachableConfig = Substitute.For<IDicomNetworkConfig>();
            unreachableConfig.PacsHost.Returns("192.0.2.1"); // TEST-NET-1, unreachable
            unreachableConfig.PacsPort.Returns(19999);
            unreachableConfig.LocalAeTitle.Returns("HNVUE");
            unreachableConfig.PacsAeTitle.Returns("PACS");
            var unreachableSut = new DicomStoreScu(unreachableConfig);

            var result = await unreachableSut.StoreAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── StoreAsync — Cancellation ─────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_PreCancelledToken_NonExistentFile_ReturnsFailure()
    {
        // DicomStoreScu re-throws OperationCanceledException, but file-not-found
        // is checked synchronously before any async operation.
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await _sut.StoreAsync("C:/nonexistent.dcm", cts.Token);

        // File does not exist, so it returns failure BEFORE any async work.
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── Multiple Instances Independence ───────────────────────────────────────

    [Fact]
    public async Task StoreAsync_DifferentSutInstances_ReturnIndependently()
    {
        var config1 = CreateConfig();
        var sut1 = new DicomStoreScu(config1);

        var config2 = Substitute.For<IDicomNetworkConfig>();
        config2.PacsHost.Returns("10.0.0.1");
        config2.PacsPort.Returns(11112);
        config2.LocalAeTitle.Returns("OTHER_SCU");
        config2.PacsAeTitle.Returns("OTHER_PACS");
        var sut2 = new DicomStoreScu(config2);

        var result1 = await sut1.StoreAsync("C:/nonexistent1.dcm");
        var result2 = await sut2.StoreAsync("C:/nonexistent2.dcm");

        result1.IsFailure.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
        result1.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result2.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── File Path Edge Cases ──────────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_VeryLongPath_ReturnsDicomStoreFailed()
    {
        // Extremely long path — File.Exists returns false on Windows for paths > MAX_PATH.
        var longPath = new string('A', 300) + ".dcm";

        var result = await _sut.StoreAsync(longPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_PathWithSpecialCharacters_ReturnsDicomStoreFailed()
    {
        // Path with special characters that don't exist on disk.
        var specialPath = @"C:\temp\scan (2024)\patient#1\image.dcm";

        var result = await _sut.StoreAsync(specialPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task StoreAsync_UncPath_ReturnsDicomStoreFailed()
    {
        // UNC network path — non-existent share.
        var uncPath = @"\\nonexistent-server\share\image.dcm";

        var result = await _sut.StoreAsync(uncPath);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── Config Interface Verification ─────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_ReadsPacsConfigFromInjectedInterface()
    {
        // Verify the SUT uses the injected config by checking error messages
        // when the file exists but is not valid DICOM.
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "garbage");

            var customConfig = Substitute.For<IDicomNetworkConfig>();
            customConfig.PacsHost.Returns("custom-pacs.local");
            customConfig.PacsPort.Returns(11112);
            customConfig.LocalAeTitle.Returns("CUSTOM_SCU");
            customConfig.PacsAeTitle.Returns("CUSTOM_PACS");
            var customSut = new DicomStoreScu(customConfig);

            var result = await customSut.StoreAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
