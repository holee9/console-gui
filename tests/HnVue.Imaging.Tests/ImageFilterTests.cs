using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Tests for image processing filters: brightness, contrast (window level), zoom, noise reduction, edge enhancement.
/// </summary>
[Trait("SWR", "SWR-IP-041")]
public sealed class ImageFilterTests
{
    private static ImageProcessor CreateSut() => new();

    // ── Brightness ──────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-052")]
    public void ApplyBrightnessOffset_ZeroOffset_ReturnsIdenticalPixelData()
    {
        var sut = CreateSut();
        var pixels = new byte[] { 50, 100, 200 };
        var image = new ProcessedImage(3, 1, 8, pixels, 128, 256);

        var result = sut.ApplyBrightnessOffset(image, 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData.Should().BeEquivalentTo(pixels);
    }

    [Theory]
    [Trait("SWR", "SWR-IP-052")]
    [InlineData(255, 0, 255)]   // Max pixel + max offset → clamp to 255
    [InlineData(0, -255, 0)]    // Min pixel + min offset → clamp to 0
    public void ApplyBrightnessOffset_BoundaryClamping(byte input, int offset, byte expected)
    {
        var sut = CreateSut();
        var image = new ProcessedImage(1, 1, 8, new[] { input }, 128, 256);

        var result = sut.ApplyBrightnessOffset(image, offset);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData[0].Should().Be(expected);
    }

    // ── Window Level (contrast) ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void ApplyWindowLevel_NarrowWindow_HighContrast()
    {
        var sut = CreateSut();
        // Gradient 0..255 with narrow window → high contrast remap
        var pixels = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var image = new ProcessedImage(256, 1, 8, pixels, 128, 10);

        var result = sut.ApplyWindowLevel(image, 128, 10);

        result.IsSuccess.Should().BeTrue();
        // Pixels well above center should map to 255, well below to 0
        result.Value.PixelData[200].Should().Be(255);
        result.Value.PixelData[50].Should().Be(0);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void ApplyWindowLevel_WideWindow_LowContrast()
    {
        var sut = CreateSut();
        var pixels = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
        var image = new ProcessedImage(256, 1, 8, pixels, 128, 256);

        var result = sut.ApplyWindowLevel(image, 128, 256);

        result.IsSuccess.Should().BeTrue();
        // With window=256 and center=128, pixel 128 → (128-0)/256*255 ≈ 127
        result.Value.PixelData[128].Should().BeCloseTo(127, 1);
    }

    // ── Noise Reduction ─────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-041")]
    public void ApplyNoiseReduction_PreservesRawPixelData16()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 1000, 2000, 3000, 4000 };
        var image = new ProcessedImage(2, 2, 8, new byte[] { 50, 100, 150, 200 }, 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyNoiseReduction(image, 0.5);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-041")]
    public void ApplyNoiseReduction_UniformImage_NoChange()
    {
        var sut = CreateSut();
        var pixels = Enumerable.Repeat((byte)128, 4 * 4).ToArray();
        var image = new ProcessedImage(4, 4, 8, pixels, 128, 256);

        var result = sut.ApplyNoiseReduction(image, 0.8);

        result.IsSuccess.Should().BeTrue();
        // Uniform input → Gaussian blur of uniform is uniform → blend is uniform
        result.Value.PixelData.Should().OnlyContain(v => v == 128);
    }

    // ── Edge Enhancement ────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-043")]
    public void ApplyEdgeEnhancement_PreservesRawPixelData16()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 500, 1000, 1500, 2000 };
        var image = new ProcessedImage(2, 2, 8, new byte[] { 50, 100, 150, 200 }, 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyEdgeEnhancement(image, 0.5);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    // ── Scatter Correction ──────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-045")]
    public void ApplyScatterCorrection_PreservesRawPixelData16()
    {
        var sut = CreateSut();
        var raw16 = Enumerable.Range(0, 64).Select(i => (ushort)(i * 100)).ToArray();
        var image = new ProcessedImage(8, 8, 8, new byte[64], 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyScatterCorrection(image);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    // ── CLAHE ───────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-050")]
    public void ApplyClahe_EmptyPixelData_ReturnsSameImage()
    {
        var sut = CreateSut();
        var image = new ProcessedImage(0, 0, 8, Array.Empty<byte>(), 128, 256);

        var result = sut.ApplyClahe(image, 2.0, 8);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(image);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-050")]
    public void ApplyClahe_PreservesRawPixelData16()
    {
        var sut = CreateSut();
        var raw16 = new ushort[] { 100, 200, 300, 400, 500, 600, 700, 800 };
        var pixels = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };
        var image = new ProcessedImage(4, 2, 8, pixels, 128, 256)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyClahe(image, 2.0, 4);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().BeEquivalentTo(raw16);
    }

    [Theory]
    [Trait("SWR", "SWR-IP-050")]
    [InlineData(1.0)]
    [InlineData(4.0)]
    public void ApplyClahe_BoundaryClipLimits_ReturnsSuccess(double clipLimit)
    {
        var sut = CreateSut();
        var image = new ProcessedImage(4, 4, 8, new byte[16], 128, 256);

        var result = sut.ApplyClahe(image, clipLimit, 4);

        result.IsSuccess.Should().BeTrue();
    }
}
