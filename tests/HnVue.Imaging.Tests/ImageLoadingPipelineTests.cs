using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Imaging;
using Xunit;

namespace HnVue.Imaging.Tests;

/// <summary>
/// Tests for image loading/rendering pipeline: raw file loading, DICOM parsing, auto-window computation.
/// </summary>
[Trait("SWR", "SWR-IP-020")]
public sealed class ImageLoadingPipelineTests
{
    private static ImageProcessor CreateSut() => new();

    [Fact]
    public async Task ProcessAsync_RawFile_WithExplicitDimensions_ReturnsCorrectSize()
    {
        var sut = CreateSut();
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_raw_{Guid.NewGuid():N}.raw");
        var pixels = new byte[6 * 4]; // 6 wide, 4 tall
        for (int i = 0; i < pixels.Length; i++) pixels[i] = (byte)(i % 256);
        File.WriteAllBytes(tempPath, pixels);

        try
        {
            var parameters = new ProcessingParameters(
                WindowCenter: null, WindowWidth: null, AutoWindow: false,
                RawImageWidth: 6, RawImageHeight: 4);

            var result = await sut.ProcessAsync(tempPath, parameters);

            result.IsSuccess.Should().BeTrue();
            result.Value.Width.Should().Be(6);
            result.Value.Height.Should().Be(4);
            result.Value.PixelData.Should().HaveCount(24);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_RawFile_NoExplicitDimensions_UsesSquareEstimate()
    {
        var sut = CreateSut();
        // 100 bytes → sqrt(100) = 10 → 10×10
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_sq_{Guid.NewGuid():N}.raw");
        File.WriteAllBytes(tempPath, new byte[100]);

        try
        {
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters());

            result.IsSuccess.Should().BeTrue();
            result.Value.Width.Should().Be(10);
            result.Value.Height.Should().Be(10);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_DicomFile_16bit_PreservesRawPixelData16()
    {
        var sut = CreateSut();
        var tempPath = await CreateDicom16BitAsync(rows: 4, columns: 4);

        try
        {
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters(AutoWindow: true));

            result.IsSuccess.Should().BeTrue();
            result.Value.RawPixelData16.Should().NotBeNull();
            result.Value.RawPixelData16.Should().HaveCount(16);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_DicomFile_WithoutVoiLutTags_AutoWindowUsesStatistics()
    {
        var sut = CreateSut();
        var tempPath = await CreateDicom8BitAsync(rows: 8, columns: 8);

        try
        {
            var result = await sut.ProcessAsync(tempPath, new ProcessingParameters(AutoWindow: true));

            result.IsSuccess.Should().BeTrue();
            result.Value.WindowCenter.Should().BeGreaterThan(0);
            result.Value.WindowWidth.Should().BeGreaterThan(0);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_DicomFile_InvalidRowsColumns_ReturnsFailure()
    {
        var dataset = new DicomDataset();
        dataset.Add(DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage);
        dataset.Add(DicomTag.SOPInstanceUID, DicomUID.Generate());
        dataset.Add(DicomTag.Modality, "DX");
        dataset.Add(DicomTag.Rows, (ushort)0); // Invalid
        dataset.Add(DicomTag.Columns, (ushort)0);
        dataset.Add(DicomTag.BitsAllocated, (ushort)8);
        dataset.Add(DicomTag.BitsStored, (ushort)8);
        dataset.Add(DicomTag.HighBit, (ushort)7);
        dataset.Add(DicomTag.PixelRepresentation, (ushort)0);
        dataset.Add(DicomTag.SamplesPerPixel, (ushort)1);
        dataset.Add(DicomTag.PhotometricInterpretation, "MONOCHROME2");
        dataset.Add(new DicomOtherByte(DicomTag.PixelData, new byte[0]));

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_inv_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);

        try
        {
            var result = await sut_ProcessAsync(tempPath);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
        }
        finally { File.Delete(tempPath); }
    }

    [Fact]
    public async Task ProcessAsync_CancelledBeforeIO_ThrowsOperationCanceledException()
    {
        var sut = CreateSut();
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_cancel_{Guid.NewGuid():N}.raw");
        File.WriteAllBytes(tempPath, new byte[16]);

        try
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            var act = async () => await sut.ProcessAsync(tempPath, new ProcessingParameters(), cts.Token);

            await act.Should().ThrowAsync<OperationCanceledException>();
        }
        finally { File.Delete(tempPath); }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static async Task<Result<ProcessedImage>> sut_ProcessAsync(string path)
        => await new ImageProcessor().ProcessAsync(path, new ProcessingParameters());

    private static async Task<string> CreateDicom8BitAsync(ushort rows, ushort columns)
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

        var pixels = new byte[rows * columns];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = (byte)(i % 256);
        dataset.Add(new DicomOtherByte(DicomTag.PixelData, pixels));

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_8bit_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }

    private static async Task<string> CreateDicom16BitAsync(ushort rows, ushort columns)
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

        var pixels = new ushort[rows * columns];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = (ushort)(i % 4096);
        dataset.Add(new DicomOtherWord(DicomTag.PixelData, pixels));

        var dcmFile = new DicomFile(dataset);
        var tempPath = Path.Combine(Path.GetTempPath(), $"hnvue_16bit_{Guid.NewGuid():N}.dcm");
        await dcmFile.SaveAsync(tempPath);
        return tempPath;
    }
}
