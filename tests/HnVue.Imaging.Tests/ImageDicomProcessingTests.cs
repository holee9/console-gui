using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// DICOM-specific processing tests: 8-bit/16-bit normalization, VOI LUT tags, pixel extraction.
/// </summary>
[Trait("SWR", "SWR-IP-020")]
public sealed class ImageDicomProcessingTests
{
    private static ImageProcessor CreateSut() => new();

    [Fact]
    public async Task ProcessAsync_Dicom8Bit_NoPixelData_ReturnsEmptyPixelArray()
    {
        var tempPath = await CreateDicomWithoutPixelDataAsync(4, 4);

        try
        {
            var result = await CreateSut().ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.PixelData.Should().BeEmpty();
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_Dicom16Bit_Gradient_NormalizesToFull8BitRange()
    {
        var tempPath = await CreateDicom16BitGradientAsync(8, 8);

        try
        {
            var result = await CreateSut().ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            // 16-bit gradient 0..63 → min=0, max=63 → normalized to 0..255
            result.Value.PixelData[0].Should().Be(0);
            result.Value.PixelData[^1].Should().Be(255);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_DicomWithExplicitWindow_OverridesAutoWindow()
    {
        var tempPath = await CreateDicom8BitAsync(4, 4, windowCenter: 2048, windowWidth: 4096);

        try
        {
            // Explicit parameters override both auto-window and DICOM tags
            var result = await CreateSut().ProcessAsync(tempPath,
                new ProcessingParameters(WindowCenter: 100.0, WindowWidth: 200.0));

            result.IsSuccess.Should().BeTrue();
            result.Value.WindowCenter.Should().Be(100.0);
            result.Value.WindowWidth.Should().Be(200.0);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_NonDicomFile_FallsBackToRawProcessing()
    {
        // Create a file that is NOT a valid DICOM
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_notdicom_{Guid.NewGuid():N}.bin");
        File.WriteAllBytes(tempPath, new byte[64]);

        try
        {
            var result = await CreateSut().ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.FilePath.Should().Be(tempPath);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_Dicom16Bit_PreservesOriginalDimensions()
    {
        var tempPath = await CreateDicom16BitGradientAsync(6, 8);

        try
        {
            var result = await CreateSut().ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.Width.Should().Be(8);
            result.Value.Height.Should().Be(6);
        }
        finally { File.Delete(tempPath); }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static async Task<string> CreateDicomWithoutPixelDataAsync(ushort rows, ushort columns)
    {
        var dataset = new DicomDataset();
        dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.Add(DicomTag.Modality, "DX");
        dataset.Add(DicomTag.Rows, rows);
        dataset.Add(DicomTag.Columns, columns);
        dataset.Add(DicomTag.BitsAllocated, (ushort)8);
        dataset.Add(DicomTag.BitsStored, (ushort)8);
        dataset.Add(DicomTag.HighBit, (ushort)7);
        dataset.Add(DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");
        // No PixelData tag

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_nopix_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }

    private static async Task<string> CreateDicom16BitGradientAsync(ushort rows, ushort columns)
    {
        var dataset = new DicomDataset();
        dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.Add(DicomTag.Modality, "DX");
        dataset.Add(DicomTag.Rows, rows);
        dataset.Add(DicomTag.Columns, columns);
        dataset.Add(DicomTag.BitsAllocated, (ushort)16);
        dataset.Add(DicomTag.BitsStored, (ushort)12);
        dataset.Add(DicomTag.HighBit, (ushort)11);
        dataset.Add(DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");

        var pixelCount = rows * columns;
        var pixels = new ushort[pixelCount];
        for (int i = 0; i < pixelCount; i++)
            pixels[i] = (ushort)(i % 64); // Small range gradient
        dataset.Add(new DicomOtherWord(DicomTag.PixelData, pixels));

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_16grad_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }

    private static async Task<string> CreateDicom8BitAsync(ushort rows, ushort columns,
        double? windowCenter = null, double? windowWidth = null)
    {
        var dataset = new DicomDataset();
        dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.Add(DicomTag.Modality, "DX");
        dataset.Add(DicomTag.Rows, rows);
        dataset.Add(DicomTag.Columns, columns);
        dataset.Add(DicomTag.BitsAllocated, (ushort)8);
        dataset.Add(DicomTag.BitsStored, (ushort)8);
        dataset.Add(DicomTag.HighBit, (ushort)7);
        dataset.Add(DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");

        if (windowCenter.HasValue) dataset.Add(DicomTag.WindowCenter, windowCenter.Value);
        if (windowWidth.HasValue) dataset.Add(DicomTag.WindowWidth, windowWidth.Value);

        var pixels = new byte[rows * columns];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = (byte)(i % 256);
        dataset.Add(new DicomOtherByte(DicomTag.PixelData, pixels));

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_8bit_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }
}
