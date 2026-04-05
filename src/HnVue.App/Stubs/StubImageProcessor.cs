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
}
