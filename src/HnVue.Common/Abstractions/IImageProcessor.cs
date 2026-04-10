using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IImageProcessor - @MX:REASON: 17 image manipulation operations, clinical display pipeline, safety-critical calibration correction
/// <summary>
/// Defines image processing and interactive display manipulation operations.
/// Implemented by the HnVue.Imaging module.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Loads a raw detector image from disk and applies the specified processing parameters.
    /// </summary>
    /// <param name="rawImagePath">Absolute path to the raw image file (DICOM or proprietary format).</param>
    /// <param name="parameters">Processing options such as windowing and auto-level settings.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the processed image ready for display,
    /// or a failure with <see cref="ErrorCode.ImageProcessingFailed"/>.
    /// </returns>
    Task<Result<ProcessedImage>> ProcessAsync(
        string rawImagePath,
        ProcessingParameters parameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a window/level transformation to an already-loaded image.
    /// This is a synchronous, in-memory operation.
    /// </summary>
    /// <param name="image">Source image to transform.</param>
    /// <param name="windowCenter">New DICOM window centre value.</param>
    /// <param name="windowWidth">New DICOM window width value.</param>
    Result<ProcessedImage> ApplyWindowLevel(
        ProcessedImage image,
        double windowCenter,
        double windowWidth);

    /// <summary>
    /// Returns a new image scaled by the specified zoom factor around its centre.
    /// </summary>
    /// <param name="image">Source image to scale.</param>
    /// <param name="factor">Zoom factor; values greater than 1 zoom in, less than 1 zoom out.</param>
    Result<ProcessedImage> Zoom(
        ProcessedImage image,
        double factor);

    /// <summary>
    /// Returns a new image shifted by the specified pixel deltas (pan operation).
    /// </summary>
    /// <param name="image">Source image to pan.</param>
    /// <param name="deltaX">Horizontal pixel offset; positive values shift right.</param>
    /// <param name="deltaY">Vertical pixel offset; positive values shift down.</param>
    Result<ProcessedImage> Pan(
        ProcessedImage image,
        int deltaX,
        int deltaY);

    /// <summary>
    /// Rotates the image by the specified angle in degrees.
    /// Supported angles: 90, 180, 270 (clockwise). Uses Bilinear interpolation.
    /// SWR-IP-027.
    /// </summary>
    /// <param name="image">Source image to rotate.</param>
    /// <param name="degrees">Rotation angle in degrees clockwise. Must be 90, 180, or 270.</param>
    Result<ProcessedImage> Rotate(ProcessedImage image, int degrees);

    /// <summary>
    /// Flips the image horizontally or vertically.
    /// SWR-IP-027.
    /// </summary>
    /// <param name="image">Source image to flip.</param>
    /// <param name="horizontal">
    /// When <see langword="true"/>, flips left-right.
    /// When <see langword="false"/>, flips top-bottom.
    /// </param>
    Result<ProcessedImage> Flip(ProcessedImage image, bool horizontal);

    /// <summary>
    /// Applies Gain/Offset calibration correction to a 16-bit raw image.
    /// SWR-IP-039 (Safety-related, HAZ-RAD, HAZ-SW).
    /// When gainMap or offsetMap is null, returns <see cref="ErrorCode.CalibrationDataMissing"/>
    /// failure to block exposure.
    /// </summary>
    /// <param name="image">Source image whose <see cref="ProcessedImage.RawPixelData16"/> is used.</param>
    /// <param name="gainMap">Per-pixel gain correction factors (must match pixel count).</param>
    /// <param name="offsetMap">Per-pixel offset correction values (must match pixel count).</param>
    Result<ProcessedImage> ApplyGainOffsetCorrection(
        ProcessedImage image,
        float[]? gainMap,
        float[]? offsetMap);

    /// <summary>
    /// Applies adaptive noise reduction using a weighted Gaussian kernel.
    /// SWR-IP-041 (Safety-related, HAZ-RAD).
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="strength">NR strength 0.0–1.0. 0 = no change, 1 = maximum smoothing.</param>
    Result<ProcessedImage> ApplyNoiseReduction(ProcessedImage image, double strength);

    /// <summary>
    /// Applies edge enhancement using unsharp masking.
    /// SWR-IP-043. Strength 0.0 = no enhancement, 1.0 = maximum.
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="strength">Enhancement strength 0.0–1.0.</param>
    Result<ProcessedImage> ApplyEdgeEnhancement(ProcessedImage image, double strength);

    /// <summary>
    /// Applies software-based scatter correction using Gaussian blur subtraction.
    /// SWR-IP-045 (Safety-related, HAZ-RAD).
    /// </summary>
    /// <param name="image">Source image to process.</param>
    Result<ProcessedImage> ApplyScatterCorrection(ProcessedImage image);

    /// <summary>
    /// Auto-detects dark border regions and applies a black mask to trim them.
    /// SWR-IP-047. Uses threshold-based border detection.
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="threshold">
    /// Pixel value threshold (0–255) below which border is considered black. Default 10.
    /// </param>
    Result<ProcessedImage> ApplyAutoTrimming(ProcessedImage image, byte threshold = 10);

    /// <summary>
    /// Applies Contrast Limited Adaptive Histogram Equalization (CLAHE).
    /// SWR-IP-050. Operates on 8-bit pixel data.
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="clipLimit">Contrast clip limit 1.0–4.0 (default 2.0). Higher = more contrast.</param>
    /// <param name="tileSize">Tile grid size (default 8). Image divided into tileSize×tileSize grid.</param>
    Result<ProcessedImage> ApplyClahe(ProcessedImage image, double clipLimit = 2.0, int tileSize = 8);

    /// <summary>
    /// Applies a brightness offset to the image.
    /// SWR-IP-052. Adds offset to each pixel, clamped to [0, 255].
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="offset">Brightness offset -255 to +255.</param>
    Result<ProcessedImage> ApplyBrightnessOffset(ProcessedImage image, int offset);

    /// <summary>
    /// Applies or removes a Black Mask (Automatic Shutters) to the image.
    /// SWR-IP-049 (Functional). Masks pixels outside the specified boundary to black (0).
    /// When <paramref name="apply"/> is <see langword="false"/>, restores original pixels from
    /// <see cref="ProcessedImage.RawPixelData16"/> if available, otherwise un-masks using stored data.
    /// Masked pixels are excluded from Window/Level calculations.
    /// </summary>
    /// <param name="image">Source image to process.</param>
    /// <param name="left">Left boundary (inclusive) of the unmasked region (pixels from left edge).</param>
    /// <param name="top">Top boundary (inclusive) of the unmasked region (pixels from top edge).</param>
    /// <param name="right">Right boundary (exclusive) of the unmasked region (pixels from left edge).</param>
    /// <param name="bottom">Bottom boundary (exclusive) of the unmasked region (pixels from top edge).</param>
    /// <param name="apply">
    /// When <see langword="true"/>, applies the black mask outside the boundary.
    /// When <see langword="false"/>, removes the mask (restores original pixel values).
    /// </param>
    Result<ProcessedImage> ApplyBlackMask(
        ProcessedImage image,
        int left,
        int top,
        int right,
        int bottom,
        bool apply = true);
}
