using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Safety-related tests for Gain/Offset calibration correction (SWR-IP-039).
/// Gain/Offset correction is marked @MX:WARN for HAZ-RAD/HAZ-SW safety concerns.
/// </summary>
[Trait("SWR", "SWR-IP-039")]
public sealed class ImageGainOffsetCorrectionTests
{
    private static ImageProcessor CreateSut() => new();

    [Fact]
    public void ApplyGainOffsetCorrection_CorrectedValuesWithin16BitRange()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 0, 65535, 30000, 10000 };
        var image = new ProcessedImage(2, 2, 8, new byte[4], 128, 256) { RawPixelData16 = raw16 };
        var gainMap = new float[] { 1.5f, 0.5f, 2.0f, 1.0f };
        var offsetMap = new float[] { 0, 0, 1000, 500 };

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().NotBeNull();
        foreach (var v in result.Value.RawPixelData16!)
        {
            v.Should().BeInRange((ushort)0, (ushort)65535);
        }
    }

    [Fact]
    public void ApplyGainOffsetCorrection_NegativeCorrectedValue_ClampsToZero()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 100 };
        var image = new ProcessedImage(1, 1, 8, new byte[1], 128, 256) { RawPixelData16 = raw16 };
        var gainMap = new float[] { 1.0f };
        var offsetMap = new float[] { 200.0f }; // offset > pixel → corrected = (100-200)*1 = -100

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16![0].Should().Be(0); // Clamped
    }

    [Fact]
    public void ApplyGainOffsetCorrection_OverflowCorrectedValue_ClampsTo65535()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 60000 };
        var image = new ProcessedImage(1, 1, 8, new byte[1], 128, 256) { RawPixelData16 = raw16 };
        var gainMap = new float[] { 10.0f }; // 60000 * 10 = 600000
        var offsetMap = new float[] { 0.0f };

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16![0].Should().Be(65535); // Clamped
    }

    [Fact]
    public void ApplyGainOffsetCorrection_BothMapsNull_ReturnsCalibrationMissing()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(1, 1, 8, new byte[1], 128, 256) { RawPixelData16 = new ushort[1] };

        var result = sut.ApplyGainOffsetCorrection(image, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.CalibrationDataMissing);
    }

    [Fact]
    public void ApplyGainOffsetCorrection_VariousGainOffset_Produces8BitOutput()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 0, 1000, 5000, 65535 };
        var image = new ProcessedImage(2, 2, 8, new byte[4], 128, 256) { RawPixelData16 = raw16 };
        var gainMap = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        var offsetMap = new float[] { 0, 0, 0, 0 };

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData.Should().HaveCount(4);
        result.Value.PixelData.Should().OnlyContain(v => v >= 0 && v <= 255);
    }
}
