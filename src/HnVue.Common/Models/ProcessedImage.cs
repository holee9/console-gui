namespace HnVue.Common.Models;

/// <summary>
/// Represents a processed radiographic image held in memory.
/// Used by <c>IImageProcessor</c> for rendering and manipulation operations.
/// </summary>
public sealed class ProcessedImage
{
    /// <summary>Initialises a new <see cref="ProcessedImage"/> with mandatory properties.</summary>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="bitsPerPixel">Bit depth of each pixel (e.g., 12 or 16 for DR detectors).</param>
    /// <param name="pixelData">Raw pixel buffer in row-major order.</param>
    /// <param name="windowCenter">DICOM window centre used for display mapping.</param>
    /// <param name="windowWidth">DICOM window width used for display mapping.</param>
    /// <param name="filePath">Optional file system path of the source image file.</param>
    /// <param name="panOffsetX">Cumulative horizontal pan offset in pixels. SWR-IP-026.</param>
    /// <param name="panOffsetY">Cumulative vertical pan offset in pixels. SWR-IP-026.</param>
    public ProcessedImage(
        int width,
        int height,
        int bitsPerPixel,
        byte[] pixelData,
        double windowCenter,
        double windowWidth,
        string? filePath = null,
        int panOffsetX = 0,
        int panOffsetY = 0)
    {
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
        PixelData = pixelData;
        WindowCenter = windowCenter;
        WindowWidth = windowWidth;
        FilePath = filePath;
        PanOffsetX = panOffsetX;
        PanOffsetY = panOffsetY;
    }

    /// <summary>Gets the image width in pixels.</summary>
    public int Width { get; }

    /// <summary>Gets the image height in pixels.</summary>
    public int Height { get; }

    /// <summary>Gets the bit depth per pixel (typically 12 or 16 for digital radiography).</summary>
    public int BitsPerPixel { get; }

    /// <summary>Gets the raw pixel buffer in row-major order.</summary>
    public byte[] PixelData { get; }

    /// <summary>Gets the DICOM window centre value used for display grey-scale mapping.</summary>
    public double WindowCenter { get; }

    /// <summary>Gets the DICOM window width value used for display grey-scale mapping.</summary>
    public double WindowWidth { get; }

    /// <summary>Gets the optional file system path of the source image file.</summary>
    public string? FilePath { get; }

    /// <summary>
    /// Gets the cumulative horizontal pan offset in pixels from the image origin.
    /// SWR-IP-026 / Issue #1.
    /// </summary>
    public int PanOffsetX { get; }

    /// <summary>
    /// Gets the cumulative vertical pan offset in pixels from the image origin.
    /// SWR-IP-026 / Issue #1.
    /// </summary>
    public int PanOffsetY { get; }

    /// <summary>
    /// Gets the 16-bit raw pixel buffer preserved from DICOM input, or <see langword="null"/>
    /// when the source was 8-bit or the 16-bit data was not retained.
    /// Required for ROI statistics (SWR-IP-036, 0–65535 range) and brightness offset operations
    /// (SWR-IP-052). SWR-DC-055 / Issue #8.
    /// </summary>
    public ushort[]? RawPixelData16 { get; init; }
}
