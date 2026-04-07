using System.IO;
using FellowOakDicom;
using FellowOakDicom.Imaging.Codec;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Imaging;

/// <summary>
/// Implements image processing and interactive display manipulation for DR/CR detector images.
/// Provides windowing, zoom, and pan operations on processed radiographic images.
/// IEC 62304 Class B — image quality directly affects diagnostic accuracy.
/// </summary>
public sealed class ImageProcessor : IImageProcessor
{
    private const int MinWindowWidth = 1;
    private const double MinZoomFactor = 0.01;

    // @MX:ANCHOR ProcessAsync - @MX:REASON: Core image processing entry point, handles DICOM parsing, 16-bit to 8-bit normalization, preserves RawPixelData16 for ROI statistics (SWR-IP-036, Issue #8)
    /// <inheritdoc/>
    /// <remarks>SWR-IP-020</remarks>
    public async Task<Result<ProcessedImage>> ProcessAsync(
        string rawImagePath,
        ProcessingParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (rawImagePath is null || string.IsNullOrWhiteSpace(rawImagePath))
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Raw image path must not be empty.");

        if (!File.Exists(rawImagePath))
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                $"File not found: {rawImagePath}");

        try
        {
            // Attempt DICOM parse first; fall back to raw byte interpretation on failure.
            DicomFile? dicomFile = null;
            try
            {
                // fo-dicom 5.x OpenAsync() does not expose a CancellationToken parameter.
                // Check cancellation before the I/O call; the operation itself is not interruptible mid-flight.
                // Issue #6 — partial fix; full cancellability requires fo-dicom 6+.
                cancellationToken.ThrowIfCancellationRequested();
                dicomFile = await DicomFile.OpenAsync(rawImagePath).ConfigureAwait(false);
            }
            catch (DicomException)
            {
                // Not a DICOM file — proceed with raw byte fallback below.
            }

            if (dicomFile is not null)
            {
                return ProcessDicomFile(dicomFile, rawImagePath, parameters);
            }

            // Raw byte fallback — keeps existing raw file tests passing.
            return ProcessRawFile(rawImagePath, parameters);
        }
        catch (IOException ex)
        {
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                $"I/O error reading image: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-IP-022: Applies the DICOM standard Window/Level LUT to the pixel buffer.
    /// For 8-bit grayscale pixel data, each byte is remapped according to:
    ///   output = clamp((input - (center - width/2)) / width, 0, 1) × 255
    /// Issue #2.
    /// </remarks>
    // @MX:NOTE Window level LUT formula: output = clamp((px - (center - width/2)) / width, 0, 1) × 255 per DICOM standard
    public Result<ProcessedImage> ApplyWindowLevel(
        ProcessedImage image,
        double windowCenter,
        double windowWidth)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (windowWidth < MinWindowWidth)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                $"Window width must be at least {MinWindowWidth}.");

        // Apply DICOM standard W/L linear rescale to 8-bit output buffer.
        var srcPixels = image.PixelData;
        var mapped = new byte[srcPixels.Length];
        double lowerBound = windowCenter - windowWidth / 2.0;

        for (int i = 0; i < srcPixels.Length; i++)
        {
            double normalised = (srcPixels[i] - lowerBound) / windowWidth;
            mapped[i] = (byte)(Math.Clamp(normalised, 0.0, 1.0) * 255.0);
        }

        var updated = new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: mapped,
            windowCenter: windowCenter,
            windowWidth: windowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY);

        return Result.Success(updated);
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-022</remarks>
    // @MX:NOTE Zoom uses BicubicResample for upscaling (> 1.0), AreaAverageResample for downscaling (< 1.0) per SWR-IP-024
    public Result<ProcessedImage> Zoom(ProcessedImage image, double factor)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (factor < MinZoomFactor)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                $"Zoom factor must be at least {MinZoomFactor}.");

        var newWidth = (int)Math.Round(image.Width * factor);
        var newHeight = (int)Math.Round(image.Height * factor);

        if (newWidth < 1) newWidth = 1;
        if (newHeight < 1) newHeight = 1;

        // SWR-IP-024: Bicubic for upscaling (factor > 1), Area Average for downscaling. Issue #4.
        var resampledPixels = factor > 1.0
            ? BicubicResample(image.PixelData, image.Width, image.Height, newWidth, newHeight)
            : AreaAverageResample(image.PixelData, image.Width, image.Height, newWidth, newHeight);

        var zoomed = new ProcessedImage(
            width: newWidth,
            height: newHeight,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: resampledPixels,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath);

        return Result.Success(zoomed);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-IP-026: Pan is a display-space operation.
    /// Pixel data is unchanged; the cumulative offset is carried on PanOffsetX/PanOffsetY.
    /// The rendering layer (WriteableBitmap viewport) uses these offsets for sub-pixel scrolling.
    /// Issue #1.
    /// </remarks>
    public Result<ProcessedImage> Pan(ProcessedImage image, int deltaX, int deltaY)
    {
        ArgumentNullException.ThrowIfNull(image);

        var panned = new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: image.PixelData,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX + deltaX,
            panOffsetY: image.PanOffsetY + deltaY);

        return Result.Success(panned);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-IP-027: Supports 90°, 180°, 270° clockwise rotation via pixel coordinate mapping.
    /// Issue #3.
    /// </remarks>
    public Result<ProcessedImage> Rotate(ProcessedImage image, int degrees)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (degrees is not (90 or 180 or 270))
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed, "Rotation angle must be 90, 180, or 270 degrees.");

        int srcW = image.Width, srcH = image.Height;
        int dstW = degrees == 180 ? srcW : srcH;
        int dstH = degrees == 180 ? srcH : srcW;
        var src = image.PixelData;
        var dst = new byte[src.Length];

        for (int dstY = 0; dstY < dstH; dstY++)
        {
            for (int dstX = 0; dstX < dstW; dstX++)
            {
                int srcX, srcY;
                switch (degrees)
                {
                    case 90:  srcX = dstY; srcY = srcH - 1 - dstX; break;
                    case 180: srcX = srcW - 1 - dstX; srcY = srcH - 1 - dstY; break;
                    default:  srcX = srcW - 1 - dstY; srcY = dstX; break; // 270
                }
                dst[dstY * dstW + dstX] = src[srcY * srcW + srcX];
            }
        }

        return Result.Success(new ProcessedImage(
            dstW, dstH, image.BitsPerPixel, dst,
            image.WindowCenter, image.WindowWidth, image.FilePath));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-IP-027: Horizontal (left-right) and vertical (top-bottom) flip.
    /// Issue #3.
    /// </remarks>
    public Result<ProcessedImage> Flip(ProcessedImage image, bool horizontal)
    {
        ArgumentNullException.ThrowIfNull(image);

        int w = image.Width, h = image.Height;
        var src = image.PixelData;
        var dst = new byte[src.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int srcIdx = horizontal
                    ? y * w + (w - 1 - x)
                    : (h - 1 - y) * w + x;
                dst[y * w + x] = src[srcIdx];
            }
        }

        return Result.Success(new ProcessedImage(
            w, h, image.BitsPerPixel, dst,
            image.WindowCenter, image.WindowWidth, image.FilePath));
    }

    // @MX:WARN ApplyGainOffsetCorrection - @MX:REASON: Safety-related (SWR-IP-039, HAZ-RAD, HAZ-SW), requires calibration data, returns ErrorCode.CalibrationDataMissing if gainMap/offsetMap null, blocks exposure without calibration
    /// <inheritdoc/>
    /// <remarks>SWR-IP-039 (Safety-related, HAZ-RAD, HAZ-SW)</remarks>
    public Result<ProcessedImage> ApplyGainOffsetCorrection(
        ProcessedImage image,
        float[]? gainMap,
        float[]? offsetMap)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (gainMap is null || offsetMap is null)
            return Result.Failure<ProcessedImage>(
                ErrorCode.CalibrationDataMissing,
                "Gain/Offset calibration data is missing. Exposure blocked. SWR-IP-039.");

        var raw16 = image.RawPixelData16;
        int pixelCount = image.Width * image.Height;

        if (raw16 is null || raw16.Length < pixelCount)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "RawPixelData16 is required for Gain/Offset correction.");

        if (gainMap.Length < pixelCount || offsetMap.Length < pixelCount)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Gain/Offset maps must cover all pixels.");

        // Correct each pixel: corrected = clamp((pixel16 - offset) * gain, 0, 65535)
        // @MX:NOTE Gain/Offset formula: corrected = clamp((pixel16 - offset) * gain, 0, 65535) per SWR-IP-039
        var corrected16 = new ushort[pixelCount];
        for (int i = 0; i < pixelCount; i++)
        {
            float correctedValue = (raw16[i] - offsetMap[i]) * gainMap[i];
            corrected16[i] = (ushort)Math.Clamp((int)Math.Round(correctedValue), 0, 65535);
        }

        // Normalise corrected 16-bit values to 8-bit display buffer.
        ushort min = corrected16[0], max = corrected16[0];
        foreach (var v in corrected16)
        {
            if (v < min) min = v;
            if (v > max) max = v;
        }

        var range = max - min;
        var display = new byte[pixelCount];
        if (range == 0)
            Array.Fill(display, (byte)128);
        else
            for (int i = 0; i < pixelCount; i++)
                display[i] = (byte)Math.Round((corrected16[i] - min) * 255.0 / range);

        var result = new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: 8,
            pixelData: display,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = corrected16
        };

        return Result.Success(result);
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-041 (Safety-related, HAZ-RAD)</remarks>
    public Result<ProcessedImage> ApplyNoiseReduction(ProcessedImage image, double strength)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (strength < 0.0 || strength > 1.0)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Noise reduction strength must be in the range [0.0, 1.0].");

        if (strength == 0.0)
            return Result.Success(image);

        // 3×3 Gaussian kernel: [1,2,1; 2,4,2; 1,2,1] / 16
        var blurred = ApplyGaussian3x3(image.PixelData, image.Width, image.Height);

        // Blend: output = (1 - strength) * original + strength * blurred
        var output = new byte[image.PixelData.Length];
        for (int i = 0; i < output.Length; i++)
        {
            double blended = (1.0 - strength) * image.PixelData[i] + strength * blurred[i];
            output[i] = (byte)Math.Clamp((int)Math.Round(blended), 0, 255);
        }

        return Result.Success(new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-043</remarks>
    public Result<ProcessedImage> ApplyEdgeEnhancement(ProcessedImage image, double strength)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (strength < 0.0 || strength > 1.0)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Edge enhancement strength must be in the range [0.0, 1.0].");

        if (strength == 0.0)
            return Result.Success(image);

        // 5×5 Gaussian blur for unsharp mask.
        var blurred = ApplyGaussian5x5(image.PixelData, image.Width, image.Height);

        // Unsharp mask: enhanced = clamp(original + strength * (original - blurred), 0, 255)
        var output = new byte[image.PixelData.Length];
        for (int i = 0; i < output.Length; i++)
        {
            double enhanced = image.PixelData[i] + strength * (image.PixelData[i] - blurred[i]);
            output[i] = (byte)Math.Clamp((int)Math.Round(enhanced), 0, 255);
        }

        return Result.Success(new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-045 (Safety-related, HAZ-RAD)</remarks>
    public Result<ProcessedImage> ApplyScatterCorrection(ProcessedImage image)
    {
        ArgumentNullException.ThrowIfNull(image);

        // Estimate scatter using a large-kernel Gaussian (radius 15).
        // Subtract 30% of the scatter estimate from the original.
        var scatterEstimate = ApplyLargeGaussian(image.PixelData, image.Width, image.Height, radius: 15);

        var output = new byte[image.PixelData.Length];
        for (int i = 0; i < output.Length; i++)
        {
            double corrected = image.PixelData[i] - 0.3 * scatterEstimate[i];
            output[i] = (byte)Math.Clamp((int)Math.Round(corrected), 0, 255);
        }

        return Result.Success(new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-047</remarks>
    public Result<ProcessedImage> ApplyAutoTrimming(ProcessedImage image, byte threshold = 10)
    {
        ArgumentNullException.ThrowIfNull(image);

        int w = image.Width, h = image.Height;
        var src = image.PixelData;

        // Find bounding box where pixel values exceed threshold.
        int minX = w, maxX = 0, minY = h, maxY = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (src[y * w + x] > threshold)
                {
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
        }

        // If no pixels exceed threshold, return all-black image.
        if (minX > maxX || minY > maxY)
            return Result.Success(new ProcessedImage(
                width: w,
                height: h,
                bitsPerPixel: image.BitsPerPixel,
                pixelData: new byte[src.Length],
                windowCenter: image.WindowCenter,
                windowWidth: image.WindowWidth,
                filePath: image.FilePath,
                panOffsetX: image.PanOffsetX,
                panOffsetY: image.PanOffsetY)
            {
                RawPixelData16 = image.RawPixelData16
            });

        // Mask border region (outside bounding box) to 0, preserve interior.
        var output = new byte[src.Length];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (y >= minY && y <= maxY && x >= minX && x <= maxX)
                    output[y * w + x] = src[y * w + x];
                // else output[y * w + x] = 0 (already zero)
            }
        }

        return Result.Success(new ProcessedImage(
            width: w,
            height: h,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-050</remarks>
    public Result<ProcessedImage> ApplyClahe(ProcessedImage image, double clipLimit = 2.0, int tileSize = 8)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (clipLimit < 1.0 || clipLimit > 4.0)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "CLAHE clip limit must be in the range [1.0, 4.0].");

        if (tileSize < 1)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "CLAHE tile size must be at least 1.");

        int w = image.Width, h = image.Height;
        var src = image.PixelData;

        if (src.Length == 0)
            return Result.Success(image);

        // Divide image into tileSize × tileSize grid and build per-tile LUTs.
        int tilesX = Math.Max(1, (w + tileSize - 1) / tileSize);
        int tilesY = Math.Max(1, (h + tileSize - 1) / tileSize);

        // Compute LUT for each tile.
        var luts = new byte[tilesY, tilesX, 256];

        for (int ty = 0; ty < tilesY; ty++)
        {
            for (int tx = 0; tx < tilesX; tx++)
            {
                int x0 = tx * tileSize;
                int y0 = ty * tileSize;
                int x1 = Math.Min(x0 + tileSize, w);
                int y1 = Math.Min(y0 + tileSize, h);
                int tilePixels = (x1 - x0) * (y1 - y0);

                // Build histogram for this tile.
                var hist = new int[256];
                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                        hist[src[y * w + x]]++;

                // Clip histogram at clipLimit.
                // Minimum clip threshold of 1 ensures histogram values are never fully suppressed.
                int clipThreshold = Math.Max(1, (int)Math.Round(clipLimit * tilePixels / 256.0));
                int redistributed = 0;
                for (int v = 0; v < 256; v++)
                {
                    if (hist[v] > clipThreshold)
                    {
                        redistributed += hist[v] - clipThreshold;
                        hist[v] = clipThreshold;
                    }
                }

                // Redistribute clipped values evenly.
                int redistPerBin = redistributed / 256;
                int redistRemainder = redistributed % 256;
                for (int v = 0; v < 256; v++)
                {
                    hist[v] += redistPerBin;
                    if (v < redistRemainder)
                        hist[v]++;
                }

                // Build cumulative distribution function (CDF) and derive LUT.
                int cdf = 0;
                for (int v = 0; v < 256; v++)
                {
                    cdf += hist[v];
                    luts[ty, tx, v] = (byte)Math.Clamp(
                        (int)Math.Round((double)cdf * 255.0 / tilePixels), 0, 255);
                }
            }
        }

        // Bilinear interpolation between tile LUTs for each output pixel.
        var output = new byte[src.Length];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                byte pv = src[y * w + x];

                // Find the four surrounding tile centres.
                double tileXPos = (x - tileSize / 2.0) / tileSize;
                double tileYPos = (y - tileSize / 2.0) / tileSize;

                int tx0 = (int)Math.Floor(tileXPos);
                int ty0 = (int)Math.Floor(tileYPos);
                int tx1 = tx0 + 1;
                int ty1 = ty0 + 1;

                double fracX = tileXPos - tx0;
                double fracY = tileYPos - ty0;

                // Clamp tile indices.
                int clTx0 = Math.Clamp(tx0, 0, tilesX - 1);
                int clTx1 = Math.Clamp(tx1, 0, tilesX - 1);
                int clTy0 = Math.Clamp(ty0, 0, tilesY - 1);
                int clTy1 = Math.Clamp(ty1, 0, tilesY - 1);

                double v00 = luts[clTy0, clTx0, pv];
                double v10 = luts[clTy0, clTx1, pv];
                double v01 = luts[clTy1, clTx0, pv];
                double v11 = luts[clTy1, clTx1, pv];

                double interpolated =
                    v00 * (1 - fracX) * (1 - fracY) +
                    v10 * fracX * (1 - fracY) +
                    v01 * (1 - fracX) * fracY +
                    v11 * fracX * fracY;

                output[y * w + x] = (byte)Math.Clamp((int)Math.Round(interpolated), 0, 255);
            }
        }

        return Result.Success(new ProcessedImage(
            width: w,
            height: h,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-052</remarks>
    public Result<ProcessedImage> ApplyBrightnessOffset(ProcessedImage image, int offset)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (offset < -255 || offset > 255)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Brightness offset must be in the range [-255, 255].");

        var src = image.PixelData;
        var output = new byte[src.Length];
        for (int i = 0; i < src.Length; i++)
            output[i] = (byte)Math.Clamp(src[i] + offset, 0, 255);

        return Result.Success(new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    /// <inheritdoc/>
    /// <remarks>SWR-IP-049. Masking is O(W×H) — well within the ≤100ms requirement.</remarks>
    public Result<ProcessedImage> ApplyBlackMask(
        ProcessedImage image,
        int left,
        int top,
        int right,
        int bottom,
        bool apply = true)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (left < 0 || top < 0 || right > image.Width || bottom > image.Height || left >= right || top >= bottom)
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "Black mask boundary is out of range or degenerate.");

        var src = image.PixelData;
        var output = new byte[src.Length];

        if (apply)
        {
            // Mask pixels outside [left,right) × [top,bottom) to black (0).
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int idx = y * image.Width + x;
                    output[idx] = (x >= left && x < right && y >= top && y < bottom)
                        ? src[idx]
                        : (byte)0;
                }
            }
        }
        else
        {
            // Remove mask: restore from RawPixelData16 (normalised to 8-bit) if available,
            // otherwise return the source pixels unchanged (mask was not applied).
            if (image.RawPixelData16 is { } raw16 && raw16.Length == src.Length)
            {
                // Re-normalise 16-bit source to 8-bit, respecting current W/L.
                double lowerBound = image.WindowCenter - image.WindowWidth / 2.0;
                for (int i = 0; i < raw16.Length; i++)
                {
                    double normalised = (raw16[i] - lowerBound) / image.WindowWidth;
                    output[i] = (byte)(Math.Clamp(normalised, 0.0, 1.0) * 255.0);
                }
            }
            else
            {
                Array.Copy(src, output, src.Length);
            }
        }

        return Result.Success(new ProcessedImage(
            width: image.Width,
            height: image.Height,
            bitsPerPixel: image.BitsPerPixel,
            pixelData: output,
            windowCenter: image.WindowCenter,
            windowWidth: image.WindowWidth,
            filePath: image.FilePath,
            panOffsetX: image.PanOffsetX,
            panOffsetY: image.PanOffsetY)
        {
            RawPixelData16 = image.RawPixelData16
        });
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>
    /// Processes a successfully parsed DICOM file, reading pixel data and window/level
    /// from DICOM tags (0028,0010/0011/0100/7FE0,0010 and VOI LUT tags).
    /// Handles 8-bit and 16-bit pixel data, normalising 16-bit to 8-bit display bytes.
    /// </summary>
    /// <param name="dicomFile">Parsed DICOM file.</param>
    /// <param name="rawImagePath">Original file path, stored on the result.</param>
    /// <param name="parameters">Processing parameters including window override and auto-window flag.</param>
    private static Result<ProcessedImage> ProcessDicomFile(
        DicomFile dicomFile,
        string rawImagePath,
        ProcessingParameters parameters)
    {
        var dataset = dicomFile.Dataset;

        // Read image geometry from mandatory DICOM tags.
        int rows = dataset.GetSingleValueOrDefault(DicomTag.Rows, (ushort)0);
        int columns = dataset.GetSingleValueOrDefault(DicomTag.Columns, (ushort)0);
        int bitsAllocated = dataset.GetSingleValueOrDefault(DicomTag.BitsAllocated, (ushort)8);

        if (rows <= 0 || columns <= 0)
        {
            return Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed,
                "DICOM file has invalid Rows or Columns tag.");
        }

        // Extract raw pixel bytes from PixelData tag.
        byte[] pixelBytes = ExtractDicomPixelBytes(dataset);

        // Normalise 16-bit pixel data to 8-bit display bytes for rendering.
        // Preserve original 16-bit buffer in RawPixelData16 for ROI statistics
        // and brightness offset operations (SWR-IP-036, SWR-IP-052). Issue #8.
        byte[] displayPixels;
        int effectiveBitsPerPixel;
        ushort[]? rawPixelData16 = null;

        if (bitsAllocated == 16)
        {
            // Preserve raw 16-bit values before normalisation.
            int pixelCount = rows * columns;
            rawPixelData16 = ExtractRaw16BitValues(pixelBytes, pixelCount);
            displayPixels = Normalize16BitTo8Bit(pixelBytes, pixelCount);
            effectiveBitsPerPixel = 8;
        }
        else
        {
            displayPixels = pixelBytes;
            effectiveBitsPerPixel = bitsAllocated;
        }

        // Determine window/level values: explicit override > DICOM VOI LUT tags > statistical auto.
        double windowCenter;
        double windowWidth;

        if (parameters.WindowCenter.HasValue && parameters.WindowWidth.HasValue)
        {
            windowCenter = parameters.WindowCenter.Value;
            windowWidth = parameters.WindowWidth.Value;
        }
        else if (parameters.AutoWindow)
        {
            // Try DICOM VOI LUT window tags first (0028,1050) and (0028,1051).
            bool hasDicomCenter = dataset.TryGetSingleValue(DicomTag.WindowCenter, out double dcmCenter);
            bool hasDicomWidth = dataset.TryGetSingleValue(DicomTag.WindowWidth, out double dcmWidth);

            if (hasDicomCenter && hasDicomWidth)
            {
                windowCenter = dcmCenter;
                windowWidth = Math.Max(MinWindowWidth, dcmWidth);
            }
            else
            {
                // Fall back to statistical computation on display pixels.
                (windowCenter, windowWidth) = ComputeAutoWindow(displayPixels);
            }
        }
        else
        {
            windowCenter = 128.0;
            windowWidth = 256.0;
        }

        var image = new ProcessedImage(
            width: columns,
            height: rows,
            bitsPerPixel: effectiveBitsPerPixel,
            pixelData: displayPixels,
            windowCenter: windowCenter,
            windowWidth: windowWidth,
            filePath: rawImagePath)
        {
            RawPixelData16 = rawPixelData16
        };

        return Result.Success(image);
    }

    /// <summary>
    /// Extracts raw 16-bit pixel values from a little-endian byte buffer.
    /// Used to preserve original DR/CR pixel data before 8-bit normalisation.
    /// SWR-IP-036 (ROI statistics 0–65535 range) / Issue #8.
    /// </summary>
    private static ushort[] ExtractRaw16BitValues(byte[] rawBytes, int pixelCount)
    {
        if (rawBytes.Length < pixelCount * 2)
            pixelCount = rawBytes.Length / 2;
        var values = new ushort[pixelCount];
        for (int i = 0; i < pixelCount; i++)
            values[i] = BitConverter.ToUInt16(rawBytes, i * 2);
        return values;
    }

    /// <summary>
    /// Processes a non-DICOM (raw) file using simple square-root dimension estimation.
    /// Maintains backward compatibility for raw file test coverage.
    /// </summary>
    /// <param name="rawImagePath">Path to the raw image file.</param>
    /// <param name="parameters">Processing parameters.</param>
    private static Result<ProcessedImage> ProcessRawFile(
        string rawImagePath,
        ProcessingParameters parameters)
    {
        var fileBytes = File.ReadAllBytes(rawImagePath);

        double windowCenter;
        double windowWidth;

        if (parameters.WindowCenter.HasValue && parameters.WindowWidth.HasValue)
        {
            windowCenter = parameters.WindowCenter.Value;
            windowWidth = parameters.WindowWidth.Value;
        }
        else if (parameters.AutoWindow)
        {
            (windowCenter, windowWidth) = ComputeAutoWindow(fileBytes);
        }
        else
        {
            windowCenter = 128.0;
            windowWidth = 256.0;
        }

        // Width/height: prefer caller-supplied dimensions (SWR-IP-020 / Issue #7).
        // Fallback: square-root estimation (inaccurate for non-square DR images).
        int width, height;
        if (parameters.RawImageWidth.HasValue && parameters.RawImageHeight.HasValue
            && parameters.RawImageWidth.Value > 0 && parameters.RawImageHeight.Value > 0)
        {
            width = parameters.RawImageWidth.Value;
            height = parameters.RawImageHeight.Value;
        }
        else
        {
            // Fallback: assumes square image (incorrect for most DR/CR images).
            // Callers should always provide explicit dimensions for production raw files.
            var pixelCount = fileBytes.Length;
            var side = (int)Math.Sqrt(pixelCount);
            width = side > 0 ? side : 1;
            height = pixelCount / width;
        }

        var image = new ProcessedImage(
            width: width,
            height: height,
            bitsPerPixel: 8,
            pixelData: fileBytes,
            windowCenter: windowCenter,
            windowWidth: windowWidth,
            filePath: rawImagePath);

        return Result.Success(image);
    }

    /// <summary>
    /// Extracts raw pixel bytes from a DICOM dataset's PixelData element (7FE0,0010).
    /// Returns an empty array if the tag is absent.
    /// </summary>
    /// <param name="dataset">DICOM dataset containing the PixelData tag.</param>
    private static byte[] ExtractDicomPixelBytes(DicomDataset dataset)
    {
        if (!dataset.Contains(DicomTag.PixelData))
            return Array.Empty<byte>();

        var pixelDataElement = dataset.GetDicomItem<DicomItem>(DicomTag.PixelData);

        if (pixelDataElement is DicomOtherByte otherByte)
            return otherByte.Get<byte[]>() ?? Array.Empty<byte>();

        if (pixelDataElement is DicomOtherWord otherWord)
        {
            // Convert ushort[] to byte[] (little-endian).
            var words = otherWord.Get<ushort[]>() ?? Array.Empty<ushort>();
            var bytes = new byte[words.Length * 2];
            Buffer.BlockCopy(words, 0, bytes, 0, bytes.Length);
            return bytes;
        }

        if (pixelDataElement is DicomFragmentSequence fragmentSeq)
        {
            // For encapsulated (compressed) pixel data, concatenate all fragments.
            using var ms = new MemoryStream();
            foreach (var fragment in fragmentSeq)
                ms.Write(fragment.Data, 0, fragment.Data.Length);
            return ms.ToArray();
        }

        return Array.Empty<byte>();
    }

    /// <summary>
    /// Normalises 16-bit pixel data (stored as byte pairs, little-endian) to 8-bit display values.
    /// Performs a linear min-max stretch across the actual pixel value range.
    /// </summary>
    /// <param name="rawBytes">Raw pixel byte buffer from the DICOM PixelData tag.</param>
    /// <param name="pixelCount">Expected number of pixels (rows × columns).</param>
    // @MX:NOTE Normalize16BitTo8Bit preserves full 0-65535 range for ROI statistics (SWR-IP-036), Issue #8
    private static byte[] Normalize16BitTo8Bit(byte[] rawBytes, int pixelCount)
    {
        // Guard: require at least 2 bytes per pixel.
        if (rawBytes.Length < pixelCount * 2)
            pixelCount = rawBytes.Length / 2;

        if (pixelCount == 0)
            return Array.Empty<byte>();

        var values = new ushort[pixelCount];
        for (var i = 0; i < pixelCount; i++)
            values[i] = (ushort)(rawBytes[i * 2] | (rawBytes[i * 2 + 1] << 8));

        ushort min = values[0];
        ushort max = values[0];
        foreach (var v in values)
        {
            if (v < min) min = v;
            if (v > max) max = v;
        }

        var range = max - min;
        var output = new byte[pixelCount];

        if (range == 0)
        {
            // Flat image — map to mid-grey.
            Array.Fill(output, (byte)128);
            return output;
        }

        for (var i = 0; i < pixelCount; i++)
            output[i] = (byte)Math.Round((values[i] - min) * 255.0 / range);

        return output;
    }

    /// <summary>
    /// Performs bilinear resampling of a row-major 8-bit pixel buffer to new dimensions.
    /// Maps each output pixel back to the source grid and interpolates among the four
    /// nearest source pixels.
    /// </summary>
    /// <param name="source">Input pixel buffer in row-major order (8 bits per pixel).</param>
    /// <param name="srcWidth">Width of the source image in pixels.</param>
    /// <param name="srcHeight">Height of the source image in pixels.</param>
    /// <param name="dstWidth">Desired output width in pixels.</param>
    /// <param name="dstHeight">Desired output height in pixels.</param>
    // @MX:NOTE Bilinear resampling maps each destination pixel back to source coordinates, interpolates among 4 nearest pixels
    private static byte[] BilinearResample(
        byte[] source,
        int srcWidth,
        int srcHeight,
        int dstWidth,
        int dstHeight)
    {
        if (source.Length == 0 || srcWidth <= 0 || srcHeight <= 0)
            return new byte[dstWidth * dstHeight];

        var output = new byte[dstWidth * dstHeight];

        double xScale = (double)srcWidth / dstWidth;
        double yScale = (double)srcHeight / dstHeight;

        for (var dstY = 0; dstY < dstHeight; dstY++)
        {
            for (var dstX = 0; dstX < dstWidth; dstX++)
            {
                // Map destination pixel centre back to source coordinate space.
                double srcX = (dstX + 0.5) * xScale - 0.5;
                double srcY = (dstY + 0.5) * yScale - 0.5;

                int x0 = (int)Math.Floor(srcX);
                int y0 = (int)Math.Floor(srcY);
                int x1 = x0 + 1;
                int y1 = y0 + 1;

                // Clamp to valid source bounds.
                x0 = Math.Clamp(x0, 0, srcWidth - 1);
                x1 = Math.Clamp(x1, 0, srcWidth - 1);
                y0 = Math.Clamp(y0, 0, srcHeight - 1);
                y1 = Math.Clamp(y1, 0, srcHeight - 1);

                double tx = srcX - Math.Floor(srcX);
                double ty = srcY - Math.Floor(srcY);

                // Bilinear interpolation of the four surrounding source pixels.
                double p00 = source[y0 * srcWidth + x0];
                double p10 = source[y0 * srcWidth + x1];
                double p01 = source[y1 * srcWidth + x0];
                double p11 = source[y1 * srcWidth + x1];

                double interpolated =
                    p00 * (1 - tx) * (1 - ty) +
                    p10 * tx * (1 - ty) +
                    p01 * (1 - tx) * ty +
                    p11 * tx * ty;

                output[dstY * dstWidth + dstX] = (byte)Math.Round(interpolated);
            }
        }

        return output;
    }

    /// <summary>
    /// Resamples using Bicubic interpolation (Catmull-Rom kernel).
    /// Used for upscaling (factor &gt; 1) per SWR-IP-024. Issue #4.
    /// </summary>
    // @MX:NOTE Bicubic uses Catmull-Rom kernel, 4×4 neighborhood for upscaling (factor > 1), prevents aliasing per SWR-IP-024
    private static byte[] BicubicResample(
        byte[] source, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
    {
        if (source.Length == 0 || srcWidth <= 0 || srcHeight <= 0)
            return new byte[dstWidth * dstHeight];

        var output = new byte[dstWidth * dstHeight];
        double xScale = (double)srcWidth / dstWidth;
        double yScale = (double)srcHeight / dstHeight;

        static double CubicWeight(double t)
        {
            t = Math.Abs(t);
            return t < 1.0
                ? 1.5 * t * t * t - 2.5 * t * t + 1.0
                : t < 2.0 ? -0.5 * t * t * t + 2.5 * t * t - 4.0 * t + 2.0 : 0.0;
        }

        for (int dstY = 0; dstY < dstHeight; dstY++)
        {
            for (int dstX = 0; dstX < dstWidth; dstX++)
            {
                double srcX = (dstX + 0.5) * xScale - 0.5;
                double srcY = (dstY + 0.5) * yScale - 0.5;
                int x0 = (int)Math.Floor(srcX);
                int y0 = (int)Math.Floor(srcY);
                double sum = 0, weightSum = 0;
                for (int m = -1; m <= 2; m++)
                    for (int n = -1; n <= 2; n++)
                    {
                        int sx = Math.Clamp(x0 + n, 0, srcWidth - 1);
                        int sy = Math.Clamp(y0 + m, 0, srcHeight - 1);
                        double w = CubicWeight(srcX - (x0 + n)) * CubicWeight(srcY - (y0 + m));
                        sum += source[sy * srcWidth + sx] * w;
                        weightSum += w;
                    }
                output[dstY * dstWidth + dstX] = (byte)Math.Clamp(
                    weightSum > 0 ? Math.Round(sum / weightSum) : 0, 0, 255);
            }
        }
        return output;
    }

    /// <summary>
    /// Resamples using Area Average (box filter) for downscaling.
    /// Prevents aliasing artefacts per SWR-IP-024. Issue #4.
    /// </summary>
    // @MX:NOTE Area Average uses box filter for downscaling (factor < 1), prevents aliasing per SWR-IP-024
    private static byte[] AreaAverageResample(
        byte[] source, int srcWidth, int srcHeight, int dstWidth, int dstHeight)
    {
        if (source.Length == 0 || srcWidth <= 0 || srcHeight <= 0)
            return new byte[dstWidth * dstHeight];

        var output = new byte[dstWidth * dstHeight];
        double xScale = (double)srcWidth / dstWidth;
        double yScale = (double)srcHeight / dstHeight;

        for (int dstY = 0; dstY < dstHeight; dstY++)
        {
            int sy0 = (int)Math.Floor(dstY * yScale);
            int sy1 = Math.Min((int)Math.Ceiling((dstY + 1) * yScale), srcHeight);
            for (int dstX = 0; dstX < dstWidth; dstX++)
            {
                int sx0 = (int)Math.Floor(dstX * xScale);
                int sx1 = Math.Min((int)Math.Ceiling((dstX + 1) * xScale), srcWidth);
                double sum = 0;
                int count = 0;
                for (int sy = sy0; sy < sy1; sy++)
                    for (int sx = sx0; sx < sx1; sx++)
                    {
                        sum += source[sy * srcWidth + sx];
                        count++;
                    }
                output[dstY * dstWidth + dstX] = (byte)Math.Round(count > 0 ? sum / count : 0);
            }
        }
        return output;
    }

    /// <summary>
    /// Computes auto window centre and width from 8-bit pixel statistics.
    /// Uses mean as centre and 2× standard deviation as width.
    /// </summary>
    /// <param name="pixelData">8-bit display pixel buffer.</param>
    private static (double center, double width) ComputeAutoWindow(byte[] pixelData)
    {
        if (pixelData.Length == 0)
            return (128.0, 256.0);

        double sum = 0;
        foreach (var b in pixelData)
            sum += b;

        var mean = sum / pixelData.Length;

        double variance = 0;
        foreach (var b in pixelData)
            variance += (b - mean) * (b - mean);

        variance /= pixelData.Length;
        var stdDev = Math.Sqrt(variance);

        var width = Math.Max(MinWindowWidth, stdDev * 2.0);
        return (mean, width);
    }

    /// <summary>
    /// Applies a 3×3 Gaussian blur kernel to an 8-bit pixel buffer.
    /// Kernel: [1,2,1; 2,4,2; 1,2,1] / 16. Border pixels use clamped sampling.
    /// </summary>
    private static byte[] ApplyGaussian3x3(byte[] source, int w, int h)
    {
        if (source.Length == 0 || w <= 0 || h <= 0)
            return Array.Empty<byte>();

        // 3×3 Gaussian kernel coefficients (sum = 16).
        ReadOnlySpan<int> kernel = [1, 2, 1, 2, 4, 2, 1, 2, 1];
        var output = new byte[source.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sum = 0;
                int ki = 0;
                for (int ky = -1; ky <= 1; ky++)
                {
                    int sy = Math.Clamp(y + ky, 0, h - 1);
                    for (int kx = -1; kx <= 1; kx++, ki++)
                    {
                        int sx = Math.Clamp(x + kx, 0, w - 1);
                        sum += source[sy * w + sx] * kernel[ki];
                    }
                }
                output[y * w + x] = (byte)(sum / 16);
            }
        }

        return output;
    }

    /// <summary>
    /// Applies a 5×5 Gaussian blur kernel to an 8-bit pixel buffer.
    /// Uses a separable approximation based on Pascal's triangle (binomial coefficients row 4).
    /// Border pixels use clamped sampling.
    /// </summary>
    private static byte[] ApplyGaussian5x5(byte[] source, int w, int h)
    {
        if (source.Length == 0 || w <= 0 || h <= 0)
            return Array.Empty<byte>();

        // 5×5 Gaussian kernel coefficients (outer product of [1,4,6,4,1]).
        // Sum = 256.
        ReadOnlySpan<int> row = [1, 4, 6, 4, 1];
        var output = new byte[source.Length];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sum = 0;
                for (int ky = -2; ky <= 2; ky++)
                {
                    int sy = Math.Clamp(y + ky, 0, h - 1);
                    for (int kx = -2; kx <= 2; kx++)
                    {
                        int sx = Math.Clamp(x + kx, 0, w - 1);
                        sum += source[sy * w + sx] * row[ky + 2] * row[kx + 2];
                    }
                }
                output[y * w + x] = (byte)Math.Clamp(sum / 256, 0, 255);
            }
        }

        return output;
    }

    /// <summary>
    /// Applies a large box-approximated Gaussian blur to estimate low-frequency scatter content.
    /// Uses iterated box filters to approximate a Gaussian of the requested radius.
    /// </summary>
    private static byte[] ApplyLargeGaussian(byte[] source, int w, int h, int radius)
    {
        if (source.Length == 0 || w <= 0 || h <= 0)
            return Array.Empty<byte>();

        // Use iterated box filter as an efficient Gaussian approximation.
        // Three passes with box of size (2*r+1) converge to a Gaussian.
        int boxSize = radius * 2 + 1;
        var current = (byte[])source.Clone();

        for (int pass = 0; pass < 3; pass++)
            current = BoxBlur(current, w, h, boxSize);

        return current;
    }

    /// <summary>
    /// Applies a separable box blur of the given kernel size to an 8-bit pixel buffer.
    /// Implements horizontal then vertical pass for O(N) per-pixel complexity.
    /// </summary>
    private static byte[] BoxBlur(byte[] source, int w, int h, int boxSize)
    {
        int half = boxSize / 2;
        var temp = new byte[source.Length];
        var output = new byte[source.Length];

        // Horizontal pass.
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sum = 0;
                for (int kx = -half; kx <= half; kx++)
                {
                    int sx = Math.Clamp(x + kx, 0, w - 1);
                    sum += source[y * w + sx];
                }
                temp[y * w + x] = (byte)(sum / boxSize);
            }
        }

        // Vertical pass.
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int sum = 0;
                for (int ky = -half; ky <= half; ky++)
                {
                    int sy = Math.Clamp(y + ky, 0, h - 1);
                    sum += temp[sy * w + x];
                }
                output[y * w + x] = (byte)(sum / boxSize);
            }
        }

        return output;
    }
}
