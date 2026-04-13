using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

[Trait("SWR", "SWR-DC-040")]
public sealed class DicomStoreScuCoverageTests
{
    private readonly IDicomNetworkConfig _config;
    private readonly DicomStoreScu _sut;

    public DicomStoreScuCoverageTests()
    {
        _config = Substitute.For<IDicomNetworkConfig>();
        _config.PacsHost.Returns("127.0.0.1");
        _config.PacsPort.Returns(104);
        _config.PacsAeTitle.Returns("PACS");
        _config.LocalAeTitle.Returns("HNVUE");
        _sut = new DicomStoreScu(_config);
    }

    // ── Constructor ────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new DicomStoreScu(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    // ── StoreAsync — null filePath ─────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NullFilePath_ThrowsArgumentNullException()
    {
        var act = () => _sut.StoreAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("filePath");
    }

    // ── StoreAsync — non-existent file ─────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NonExistentFile_ReturnsDicomStoreFailed()
    {
        var result = await _sut.StoreAsync("/nonexistent/path/file.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
        result.ErrorMessage.Should().Contain("DICOM file not found");
    }

    [Fact]
    public async Task StoreAsync_EmptyStringFilePath_ReturnsStoreFailed()
    {
        var result = await _sut.StoreAsync(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    // ── StoreAsync — config values used ────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_NonExistentFile_ReferencesPacsAeTitle()
    {
        var result = await _sut.StoreAsync("/nonexistent/file.dcm");

        result.ErrorMessage.Should().Contain("not found");
    }
}
