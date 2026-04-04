using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="ImageViewerViewModel"/>.
/// </summary>
public sealed class ImageViewerViewModelTests
{
    private readonly IImageProcessor _imageProcessor = Substitute.For<IImageProcessor>();

    private static ProcessedImage MakeImage(double wc = 1024, double ww = 2048) =>
        new(width: 512, height: 512, bitsPerPixel: 12,
            pixelData: new byte[512 * 512 * 2],
            windowCenter: wc, windowWidth: ww,
            filePath: "/tmp/test.dcm");

    private ImageViewerViewModel CreateSut() => new(_imageProcessor);

    [Fact]
    public void Constructor_SetsDefaultProperties()
    {
        var sut = CreateSut();

        sut.CurrentImagePath.Should().BeNull();
        sut.IsImageLoaded.Should().BeFalse();
        sut.ZoomFactor.Should().Be(1.0);
    }

    [Fact]
    public async Task LoadImageCommand_Success_SetsIsImageLoadedTrue()
    {
        var image = MakeImage();
        _imageProcessor
            .ProcessAsync("/path/to/image.dcm", Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/path/to/image.dcm");

        sut.IsImageLoaded.Should().BeTrue();
    }

    [Fact]
    public async Task LoadImageCommand_Success_SetsCurrentImagePath()
    {
        var image = MakeImage();
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/path/to/image.dcm");

        sut.CurrentImagePath.Should().Be("/path/to/image.dcm");
    }

    [Fact]
    public async Task LoadImageCommand_Success_SetsWindowLevelFromImage()
    {
        var image = MakeImage(wc: 500, ww: 1000);
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/any.dcm");

        sut.WindowLevel.Should().Be(500);
        sut.WindowWidth.Should().Be(1000);
    }

    [Fact]
    public async Task LoadImageCommand_Failure_SetsErrorMessageAndIsImageLoadedFalse()
    {
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<ProcessedImage>(
                ErrorCode.ImageProcessingFailed, "File not found."));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/bad/path.dcm");

        sut.IsImageLoaded.Should().BeFalse();
        sut.ErrorMessage.Should().Be("File not found.");
    }

    [Fact]
    public async Task ZoomInCommand_IncreasesZoomFactor()
    {
        var image = MakeImage();
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));
        _imageProcessor
            .Zoom(Arg.Any<ProcessedImage>(), Arg.Any<double>())
            .Returns(args => Result.Success<ProcessedImage>(MakeImage()));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/image.dcm");
        sut.ZoomInCommand.Execute(null);

        sut.ZoomFactor.Should().BeApproximately(1.25, 0.001);
    }

    [Fact]
    public async Task ZoomOutCommand_DecreasesZoomFactor()
    {
        var image = MakeImage();
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));
        _imageProcessor
            .Zoom(Arg.Any<ProcessedImage>(), Arg.Any<double>())
            .Returns(Result.Success<ProcessedImage>(MakeImage()));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/image.dcm");
        sut.ZoomOutCommand.Execute(null);

        sut.ZoomFactor.Should().BeApproximately(0.75, 0.001);
    }

    [Fact]
    public async Task ResetWindowCommand_RestoresOriginalWindowValues()
    {
        var image = MakeImage(wc: 400, ww: 800);
        _imageProcessor
            .ProcessAsync(Arg.Any<string>(), Arg.Any<ProcessingParameters>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<ProcessedImage>(image));
        _imageProcessor
            .ApplyWindowLevel(Arg.Any<ProcessedImage>(), Arg.Any<double>(), Arg.Any<double>())
            .Returns(Result.Success<ProcessedImage>(image));

        var sut = CreateSut();
        await sut.LoadImageCommand.ExecuteAsync("/image.dcm");

        // Manually change window values.
        sut.WindowLevel = 9999;
        sut.WindowWidth = 9999;

        sut.ResetWindowCommand.Execute(null);

        sut.WindowLevel.Should().Be(400);
        sut.WindowWidth.Should().Be(800);
    }
}
