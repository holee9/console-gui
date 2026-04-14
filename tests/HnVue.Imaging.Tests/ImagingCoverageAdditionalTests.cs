using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Additional coverage tests for ImageProcessor edge cases and null/validation paths.
/// Targets branch coverage for Safety-Adjacent Imaging module (85%+ goal).
/// </summary>
[Trait("SWR", "SWR-IP-020")]
public sealed class ImagingCoverageAdditionalTests
{
    private static ImageProcessor CreateSut() => new();

    // ── Null image guards ────────────────────────────────────────────────────────

    [Fact]
    public void ApplyScatterCorrection_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyScatterCorrection(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyAutoTrimming_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyAutoTrimming(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyClahe_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyClahe(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyBrightnessOffset_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyBrightnessOffset(null!, 10);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyBlackMask_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyBlackMask(null!, 0, 0, 4, 4);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyGainOffsetCorrection_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyGainOffsetCorrection(null!, new float[1], new float[1]);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyNoiseReduction_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyNoiseReduction(null!, 0.5);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ApplyEdgeEnhancement_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = () => sut.ApplyEdgeEnhancement(null!, 0.5);
        act.Should().Throw<ArgumentNullException>();
    }

    // ── Validation ranges ────────────────────────────────────────────────────────

    [Fact]
    public void ApplyBrightnessOffset_ExceedsMaxOffset_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyBrightnessOffset(image, 300);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyBrightnessOffset_BelowMinOffset_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyBrightnessOffset(image, -300);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyNoiseReduction_NegativeStrength_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyNoiseReduction(image, -0.1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyNoiseReduction_StrengthAboveOne_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyNoiseReduction(image, 1.5);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyClahe_ClipLimitTooLow_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyClahe(image, clipLimit: 0.5, tileSize: 8);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyClahe_ClipLimitTooHigh_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyClahe(image, clipLimit: 5.0, tileSize: 8);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyClahe_ZeroTileSize_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyClahe(image, clipLimit: 2.0, tileSize: 0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    // ── ApplyBlackMask boundary checks ───────────────────────────────────────────

    [Fact]
    public void ApplyBlackMask_RightExceedsWidth_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 4, height: 4);

        var result = sut.ApplyBlackMask(image, 0, 0, 5, 4);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ApplyBlackMask_BottomExceedsHeight_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 4, height: 4);

        var result = sut.ApplyBlackMask(image, 0, 0, 4, 5);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ApplyBlackMask_RemoveWithRaw16_RestoresFromRawData()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 500, 1000, 1500, 2000 };
        var image = new ProcessedImage(
            width: 2, height: 2, bitsPerPixel: 8,
            pixelData: new byte[] { 50, 100, 150, 200 },
            windowCenter: 128.0, windowWidth: 256.0)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyBlackMask(image, 0, 0, 2, 2, apply: false);

        result.IsSuccess.Should().BeTrue();
        // Should have re-normalised from RawPixelData16
        result.Value.PixelData.Should().NotBeEquivalentTo(new byte[] { 50, 100, 150, 200 });
    }

    // ── Zoom with very small factor ─────────────────────────────────────────────

    [Fact]
    public void Zoom_MinimumValidFactor_ClampsTo1x1()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 100, height: 100);

        // Use exactly MinZoomFactor (0.01) → 100*0.01 = 1px
        var result = sut.Zoom(image, 0.01);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().BeGreaterOrEqualTo(1);
        result.Value.Height.Should().BeGreaterOrEqualTo(1);
    }

    // ── Rotate 270 ──────────────────────────────────────────────────────────────

    [Fact]
    public void Rotate_270Degrees_SwapsDimensions()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(4, 2, 8, new byte[4 * 2], 128, 256);

        var result = sut.Rotate(image, 270);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(2);
        result.Value.Height.Should().Be(4);
    }

    // ── ApplyGainOffsetCorrection with uniform image (range=0) ──────────────────

    [Fact]
    public void ApplyGainOffsetCorrection_UniformPixels_ReturnsMidGrey()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 1000, 1000, 1000, 1000 };
        var image = new ProcessedImage(
            width: 2, height: 2, bitsPerPixel: 8,
            pixelData: new byte[4], windowCenter: 128.0, windowWidth: 256.0)
        {
            RawPixelData16 = raw16
        };
        var gainMap = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
        var offsetMap = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        // Uniform values → range=0 → all 128
        result.Value.PixelData.Should().OnlyContain(v => v == 128);
    }

    // ── ProcessAsync null parameters ─────────────────────────────────────────────

    [Fact]
    public async Task ProcessAsync_NullParameters_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        var act = async () => await sut.ProcessAsync("path", null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── ApplyBlackMask with Raw16 length mismatch ───────────────────────────────

    [Fact]
    public void ApplyBlackMask_RemoveWithMismatchedRaw16_FallsBackToCopy()
    {
        var sut = CreateSut();
        var pixels = new byte[] { 100, 150, 200, 250 };
        var image = new ProcessedImage(2, 2, 8, pixels, 128, 256)
        {
            RawPixelData16 = new ushort[] { 1000, 2000 } // only 2, needs 4
        };

        var result = sut.ApplyBlackMask(image, 0, 0, 2, 2, apply: false);

        result.IsSuccess.Should().BeTrue();
        // Falls back to Array.Copy since Raw16 length != src.Length
        result.Value.PixelData.Should().BeEquivalentTo(pixels);
    }

    // ── Helper ───────────────────────────────────────────────────────────────────

    private static ProcessedImage MakeProcessedImage(
        int width = 64, int height = 64,
        double windowCenter = 128.0, double windowWidth = 256.0)
    {
        var pixels = new byte[width * height];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 256);
        return new ProcessedImage(width, height, 8, pixels, windowCenter, windowWidth);
    }
}
