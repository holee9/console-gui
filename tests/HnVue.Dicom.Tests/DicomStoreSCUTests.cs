using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using NSubstitute;
using Xunit;

namespace HnVue.Dicom.Tests;

[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomStoreSCUTests
{
    private readonly IDicomNetworkConfig _config;
    private readonly DicomStoreScu _sut;

    public DicomStoreSCUTests()
    {
        _config = Substitute.For<IDicomNetworkConfig>();
        _config.PacsHost.Returns("localhost");
        _config.PacsPort.Returns(11112);
        _config.PacsAeTitle.Returns("PACS");
        _config.LocalAeTitle.Returns("HNVUE");
        _config.MwlHost.Returns("localhost");
        _config.MwlPort.Returns(11113);

        _sut = new DicomStoreScu(_config);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new DicomStoreScu(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    // ── StoreAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Store_NonExistentFile_ReturnsDicomStoreFailed()
    {
        var result = await _sut.StoreAsync("C:/nonexistent/test.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DicomStoreFailed);
    }

    [Fact]
    public async Task Store_NullFilePath_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.StoreAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Store_NotADicomFile_ReturnsDicomStoreFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "not a dicom file");

            var result = await _sut.StoreAsync(tempFile);

            // Should fail with DicomStoreFailed (cannot parse as DICOM, or connection fails)
            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
