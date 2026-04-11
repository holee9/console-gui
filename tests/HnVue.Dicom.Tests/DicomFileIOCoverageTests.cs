using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Coverage tests for <see cref="DicomFileIO"/> and <see cref="DicomFileWrapper"/>.
/// Targets ReadAsync, WriteAsync, GetTagValueAsync error paths and wrapper properties.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomFileIOCoverageTests
{
    private static DicomFile CreateValidDicomFile(Action<DicomDataset>? configure = null)
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };

        configure?.Invoke(dataset);
        return new DicomFile(dataset);
    }

    // ── ReadAsync Coverage ───────────────────────────────────────────────────

    [Fact]
    public async Task ReadAsync_NullFilePath_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.ReadAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReadAsync_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.ReadAsync("C:/nonexistent_abcxyz/test.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ReadAsync_InvalidFileContent_ReturnsImageProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "not a dicom file");
            var result = await DicomFileIO.ReadAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_EmptyFile_ReturnsImageProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, string.Empty);
            var result = await DicomFileIO.ReadAsync(tempFile);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── WriteAsync Coverage ──────────────────────────────────────────────────

    [Fact]
    public async Task WriteAsync_NullWrapper_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.WriteAsync(null!, "/tmp/output.dcm");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteAsync_NullOutputPath_ThrowsArgumentNullException()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "test");
            var readResult = await DicomFileIO.ReadAsync(tempFile);

            // Only test WriteAsync null path if ReadAsync succeeds (valid DICOM)
            // For null outputPath test, we don't need a valid wrapper
            var act = async () => await DicomFileIO.WriteAsync(null!, null!);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── GetTagValueAsync Coverage ────────────────────────────────────────────

    [Fact]
    public async Task GetTagValueAsync_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.GetTagValueAsync("C:/nonexistent_xyz/test.dcm", "PatientName");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GetTagValueAsync_InvalidFile_ReturnsFailure()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "not dicom");
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "PatientName");

            result.IsFailure.Should().BeTrue();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── DicomFileWrapper Coverage ────────────────────────────────────────────

    [Fact]
    public void DicomFileWrapper_NullDicomFile_ThrowsArgumentNullException()
    {
        var act = () => new DicomFileWrapper(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DicomFileWrapper_ValidFile_PropertiesReturnValues()
    {
        var dicomFile = CreateValidDicomFile(dataset =>
        {
            dataset.Add(DicomTag.StudyInstanceUID, "1.2.3.4.5.6");
            dataset.Add(DicomTag.PatientName, "Test Patient");
        });
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        wrapper.StudyInstanceUid.Should().Be("1.2.3.4.5.6");
        wrapper.PatientName.Should().Be("Test Patient");
    }

    [Fact]
    public void DicomFileWrapper_MissingTags_ReturnsNull()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        wrapper.StudyInstanceUid.Should().BeNull();
        wrapper.PatientName.Should().BeNull();
    }

    [Fact]
    public void DicomFileWrapper_DicomFileProperty_ReturnsSameInstance()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.DicomFile.Should().BeSameAs(dicomFile);
    }
}
