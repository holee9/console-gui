using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="IImageProcessor"/> used until the HnVue.Imaging module
/// is integrated in Wave 3.
/// All operations return failure results with a descriptive message.
/// </summary>
internal sealed class StubImageProcessor : IImageProcessor
{
    private const string NotImplementedMessage =
        "ImageProcessor not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public Task<Result<ProcessedImage>> ProcessAsync(
        string rawImagePath,
        ProcessingParameters parameters,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<ProcessedImage>(
            ErrorCode.ImageProcessingFailed, NotImplementedMessage));

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyWindowLevel(
        ProcessedImage image,
        double windowCenter,
        double windowWidth)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> Zoom(
        ProcessedImage image,
        double factor)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> Pan(
        ProcessedImage image,
        int deltaX,
        int deltaY)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> Rotate(ProcessedImage image, int degrees)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> Flip(ProcessedImage image, bool horizontal)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyGainOffsetCorrection(
        ProcessedImage image, float[]? gainMap, float[]? offsetMap)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyNoiseReduction(ProcessedImage image, double strength)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyEdgeEnhancement(ProcessedImage image, double strength)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyScatterCorrection(ProcessedImage image)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyAutoTrimming(ProcessedImage image, byte threshold = 10)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyClahe(ProcessedImage image, double clipLimit = 2.0, int tileSize = 8)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyBrightnessOffset(ProcessedImage image, int offset)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);

    /// <inheritdoc/>
    public Result<ProcessedImage> ApplyBlackMask(
        ProcessedImage image, int left, int top, int right, int bottom, bool apply = true)
        => Result.Failure<ProcessedImage>(ErrorCode.ImageProcessingFailed, NotImplementedMessage);
}
