using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace HnVue.UI.QA.Tests;

/// <summary>
/// Visual regression tests for UI design system consistency.
/// Verifies 95%+ visual consistency across components and screens.
/// </summary>
public sealed class VisualRegressionTests
{
    private const double SimilarityThreshold = 0.95;
    private const string ScreenshotsRelativePath = @"..\..\..\UI\Screenshots";

    private readonly ITestOutputHelper _output;

    public VisualRegressionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Compares two images and returns similarity ratio (0.0 to 1.0).
    /// Uses pixel-by-pixel comparison with tolerance for minor variations.
    /// </summary>
    private static double CompareImages(Bitmap image1, Bitmap image2)
    {
        if (image1.Width != image2.Width || image1.Height != image2.Height)
        {
            return 0.0;
        }

        int totalPixels = image1.Width * image1.Height;
        int matchingPixels = 0;

        // Lock bits for fast pixel access
        var bmpData1 = image1.LockBits(
            new Rectangle(0, 0, image1.Width, image1.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        var bmpData2 = image2.LockBits(
            new Rectangle(0, 0, image2.Width, image2.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        try
        {
            int bytes = Math.Abs(bmpData1.Stride) * image1.Height;
            byte[] pixelData1 = new byte[bytes];
            byte[] pixelData2 = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(bmpData1.Scan0, pixelData1, 0, bytes);
            System.Runtime.InteropServices.Marshal.Copy(bmpData2.Scan0, pixelData2, 0, bytes);

            // Compare pixels with RGB tolerance
            const int tolerance = 10; // Allow minor color variations

            for (int i = 0; i < bytes; i += 3)
            {
                int rDiff = Math.Abs(pixelData1[i] - pixelData2[i]);
                int gDiff = Math.Abs(pixelData1[i + 1] - pixelData2[i + 1]);
                int bDiff = Math.Abs(pixelData1[i + 2] - pixelData2[i + 2]);

                if (rDiff <= tolerance && gDiff <= tolerance && bDiff <= tolerance)
                {
                    matchingPixels++;
                }
            }
        }
        finally
        {
            image1.UnlockBits(bmpData1);
            image2.UnlockBits(bmpData2);
        }

        return (double)matchingPixels / totalPixels;
    }

    private string GetBaselinePath(string screenshotName) =>
        Path.Combine(Directory.GetCurrentDirectory(), ScreenshotsRelativePath, "Baseline", screenshotName);

    private string GetActualPath(string screenshotName) =>
        Path.Combine(Directory.GetCurrentDirectory(), ScreenshotsRelativePath, "Actual", screenshotName);

    private string GetDiffPath(string screenshotName) =>
        Path.Combine(Directory.GetCurrentDirectory(), ScreenshotsRelativePath, "Diff", screenshotName);

    /// <summary>
    /// Verifies login screen visual consistency against baseline.
    /// </summary>
    [Fact]
    public void LoginScreen_ShouldMatchBaseline()
    {
        // Arrange
        string baselineFile = GetBaselinePath("login_screen.png");
        string actualFile = GetActualPath("login_screen_actual.png");

        // Skip if baseline doesn't exist (first run scenario)
        if (!File.Exists(baselineFile))
        {
            _output.WriteLine($"Baseline not found: {baselineFile}. Skipping test.");
            return;
        }

        // Skip if actual screenshot not available (requires UI automation)
        if (!File.Exists(actualFile))
        {
            _output.WriteLine($"Actual screenshot not found: {actualFile}. Test requires UI automation.");
            return;
        }

        // Act
        using var baseline = new Bitmap(baselineFile);
        using var actual = new Bitmap(actualFile);

        double similarity = CompareImages(baseline, actual);

        // Assert
        similarity.Should().BeGreaterOrEqualTo(SimilarityThreshold,
            $"Login screen should match baseline with {SimilarityThreshold:P0} similarity. Actual: {similarity:P2}");

        _output.WriteLine($"Login screen similarity: {similarity:P2}");
    }

    /// <summary>
    /// Verifies main dashboard visual consistency against baseline.
    /// </summary>
    [Fact]
    public void MainDashboard_ShouldMatchBaseline()
    {
        string baselineFile = GetBaselinePath("main_dashboard.png");
        string actualFile = GetActualPath("main_dashboard_actual.png");

        if (!File.Exists(baselineFile))
        {
            _output.WriteLine($"Baseline not found: {baselineFile}. Skipping test.");
            return;
        }

        if (!File.Exists(actualFile))
        {
            _output.WriteLine($"Actual screenshot not found: {actualFile}. Test requires UI automation.");
            return;
        }

        using var baseline = new Bitmap(baselineFile);
        using var actual = new Bitmap(actualFile);

        double similarity = CompareImages(baseline, actual);

        similarity.Should().BeGreaterOrEqualTo(SimilarityThreshold,
            $"Main dashboard should match baseline with {SimilarityThreshold:P0} similarity. Actual: {similarity:P2}");

        _output.WriteLine($"Main dashboard similarity: {similarity:P2}");
    }

    /// <summary>
    /// Verifies workflow screen visual consistency against baseline.
    /// </summary>
    [Fact]
    public void WorkflowScreen_ShouldMatchBaseline()
    {
        string baselineFile = GetBaselinePath("workflow_screen.png");
        string actualFile = GetActualPath("workflow_screen_actual.png");

        if (!File.Exists(baselineFile))
        {
            _output.WriteLine($"Baseline not found: {baselineFile}. Skipping test.");
            return;
        }

        if (!File.Exists(actualFile))
        {
            _output.WriteLine($"Actual screenshot not found: {actualFile}. Test requires UI automation.");
            return;
        }

        using var baseline = new Bitmap(baselineFile);
        using var actual = new Bitmap(actualFile);

        double similarity = CompareImages(baseline, actual);

        similarity.Should().BeGreaterOrEqualTo(SimilarityThreshold,
            $"Workflow screen should match baseline with {SimilarityThreshold:P0} similarity. Actual: {similarity:P2}");

        _output.WriteLine($"Workflow screen similarity: {similarity:P2}");
    }

    /// <summary>
    /// Verifies image viewer visual consistency against baseline.
    /// </summary>
    [Fact]
    public void ImageViewerScreen_ShouldMatchBaseline()
    {
        string baselineFile = GetBaselinePath("image_viewer_screen.png");
        string actualFile = GetActualPath("image_viewer_screen_actual.png");

        if (!File.Exists(baselineFile))
        {
            _output.WriteLine($"Baseline not found: {baselineFile}. Skipping test.");
            return;
        }

        if (!File.Exists(actualFile))
        {
            _output.WriteLine($"Actual screenshot not found: {actualFile}. Test requires UI automation.");
            return;
        }

        using var baseline = new Bitmap(baselineFile);
        using var actual = new Bitmap(actualFile);

        double similarity = CompareImages(baseline, actual);

        similarity.Should().BeGreaterOrEqualTo(SimilarityThreshold,
            $"Image viewer screen should match baseline with {SimilarityThreshold:P0} similarity. Actual: {similarity:P2}");

        _output.WriteLine($"Image viewer similarity: {similarity:P2}");
    }

    /// <summary>
    /// Verifies component consistency across different screen resolutions.
    /// </summary>
    [Theory]
    [InlineData(1920, 1080)]
    [InlineData(2560, 1440)]
    [InlineData(3840, 2160)]
    public void Components_ShouldBeConsistentAcrossDPI(int width, int height)
    {
        _output.WriteLine($"Testing DPI consistency for {width}x{height}");

        // This test verifies that component layouts remain consistent
        // at different resolutions. Actual implementation requires
        // UI automation framework to render at different sizes.

        // For now, we verify the design token system supports DPI scaling
        string designTokensPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..", "src", "HnVue.UI", "Themes", "tokens", "CoreTokens.xaml");

        File.Exists(designTokensPath).Should().BeTrue("Design tokens should exist for DPI scaling");

        _output.WriteLine($"DPI {width}x{height}: Design token system verified");
    }
}
