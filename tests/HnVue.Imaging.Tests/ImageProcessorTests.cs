using System.IO;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Unit tests for <see cref="ImageProcessor"/> covering all IImageProcessor operations.
/// SWR-IP-020: Image processing functionality including windowing, zoom, and pan.
/// </summary>
public sealed class ImageProcessorTests
{
    private static ImageProcessor CreateSut() => new();

    // ── ProcessAsync ───────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_NullPath_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.ProcessAsync(null!, new ProcessingParameters());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_EmptyPath_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.ProcessAsync("   ", new ProcessingParameters());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_NonExistentFile_ReturnsFailure()
    {
        var sut = CreateSut();

        var result = await sut.ProcessAsync("/nonexistent/path/image.dcm", new ProcessingParameters());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidFile_WithExplicitWindowLevel_ReturnsProcessedImage()
    {
        var sut = CreateSut();
        var tempFile = CreateTempImageFile(width: 4, height: 4);
        try
        {
            var parameters = new ProcessingParameters(WindowCenter: 200.0, WindowWidth: 400.0, AutoWindow: false);

            var result = await sut.ProcessAsync(tempFile, parameters);

            result.IsSuccess.Should().BeTrue();
            result.Value.WindowCenter.Should().Be(200.0);
            result.Value.WindowWidth.Should().Be(400.0);
            result.Value.FilePath.Should().Be(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidFile_AutoWindow_ReturnsProcessedImageWithComputedWindow()
    {
        var sut = CreateSut();
        var tempFile = CreateTempImageFile(width: 4, height: 4);
        try
        {
            var parameters = new ProcessingParameters(AutoWindow: true);

            var result = await sut.ProcessAsync(tempFile, parameters);

            result.IsSuccess.Should().BeTrue();
            result.Value.WindowWidth.Should().BeGreaterThan(0);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidFile_NoAutoNoOverride_UsesDefaultWindow()
    {
        var sut = CreateSut();
        var tempFile = CreateTempImageFile(width: 4, height: 4);
        try
        {
            var parameters = new ProcessingParameters(AutoWindow: false);

            var result = await sut.ProcessAsync(tempFile, parameters);

            result.IsSuccess.Should().BeTrue();
            result.Value.WindowCenter.Should().Be(128.0);
            result.Value.WindowWidth.Should().Be(256.0);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidFile_ReturnsImageWithCorrectFilePath()
    {
        var sut = CreateSut();
        var tempFile = CreateTempImageFile(width: 2, height: 2);
        try
        {
            var result = await sut.ProcessAsync(tempFile, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.FilePath.Should().Be(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── ApplyWindowLevel ───────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-021")]
    public void ApplyWindowLevel_ValidParameters_ReturnsUpdatedImage()
    {
        var sut = CreateSut();
        var original = MakeProcessedImage(windowCenter: 100.0, windowWidth: 200.0);

        var result = sut.ApplyWindowLevel(original, 300.0, 600.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.WindowCenter.Should().Be(300.0);
        result.Value.WindowWidth.Should().Be(600.0);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-021")]
    public void ApplyWindowLevel_PreservesPixelDataAndDimensions()
    {
        var sut = CreateSut();
        var original = MakeProcessedImage(width: 10, height: 10);

        var result = sut.ApplyWindowLevel(original, 128.0, 256.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(10);
        result.Value.Height.Should().Be(10);
        result.Value.PixelData.Should().BeSameAs(original.PixelData);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-021")]
    public void ApplyWindowLevel_WindowWidthTooSmall_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.ApplyWindowLevel(image, 128.0, 0.0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-021")]
    public void ApplyWindowLevel_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();

        var act = () => sut.ApplyWindowLevel(null!, 128.0, 256.0);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Zoom ───────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void Zoom_Factor2_DoublesDimensions()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 100, height: 200);

        var result = sut.Zoom(image, 2.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(200);
        result.Value.Height.Should().Be(400);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void Zoom_FactorHalf_HalvesDimensions()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 100, height: 200);

        var result = sut.Zoom(image, 0.5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(50);
        result.Value.Height.Should().Be(100);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void Zoom_FactorZero_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.Zoom(image, 0.0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void Zoom_NegativeFactor_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.Zoom(image, -1.0);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-022")]
    public void Zoom_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();

        var act = () => sut.Zoom(null!, 1.5);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Pan ────────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-023")]
    public void Pan_ValidDeltas_ReturnsImageWithSameDimensions()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 100, height: 100);

        var result = sut.Pan(image, 10, -5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(100);
        result.Value.Height.Should().Be(100);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-023")]
    public void Pan_PreservesWindowLevel()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(windowCenter: 300.0, windowWidth: 600.0);

        var result = sut.Pan(image, 20, 30);

        result.IsSuccess.Should().BeTrue();
        result.Value.WindowCenter.Should().Be(300.0);
        result.Value.WindowWidth.Should().Be(600.0);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-023")]
    public void Pan_ZeroDeltas_ReturnsEquivalentImage()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 50, height: 80);

        var result = sut.Pan(image, 0, 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(50);
        result.Value.Height.Should().Be(80);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-023")]
    public void Pan_NullImage_ThrowsArgumentNullException()
    {
        var sut = CreateSut();

        var act = () => sut.Pan(null!, 0, 0);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static ProcessedImage MakeProcessedImage(
        int width = 64,
        int height = 64,
        double windowCenter = 128.0,
        double windowWidth = 256.0)
    {
        var pixels = new byte[width * height];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 256);

        return new ProcessedImage(
            width: width,
            height: height,
            bitsPerPixel: 8,
            pixelData: pixels,
            windowCenter: windowCenter,
            windowWidth: windowWidth);
    }

    /// <summary>Creates a temporary file filled with simple gradient pixel data.</summary>
    private static string CreateTempImageFile(int width, int height)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_test_{Guid.NewGuid():N}.raw");
        var pixels = new byte[width * height];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 256);

        File.WriteAllBytes(tempPath, pixels);
        return tempPath;
    }
}
