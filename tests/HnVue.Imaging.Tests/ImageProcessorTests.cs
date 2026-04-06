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
    [Trait("SWR", "SWR-IP-022")]
    public void ApplyWindowLevel_PreservesDimensionsAndAppliesLutToPixelData()
    {
        // SWR-IP-022: W/L LUT must be applied to pixel data, not just stored as metadata.
        // The result pixel buffer must differ from the original when W/L changes contrast.
        // Issue #2 fix verification.
        var sut = CreateSut();
        // Use a gradient (0..99) so that W/L remapping produces measurable differences.
        var pixelData = Enumerable.Range(0, 100).Select(i => (byte)i).ToArray();
        var original = new HnVue.Common.Models.ProcessedImage(
            width: 10, height: 10, bitsPerPixel: 8, pixelData: pixelData,
            windowCenter: 50.0, windowWidth: 100.0);

        var result = sut.ApplyWindowLevel(original, 128.0, 256.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(10);
        result.Value.Height.Should().Be(10);
        // W/L metadata updated
        result.Value.WindowCenter.Should().Be(128.0);
        result.Value.WindowWidth.Should().Be(256.0);
        // Pixel data is a new mapped buffer — NOT the same reference (LUT was applied)
        result.Value.PixelData.Should().NotBeSameAs(original.PixelData);
        // With center=128, width=256 → lower=0, upper=256: all original values [0..99]
        // map to [0, (99-0)/256 * 255] ≈ [0, 98]. First pixel (0) → 0. Last pixel (99) < 255.
        result.Value.PixelData[0].Should().Be(0);
        result.Value.PixelData[99].Should().BeLessThan(255);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-026")]
    public void Pan_AccumulatesOffset()
    {
        // SWR-IP-026: Pan offset must accumulate across calls. Issue #1 fix verification.
        var sut = CreateSut();
        var original = MakeProcessedImage(width: 10, height: 10);

        var step1 = sut.Pan(original, 10, 20);
        var step2 = sut.Pan(step1.Value, 5, -3);

        step1.IsSuccess.Should().BeTrue();
        step1.Value.PanOffsetX.Should().Be(10);
        step1.Value.PanOffsetY.Should().Be(20);

        step2.IsSuccess.Should().BeTrue();
        step2.Value.PanOffsetX.Should().Be(15);  // 10 + 5
        step2.Value.PanOffsetY.Should().Be(17);  // 20 + (-3)
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

    // ── Rotate ─────────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-027")]
    public void Rotate_90Degrees_SwapsDimensions()
    {
        // SWR-IP-027: 90° CW rotation should swap width and height. Issue #3 fix.
        var sut = CreateSut();
        var image = new HnVue.Common.Models.ProcessedImage(4, 2, 8, new byte[4 * 2], 128, 256);

        var result = sut.Rotate(image, 90);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(2);   // srcH → dstW
        result.Value.Height.Should().Be(4);  // srcW → dstH
        result.Value.PixelData.Should().HaveCount(2 * 4);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-027")]
    public void Rotate_180Degrees_PreservesDimensions()
    {
        var sut = CreateSut();
        var image = new HnVue.Common.Models.ProcessedImage(4, 2, 8, new byte[4 * 2], 128, 256);

        var result = sut.Rotate(image, 180);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(4);
        result.Value.Height.Should().Be(2);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-027")]
    public void Rotate_InvalidAngle_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage();

        var result = sut.Rotate(image, 45);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    // ── Flip ───────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("SWR", "SWR-IP-027")]
    public void Flip_PreservesDimensionsAndPixelCount(bool horizontal)
    {
        // SWR-IP-027: Flip must preserve dimensions. Issue #3 fix.
        var sut = CreateSut();
        var pixelData = new byte[] { 1, 2, 3, 4, 5, 6 };
        var image = new HnVue.Common.Models.ProcessedImage(3, 2, 8, pixelData, 128, 256);

        var result = sut.Flip(image, horizontal);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(3);
        result.Value.Height.Should().Be(2);
        result.Value.PixelData.Should().HaveCount(6);
        // Flipped result must differ from original (pixels are rearranged)
        result.Value.PixelData.Should().NotBeEquivalentTo(pixelData);
    }

    // ── Zoom (Bicubic/AreaAverage) ─────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-024")]
    public void Zoom_Upscale_UsesBicubicAndProducesLargerImage()
    {
        // SWR-IP-024: factor > 1 should produce a bicubic-interpolated image. Issue #4 fix.
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 4, height: 4);

        var result = sut.Zoom(image, 2.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(8);
        result.Value.Height.Should().Be(8);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-024")]
    public void Zoom_Downscale_UsesAreaAverageAndProducesSmallerImage()
    {
        // SWR-IP-024: factor <= 1 should produce an area-averaged image. Issue #4 fix.
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 8, height: 8);

        var result = sut.Zoom(image, 0.5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(4);
        result.Value.Height.Should().Be(4);
    }

    // ── ApplyGainOffsetCorrection ──────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-039")]
    public void ApplyGainOffsetCorrection_WithNullGainMap_ReturnsCalibrationError()
    {
        // SWR-IP-039 Safety: null gainMap must block operation with CalibrationDataMissing.
        var sut = CreateSut();
        var image = MakeProcessedImageWith16Bit(width: 4, height: 4);
        var offsetMap = new float[16];

        var result = sut.ApplyGainOffsetCorrection(image, gainMap: null, offsetMap);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.CalibrationDataMissing);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-039")]
    public void ApplyGainOffsetCorrection_WithValidMaps_CorrectsCalibratedPixels()
    {
        // SWR-IP-039: corrected[i] = clamp((pixel16 - offset) * gain, 0, 65535)
        var sut = CreateSut();
        int pixelCount = 4;
        // Raw 16-bit pixels: all 1000
        var raw16 = Enumerable.Repeat((ushort)1000, pixelCount).ToArray();
        // gain = 2.0, offset = 500 → corrected = (1000 - 500) * 2 = 1000
        var gainMap = Enumerable.Repeat(2.0f, pixelCount).ToArray();
        var offsetMap = Enumerable.Repeat(500.0f, pixelCount).ToArray();

        var image = new ProcessedImage(
            width: 2, height: 2, bitsPerPixel: 8,
            pixelData: new byte[pixelCount],
            windowCenter: 128.0, windowWidth: 256.0)
        {
            RawPixelData16 = raw16
        };

        var result = sut.ApplyGainOffsetCorrection(image, gainMap, offsetMap);

        result.IsSuccess.Should().BeTrue();
        result.Value.RawPixelData16.Should().NotBeNull();
        // All corrected 16-bit values should equal 1000 (flat → mid-grey display)
        result.Value.RawPixelData16!.Should().OnlyContain(v => v == 1000);
    }

    // ── ApplyNoiseReduction ───────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-041")]
    public void ApplyNoiseReduction_WithZeroStrength_ReturnsUnchangedImage()
    {
        // SWR-IP-041: strength=0 must return image identical to the input.
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 8, height: 8);

        var result = sut.ApplyNoiseReduction(image, 0.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(image);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-041")]
    public void ApplyNoiseReduction_WithFullStrength_SmoothesNoisyImage()
    {
        // SWR-IP-041: strength=1 should produce a blurred output (lower variance).
        var sut = CreateSut();
        var rng = new Random(42);
        var pixels = new byte[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)rng.Next(0, 256);

        var image = new ProcessedImage(16, 16, 8, pixels, 128.0, 256.0);

        var result = sut.ApplyNoiseReduction(image, 1.0);

        result.IsSuccess.Should().BeTrue();
        // Output must differ from noisy input.
        result.Value.PixelData.Should().NotEqual(pixels);
        // Variance of smoothed image should be lower than input.
        double inputVariance = ComputeVariance(pixels);
        double outputVariance = ComputeVariance(result.Value.PixelData);
        outputVariance.Should().BeLessThan(inputVariance);
    }

    // ── ApplyEdgeEnhancement ──────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-043")]
    public void ApplyEdgeEnhancement_WithZeroStrength_ReturnsUnchangedImage()
    {
        // SWR-IP-043: strength=0 must return image identical to the input.
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 8, height: 8);

        var result = sut.ApplyEdgeEnhancement(image, 0.0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(image);
    }

    // ── ApplyScatterCorrection ────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-045")]
    public void ApplyScatterCorrection_ReturnsReducedLowFrequencyContent()
    {
        // SWR-IP-045: after scatter correction the mean pixel value should decrease
        // because the low-frequency scatter component is subtracted.
        var sut = CreateSut();
        // Uniform mid-grey image: scatter removal reduces mean.
        var pixels = Enumerable.Repeat((byte)128, 32 * 32).ToArray();
        var image = new ProcessedImage(32, 32, 8, pixels, 128.0, 256.0);

        var result = sut.ApplyScatterCorrection(image);

        result.IsSuccess.Should().BeTrue();
        double inputMean = pixels.Average(b => (double)b);
        double outputMean = result.Value.PixelData.Average(b => (double)b);
        outputMean.Should().BeLessThan(inputMean);
    }

    // ── ApplyAutoTrimming ─────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-047")]
    public void ApplyAutoTrimming_BlackBorderImage_MasksCorrectly()
    {
        // SWR-IP-047: border pixels (<=threshold) must be set to 0; interior preserved.
        // Image: 4×4, all zeros except centre 2×2 which is 128.
        var sut = CreateSut();
        var pixels = new byte[4 * 4]; // all zeros
        // Set centre 2×2 (rows 1-2, cols 1-2) to 128
        pixels[1 * 4 + 1] = 128;
        pixels[1 * 4 + 2] = 128;
        pixels[2 * 4 + 1] = 128;
        pixels[2 * 4 + 2] = 128;

        var image = new ProcessedImage(4, 4, 8, pixels, 128.0, 256.0);

        var result = sut.ApplyAutoTrimming(image, threshold: 10);

        result.IsSuccess.Should().BeTrue();
        // Corners (border) must be zero.
        result.Value.PixelData[0 * 4 + 0].Should().Be(0);
        result.Value.PixelData[3 * 4 + 3].Should().Be(0);
        // Centre pixels must be preserved.
        result.Value.PixelData[1 * 4 + 1].Should().Be(128);
        result.Value.PixelData[2 * 4 + 2].Should().Be(128);
        // Dimensions unchanged.
        result.Value.Width.Should().Be(4);
        result.Value.Height.Should().Be(4);
    }

    // ── ApplyClahe ────────────────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-050")]
    public void ApplyClahe_LowContrastImage_IncreasesContrast()
    {
        // SWR-IP-050: CLAHE must increase the contrast (pixel value spread) of a low-contrast image.
        var sut = CreateSut();
        // Low-contrast image: all pixels clustered around mid-grey (120-135).
        var pixels = new byte[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = (byte)(120 + (i % 16));

        var image = new ProcessedImage(16, 16, 8, pixels, 128.0, 256.0);

        var result = sut.ApplyClahe(image, clipLimit: 2.0, tileSize: 8);

        result.IsSuccess.Should().BeTrue();
        // Output pixel range should be wider than input range.
        byte inputMin = pixels.Min();
        byte inputMax = pixels.Max();
        byte outputMin = result.Value.PixelData.Min();
        byte outputMax = result.Value.PixelData.Max();
        int inputRange = inputMax - inputMin;
        int outputRange = outputMax - outputMin;
        outputRange.Should().BeGreaterThan(inputRange);
    }

    // ── ApplyBrightnessOffset ─────────────────────────────────────────────────

    [Fact]
    [Trait("SWR", "SWR-IP-052")]
    public void ApplyBrightnessOffset_PositiveOffset_IncreasesAllPixels()
    {
        // SWR-IP-052: positive offset must increase all pixel values (before clamping).
        var sut = CreateSut();
        var pixels = new byte[] { 0, 50, 100, 150 };
        var image = new ProcessedImage(4, 1, 8, pixels, 128.0, 256.0);

        var result = sut.ApplyBrightnessOffset(image, 10);

        result.IsSuccess.Should().BeTrue();
        result.Value.PixelData[0].Should().Be(10);
        result.Value.PixelData[1].Should().Be(60);
        result.Value.PixelData[2].Should().Be(110);
        result.Value.PixelData[3].Should().Be(160);
    }

    [Fact]
    [Trait("SWR", "SWR-IP-052")]
    public void ApplyBrightnessOffset_ClampsMagnitude_NoOverflow()
    {
        // SWR-IP-052: values must be clamped to [0, 255]; no overflow or underflow.
        var sut = CreateSut();
        var pixels = new byte[] { 10, 250 };
        var image = new ProcessedImage(2, 1, 8, pixels, 128.0, 256.0);

        // Negative offset: first pixel (10 - 20) should clamp to 0.
        var negResult = sut.ApplyBrightnessOffset(image, -20);
        negResult.IsSuccess.Should().BeTrue();
        negResult.Value.PixelData[0].Should().Be(0);

        // Positive offset: second pixel (250 + 20) should clamp to 255.
        var posResult = sut.ApplyBrightnessOffset(image, 20);
        posResult.IsSuccess.Should().BeTrue();
        posResult.Value.PixelData[1].Should().Be(255);
    }

    // ── Additional helpers ─────────────────────────────────────────────────────

    /// <summary>Creates a ProcessedImage that also carries a synthetic 16-bit pixel buffer.</summary>
    private static ProcessedImage MakeProcessedImageWith16Bit(int width, int height)
    {
        int pixelCount = width * height;
        var pixels8 = new byte[pixelCount];
        var pixels16 = new ushort[pixelCount];
        for (int i = 0; i < pixelCount; i++)
        {
            pixels8[i] = (byte)(i % 256);
            pixels16[i] = (ushort)(i * 256 % 65535);
        }

        return new ProcessedImage(
            width: width,
            height: height,
            bitsPerPixel: 8,
            pixelData: pixels8,
            windowCenter: 128.0,
            windowWidth: 256.0)
        {
            RawPixelData16 = pixels16
        };
    }

    /// <summary>Computes variance of a byte pixel buffer.</summary>
    private static double ComputeVariance(byte[] data)
    {
        if (data.Length == 0) return 0.0;
        double mean = data.Average(b => (double)b);
        return data.Sum(b => (b - mean) * (b - mean)) / data.Length;
    }

    // ── ApplyBlackMask (SWR-IP-049) ───────────────────────────────────────────

    [Fact]
    public void ApplyBlackMask_Apply_SetsOutsideBoundaryToZero()
    {
        var sut = CreateSut();
        // 4×4 image, all pixels = 200
        var pixels = Enumerable.Repeat((byte)200, 16).ToArray();
        var image = new ProcessedImage(4, 4, 8, pixels, 128, 256);

        // Mask keeps only center 2×2 (cols 1-2, rows 1-2)
        var result = sut.ApplyBlackMask(image, left: 1, top: 1, right: 3, bottom: 3, apply: true);

        result.IsSuccess.Should().BeTrue();
        var output = result.Value.PixelData;
        // Row 0 — all outside → 0
        output[0].Should().Be(0); output[1].Should().Be(0);
        output[2].Should().Be(0); output[3].Should().Be(0);
        // Row 1, col 1-2 inside → 200; col 0,3 outside → 0
        output[4].Should().Be(0);
        output[5].Should().Be(200);
        output[6].Should().Be(200);
        output[7].Should().Be(0);
    }

    [Fact]
    public void ApplyBlackMask_RemoveWithoutRaw16_ReturnsSamePixels()
    {
        var sut = CreateSut();
        var pixels = new byte[] { 100, 150, 200, 250 };
        var image = new ProcessedImage(2, 2, 8, pixels, 128, 256);

        var result = sut.ApplyBlackMask(image, 1, 1, 2, 2, apply: false);

        result.IsSuccess.Should().BeTrue();
        // No Raw16 → source pixels copied unchanged
        result.Value.PixelData.Should().BeEquivalentTo(pixels);
    }

    [Fact]
    public void ApplyBlackMask_OutOfRangeBoundary_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 4, height: 4);

        var result = sut.ApplyBlackMask(image, left: -1, top: 0, right: 4, bottom: 4);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
    }

    [Fact]
    public void ApplyBlackMask_DegenerateBoundary_ReturnsFailure()
    {
        var sut = CreateSut();
        var image = MakeProcessedImage(width: 4, height: 4);

        // left >= right is degenerate
        var result = sut.ApplyBlackMask(image, left: 3, top: 0, right: 2, bottom: 4);

        result.IsFailure.Should().BeTrue();
    }
}
