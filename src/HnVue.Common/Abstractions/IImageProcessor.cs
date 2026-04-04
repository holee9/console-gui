using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

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
}
