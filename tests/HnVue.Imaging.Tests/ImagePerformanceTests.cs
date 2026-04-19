using System.Diagnostics;
using FluentAssertions;
using HnVue.Common.Models;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Performance threshold tests for large image processing.
/// Verifies that critical operations complete within acceptable time bounds.
/// </summary>
[Trait("SWR", "SWR-IP-020")]
public sealed class ImagePerformanceTests
{
    private static ImageProcessor CreateSut() => new();

    [Fact]
    public void Zoom_Upscale_2048x2048_CompletesWithin5s()
    {
        var sut = CreateSut();
        var pixels = new byte[2048 * 2048];
        var image = new ProcessedImage(2048, 2048, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.Zoom(image, 2.0);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(4096);
        result.Value.Height.Should().Be(4096);
        sw.ElapsedMilliseconds.Should().BeLessThan(5000);
    }

    [Fact]
    public void Zoom_Downscale_2048x2048_CompletesWithin1000ms()
    {
        var sut = CreateSut();
        var pixels = new byte[2048 * 2048];
        var image = new ProcessedImage(2048, 2048, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.Zoom(image, 0.5);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        result.Value.Width.Should().Be(1024);
        result.Value.Height.Should().Be(1024);
        sw.ElapsedMilliseconds.Should().BeLessThan(1000);
    }

    [Fact]
    public void ApplyWindowLevel_2048x2048_CompletesWithin200ms()
    {
        var sut = CreateSut();
        var pixels = new byte[2048 * 2048];
        var image = new ProcessedImage(2048, 2048, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.ApplyWindowLevel(image, 128, 256);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public void Rotate_90_1024x1024_CompletesWithin200ms()
    {
        var sut = CreateSut();
        var pixels = new byte[1024 * 1024];
        var image = new ProcessedImage(1024, 1024, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.Rotate(image, 90);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public void ApplyBrightnessOffset_2048x2048_CompletesWithin200ms()
    {
        var sut = CreateSut();
        var pixels = new byte[2048 * 2048];
        var image = new ProcessedImage(2048, 2048, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.ApplyBrightnessOffset(image, 50);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public void Flip_1024x1024_CompletesWithin200ms()
    {
        var sut = CreateSut();
        var pixels = new byte[1024 * 1024];
        var image = new ProcessedImage(1024, 1024, 8, pixels, 128, 256);

        var sw = Stopwatch.StartNew();
        var result = sut.Flip(image, true);
        sw.Stop();

        result.IsSuccess.Should().BeTrue();
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }
}
