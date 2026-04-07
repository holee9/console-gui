namespace HnVue.Common.Models;

/// <summary>
/// Carries raw pixel data delivered by the flat-panel detector after readout.
/// Pixel data is 16-bit unsigned little-endian (2 bytes per pixel) for DR detectors.
/// Consumed by <c>IImageProcessor.ProcessAsync</c> for further processing.
/// </summary>
/// <param name="Width">Image width in pixels (e.g., 2560 for a 43 cm × 43 cm panel at 169 µm pitch).</param>
/// <param name="Height">Image height in pixels.</param>
/// <param name="BitsPerPixel">Bit depth per pixel (12 or 14 for typical CsI/a-Si DR detectors).</param>
/// <param name="PixelData">
/// Raw pixel byte buffer in row-major order.
/// For 16-bit pixels: 2 × Width × Height bytes, little-endian.
/// </param>
/// <param name="SerialNumber">Detector panel serial number. <see langword="null"/> if not reported.</param>
/// <param name="TemperatureCelsius">Panel temperature at time of acquisition. 0.0 if not reported.</param>
/// <param name="Timestamp">UTC timestamp when the image was acquired. <see langword="null"/> uses UtcNow at creation.</param>
public sealed record RawDetectorImage(
    int Width,
    int Height,
    int BitsPerPixel,
    byte[] PixelData,
    string? SerialNumber = null,
    double TemperatureCelsius = 0.0,
    DateTimeOffset? Timestamp = null);
