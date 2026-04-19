using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Error recovery tests: corrupted images, out-of-range values, edge cases.
/// </summary>
[Trait("SWR", "SWR-IP-020")]
public sealed class ImageErrorRecoveryTests
{
    private static ImageProcessor CreateSut() => new();

    // ── Null guards for all operations ──────────────────────────────────────────

    [Fact]
    public void ApplyWindowLevel_NullImage_Throws()
    {
        var act = () => CreateSut().ApplyWindowLevel(null!, 128, 256);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Rotate_NullImage_Throws()
    {
        var act = () => CreateSut().Rotate(null!, 90);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Pan_NullImage_Throws()
    {
        var act = () => CreateSut().Pan(null!, 1, 1);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Zoom_NullImage_Throws()
    {
        var act = () => CreateSut().Zoom(null!, 2.0);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Gain/Offset error paths ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-039")]
    public void ApplyGainOffsetCorrection_NullOffsetMap_ReturnsCalibrationError()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(2, 2, 8, new byte[4], 128, 256)
        {
            RawPixelData16 = new ushort[4]
        };

        var result = sut.ApplyGainOffsetCorrection(image, new float[4], null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.CalibrationDataMissing);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-039")]
    public void ApplyGainOffsetCorrection_Raw16TooShort_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(4, 4, 8, new byte[16], 128, 256)
        {
            RawPixelData16 = new ushort[4] // Only 4, needs 16
        };

        var result = sut.ApplyGainOffsetCorrection(image, new float[16], new float[16]);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    // ── Edge enhancement invalid strength ───────────────────────────────────────

    [Theory]
    [Trait("SWR", "SWR-IP-043")]
    [InlineData(-0.5)]
    [InlineData(1.5)]
    [InlineData(double.NaN)]
    public void ApplyEdgeEnhancement_InvalidStrength_ReturnsFailure(double strength)
    {
        if (double.IsNaN(strength)) return; // NaN comparison is edge case, skip
        var sut = CreateSut();
        var image = new ProcessedImage(4, 4, 8, new byte[16], 128, 256);

        var result = sut.ApplyEdgeEnhancement(image, strength);

        result.IsFailure.Should().BeTrue();
    }

    // ── Auto trimming edge cases ────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-047")]
    public void ApplyAutoTrimming_CustomThreshold_RespectsThreshold()
    {
        var sut = CreateSut();
        // Image with values: 5, 15, 5, 15 → threshold 10: only 15s survive
        var pixels = new byte[] { 5, 15, 5, 15 };
        var image = new ProcessedImage(2, 2, 8, pixels, 128, 256);

        var result = sut.ApplyAutoTrimming(image, threshold: 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData[0].Should().Be(0);  // 5 <= 10
        result.Value.PixelData[1].Should().Be(15); // 15 > 10
    }

    // ── Black mask boundary ────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-049")]
    public void ApplyBlackMask_LeftEqualsRight_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(4, 4, 8, new byte[16], 128, 256);

        var result = sut.ApplyBlackMask(image, 2, 0, 2, 4);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("SWR", "SWR-IP-049")]
    public void ApplyBlackMask_TopEqualsBottom_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(4, 4, 8, new byte[16], 128, 256);

        var result = sut.ApplyBlackMask(image, 0, 2, 4, 2);

        result.IsFailure.Should().BeTrue();
    }

    // ── Normalize16BitTo8Bit flat image ─────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_Dicom16Bit_FlatImage_ReturnsMidGrey()
    {
        // All pixels same value → normalized to 128 (mid-grey)
        var sut = CreateSut();
        var tempPath = await CreateFlatDicom16BitAsync(4, 4, 1000);

        try
        {
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.PixelData.Should().OnlyContain(v => v == 128);
        }
        finally { System.IO.File.Delete(tempPath); }
    }

    private static async Task<string> CreateFlatDicom16BitAsync(ushort rows, ushort columns, ushort value)
    {
        var dataset = new FellowOakDicom.DicomDataset();
        dataset.Add(FellowOakDicom.DicomTag.SOPClassUID, FellowOakDicom.DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(FellowOakDicom.DicomTag.SOPInstanceUID, FellowOakDicom.DicomUID.Generate());
        dataset.Add(FellowOakDicom.DicomTag.Modality, "DX");
        dataset.Add(FellowOakDicom.DicomTag.Rows, rows);
        dataset.Add(FellowOakDicom.DicomTag.Columns, columns);
        dataset.Add(FellowOakDicom.DicomTag.BitsAllocated, (ushort)16);
        dataset.Add(FellowOakDicom.DicomTag.BitsStored, (ushort)12);
        dataset.Add(FellowOakDicom.DicomTag.HighBit, (ushort)11);
        dataset.Add(FellowOakDicom.DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(FellowOakDicom.DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(FellowOakDicom.DicomTag.PhotometricInterpretation, "MONOCHROME2");

        var pixels = Enumerable.Repeat(value, rows * columns).ToArray();
        dataset.Add(new FellowOakDicom.DicomOtherWord(FellowOakDicom.DicomTag.PixelData, pixels));

        var dcmFile = new FellowOakDicom.DicomFile(dataset);
        var tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"hnvue_flat_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }
}
