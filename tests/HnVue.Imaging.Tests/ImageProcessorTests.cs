using System.IO;
using FellowOakDicom;
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

    // ── DICOM synthetic tests ─────────────────────────────────────────────────

    /// <summary>
    /// Creates an in-memory DICOM dataset saved to a temporary file.
    /// The pixel buffer is a simple gradient over the specified dimensions.
    /// Includes mandatory DICOM file meta tags (SOPClassUID, SOPInstanceUID, TransferSyntaxUID).
    /// </summary>
    private static async Task<string> CreateTempDicomFileAsync(
        ushort rows,
        ushort columns,
        ushort bitsAllocated,
        byte[]? pixelBytes = null,
        double? windowCenter = null,
        double? windowWidth = null)
    {
        var dataset = new DicomDataset();

        // Mandatory file meta / composite IOD tags required by DicomFile constructor.
        dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.Add(DicomTag.Modality, "DX");

        // Image pixel module.
        dataset.Add(DicomTag.Rows, rows);
        dataset.Add(DicomTag.Columns, columns);
        dataset.Add(DicomTag.BitsAllocated, bitsAllocated);
        dataset.Add(DicomTag.BitsStored, bitsAllocated);
        dataset.Add(DicomTag.HighBit, (ushort)(bitsAllocated - 1));
        dataset.Add(DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");

        if (windowCenter.HasValue)
            dataset.Add(DicomTag.WindowCenter, windowCenter.Value);
        if (windowWidth.HasValue)
            dataset.Add(DicomTag.WindowWidth, windowWidth.Value);

        if (bitsAllocated == 8)
        {
            var pixels8 = pixelBytes ?? CreateGradientPixels8(rows, columns);
            dataset.Add(new DicomOtherByte(DicomTag.PixelData, pixels8));
        }
        else
        {
            // 16-bit: store as OtherWord.
            var pixels16 = CreateGradientPixels16(rows, columns);
            dataset.Add(new DicomOtherWord(DicomTag.PixelData, pixels16));
        }

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_dicom_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath).ConfigureAwait(false);
        return tempPath;
    }

    /// <summary>Creates an 8-bit gradient pixel buffer for synthetic DICOM tests.</summary>
    private static byte[] CreateGradientPixels8(ushort rows, ushort columns)
    {
        var pixels = new byte[rows * columns];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(i % 256);
        return pixels;
    }

    /// <summary>Creates a 16-bit gradient pixel buffer for synthetic DICOM tests.</summary>
    private static ushort[] CreateGradientPixels16(ushort rows, ushort columns)
    {
        var pixels = new ushort[rows * columns];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (ushort)(i % 4096); // 12-bit range typical for DR detectors
        return pixels;
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidDicomFile_8bit_ReturnsCorrectDimensions()
    {
        // Arrange
        var sut = CreateSut();
        var tempPath = await CreateTempDicomFileAsync(rows: 4, columns: 4, bitsAllocated: 8);

        try
        {
            // Act
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters());

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Width.Should().Be(4);
            result.Value.Height.Should().Be(4);
            result.Value.BitsPerPixel.Should().Be(8);
            result.Value.FilePath.Should().Be(tempPath);
            result.Value.PixelData.Should().HaveCount(16);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_ValidDicomFile_16bit_NormalizesTo8bit()
    {
        // Arrange
        var sut = CreateSut();
        var tempPath = await CreateTempDicomFileAsync(rows: 4, columns: 4, bitsAllocated: 16);

        try
        {
            // Act
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters());

            // Assert — 16-bit DICOM is normalised to 8-bit display data
            result.IsSuccess.Should().BeTrue();
            result.Value.Width.Should().Be(4);
            result.Value.Height.Should().Be(4);
            result.Value.BitsPerPixel.Should().Be(8);
            result.Value.PixelData.Should().HaveCount(16);

            // All output values must be within 8-bit range
            result.Value.PixelData.Should().OnlyContain(b => b >= 0 && b <= 255);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }

    [Fact]
    [Trait("SWR", "SWR-IP-020")]
    public async Task ProcessAsync_DicomWithVoiLutTags_UsesTagWindowWhenAutoWindowRequested()
    {
        // Arrange
        var sut = CreateSut();
        const double expectedCenter = 2048.0;
        const double expectedWidth = 4096.0;

        var tempPath = await CreateTempDicomFileAsync(
            rows: 4,
            columns: 4,
            bitsAllocated: 8,
            windowCenter: expectedCenter,
            windowWidth: expectedWidth);

        try
        {
            // Act — AutoWindow: true should prefer DICOM VOI LUT tags over statistics
            var parameters = new ProcessingParameters(AutoWindow: true);
            var result = await sut.ProcessAsync(tempPath, parameters);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.WindowCenter.Should().Be(expectedCenter);
            result.Value.WindowWidth.Should().Be(expectedWidth);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}
