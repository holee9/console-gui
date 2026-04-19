using FluentAssertions;
using HnVue.CDBurning;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.CDBurning.Tests;

/// <summary>
/// Tests for disc media status checking and capacity verification.
/// </summary>
[Trait("SWR", "SWR-CD-020")]
public sealed class DiscMediaStatusTests
{
    private readonly IMAPIComWrapper _sut = new();

    [Fact]
    public async Task GetDiscCapacity_DefaultCdCapacity_Is700Mb()
    {
        var result = await _sut.GetDiscCapacityBytesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(700L * 1024 * 1024);
    }

    [Fact]
    public async Task SimulateDiscInserted_DvdCapacity_Returns4Gb()
    {
        const long dvdCapacity = 4_700_000_000L;
        _sut.SimulateDiscInserted(blank: true, capacityBytes: dvdCapacity);

        var result = await _sut.GetDiscCapacityBytesAsync();

        result.Value.Should().Be(dvdCapacity);
    }

    [Fact]
    public async Task BurnFiles_ExactlyAtCapacity_Succeeds()
    {
        const long capacity = 1000;
        _sut.SimulateDiscInserted(blank: true, capacityBytes: capacity);

        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", capacity) };

        var result = await _sut.BurnFilesAsync(files, "EXACT");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BurnFiles_OneByteOverCapacity_ReturnsBurnFailed()
    {
        const long capacity = 1000;
        _sut.SimulateDiscInserted(blank: true, capacityBytes: capacity);

        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", capacity + 1) };

        var result = await _sut.BurnFilesAsync(files, "OVER");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnFiles_MultipleFiles_SumExceedsCapacity_ReturnsBurnFailed()
    {
        const long capacity = 1000;
        _sut.SimulateDiscInserted(blank: true, capacityBytes: capacity);

        var files = new[]
        {
            new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 600),
            new BurnFileEntry("src/b.dcm", "DICOM\\b.dcm", 500), // Total = 1100 > 1000
        };

        var result = await _sut.BurnFilesAsync(files, "MULTI_OVER");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.BurnFailed);
    }

    [Fact]
    public async Task BurnFiles_MultipleFiles_WithinCapacity_Succeeds()
    {
        const long capacity = 1000;
        _sut.SimulateDiscInserted(blank: true, capacityBytes: capacity);

        var files = new[]
        {
            new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 400),
            new BurnFileEntry("src/b.dcm", "DICOM\\b.dcm", 300),
            new BurnFileEntry("src/c.dcm", "DICOM\\c.dcm", 300), // Total = 1000
        };

        var result = await _sut.BurnFilesAsync(files, "MULTI_OK");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task BurnFiles_AfterBurn_DiscIsMarkedNotBlank()
    {
        _sut.SimulateDiscInserted(blank: true);
        var files = new[] { new BurnFileEntry("src/a.dcm", "DICOM\\a.dcm", 100) };

        await _sut.BurnFilesAsync(files, "TEST");
        var blankResult = await _sut.IsDiscBlankAsync();

        blankResult.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Verify_DiscNotInserted_ReturnsVerificationFailed()
    {
        // No SimulateDiscInserted call → no disc
        var result = await _sut.VerifyAsync(Array.Empty<BurnFileEntry>());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DiscVerificationFailed);
    }
}
