using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Tests for auto-trimming and black mask operations.
/// </summary>
[Trait("SWR", "SWR-IP-047")]
public sealed class ImageAutoTrimmingAndMaskTests
{
    private static ImageProcessor CreateSut() => new();

    // ── Auto trimming ───────────────────────────────────────────────────────────

    [Fact]
    public void ApplyAutoTrimming_PreservesRawPixelData16()
    {
        var raw16 = new ushort[] { 100, 200, 300, 400 };
        var image = new ProcessedImage(2, 2, 8, new byte[] { 10, 20, 30, 40 }, 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut().ApplyAutoTrimming(image, 5);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    [Fact]
    public void ApplyAutoTrimming_FullImageAboveThreshold_NoMasking()
    {
        var pixels = Enumerable.Repeat((byte)200, 4 * 4).ToArray();
        var image = new ProcessedImage(4, 4, 8, pixels, 128, 256);

        var result = sut().ApplyAutoTrimming(image, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData.Should().BeEquivalentTo(pixels);
    }

    [Fact]
    public void ApplyAutoTrimming_SinglePixelAboveThreshold_MasksAllExceptOne()
    {
        var pixels = new byte[4 * 4]; // all zeros
        pixels[7] = 128; // one pixel above threshold

        var image = new ProcessedImage(4, 4, 8, pixels, 128, 256);

        var result = sut().ApplyAutoTrimming(image, 10);

        result.IsSuccess.Should().BeTrue();
        // Only pixel at (3,1) should survive — index 1*4+3 = 7
        var surviving = result.Value.PixelData.Where(v => v > 0).ToList();
        surviving.Should().HaveCount(1);
        surviving[0].Should().Be(128);
    }

    // ── Black mask ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-049")]
    public void ApplyBlackMask_FullImageMask_RemovesAll()
    {
        var pixels = Enumerable.Repeat((byte)200, 4 * 4).ToArray();
        var image = new ProcessedImage(4, 4, 8, pixels, 128, 256);

        // Mask with zero-area (left=right=0, top=0, bottom=0) — degenerate
        var result = sut().ApplyBlackMask(image, 0, 0, 0, 0);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    [Trait("SWR", "SWR-IP-049")]
    public void ApplyBlackMask_PreservesRawPixelData16()
    {
        var raw16 = new ushort[] { 100, 200, 300, 400 };
        var image = new ProcessedImage(2, 2, 8, new byte[] { 50, 100, 150, 200 }, 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut().ApplyBlackMask(image, 0, 0, 2, 2, apply: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-049")]
    public void ApplyBlackMask_RemoveWithRaw16_ProducesValidOutput()
    {
        var raw16 = new ushort[] { 0, 128, 256, 512 };
        var image = new ProcessedImage(2, 2, 8, new byte[] { 50, 100, 150, 200 }, 128.0, 256.0)
        {
            RawPixelData16 = raw16
        };

        var result = sut().ApplyBlackMask(image, 0, 0, 2, 2, apply: false);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData.Should().OnlyContain(v => v >= 0 && v <= 255);
    }

    private static ImageProcessor sut() => new();
}
