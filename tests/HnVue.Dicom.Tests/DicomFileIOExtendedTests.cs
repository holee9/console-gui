using System.IO;
using FellowOakDicom;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Extended coverage tests for <see cref="DicomFileIO"/> and <see cref="DicomFileWrapper"/>.
/// Targets success paths with valid DICOM data and additional error edge cases
/// to push ReadAsync, WriteAsync, GetTagValueAsync coverage toward 90%+.
/// </summary>
[Trait("SWR", "SWR-DICOM-020")]
public sealed class DicomFileIOExtendedTests
{
    /// <summary>
    /// Creates a valid fo-dicom DicomFile with minimum required tags
    /// and optional additional tags via the configure callback.
    /// </summary>
    private static DicomFile CreateValidDicomFile(
        Action<DicomDataset>? configure = null)
    {
        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, DicomUID.SecondaryCaptureImageStorage },
            { DicomTag.SOPInstanceUID, DicomUID.Generate() },
        };

        configure?.Invoke(dataset);
        return new DicomFile(dataset);
    }

    /// <summary>
    /// Writes a valid DICOM file to a temp path and returns the path.
    /// Caller is responsible for deleting the file.
    /// </summary>
    private static async Task<string> WriteValidDicomToTempAsync(
        Action<DicomDataset>? configure = null)
    {
        var dicomFile = CreateValidDicomFile(configure);
        var tempPath = Path.Combine(
            Path.GetTempPath(),
            $"dicom_io_test_{Guid.NewGuid():N}.dcm");
        await dicomFile.SaveAsync(tempPath);
        return tempPath;
    }

    // ── ReadAsync Success Path ────────────────────────────────────────────────

    [Fact]
    public async Task ReadAsync_ValidDicomFile_ReturnsSuccessWithWrapper()
    {
        var tempFile = await WriteValidDicomToTempAsync();
        try
        {
            var result = await DicomFileIO.ReadAsync(tempFile);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.DicomFile.Should().NotBeNull();
            result.Value.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_ValidDicomFile_WithPatientTags_ReturnsCorrectValues()
    {
        var tempFile = await WriteValidDicomToTempAsync(ds =>
        {
            ds.Add(DicomTag.PatientID, "PAT12345");
            ds.Add(DicomTag.PatientName, "Doe^John");
            ds.Add(DicomTag.StudyInstanceUID, "1.2.3.4.5.6.7");
        });
        try
        {
            var result = await DicomFileIO.ReadAsync(tempFile);

            result.IsSuccess.Should().BeTrue();
            result.Value.PatientName.Should().Be("Doe^John");
            result.Value.StudyInstanceUid.Should().Be("1.2.3.4.5.6.7");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ReadAsync_BinaryGarbageFile_ReturnsImageProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var garbage = new byte[] { 0x00, 0x01, 0x02, 0xFF, 0xFE, 0xFD, 0xAA, 0xBB };
            await File.WriteAllBytesAsync(tempFile, garbage);

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
    public async Task ReadAsync_PartialDicomHeader_ReturnsImageProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            // Write the DICM magic bytes followed by truncated data
            var partial = new byte[] { 0x44, 0x49, 0x43, 0x4D, 0x00, 0x01 }; // "DICM\0\1"
            await File.WriteAllBytesAsync(tempFile, partial);

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
    public async Task ReadAsync_NullFilePath_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.ReadAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── WriteAsync Success Path ───────────────────────────────────────────────

    [Fact]
    public async Task WriteAsync_ValidWrapper_WritesFileSuccessfully()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"dicom_write_test_{Guid.NewGuid():N}.dcm");

        try
        {
            var result = await DicomFileIO.WriteAsync(wrapper, outputPath);

            result.IsSuccess.Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();

            // Verify the written file can be read back
            var readBack = await FellowOakDicom.DicomFile.OpenAsync(outputPath);
            readBack.Should().NotBeNull();
            readBack.Dataset.Should().NotBeNull();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task WriteAsync_ValidWrapper_CreatesSubdirectoryAndWritesFile()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);
        var subDir = Path.Combine(
            Path.GetTempPath(),
            $"dicom_subdir_test_{Guid.NewGuid():N}");
        var outputPath = Path.Combine(subDir, "nested", "output.dcm");

        try
        {
            var result = await DicomFileIO.WriteAsync(wrapper, outputPath);

            result.IsSuccess.Should().BeTrue();
            File.Exists(outputPath).Should().BeTrue();
            Directory.Exists(Path.Combine(subDir, "nested")).Should().BeTrue();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
            if (Directory.Exists(subDir)) Directory.Delete(subDir, recursive: true);
        }
    }

    [Fact]
    public async Task WriteAsync_NullWrapper_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.WriteAsync(null!, "/tmp/test.dcm");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteAsync_NullOutputPath_ThrowsArgumentNullException()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);

        var act = async () => await DicomFileIO.WriteAsync(wrapper, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteAsync_ReadAsync_Roundtrip_PreservesData()
    {
        var dicomFile = CreateValidDicomFile(ds =>
        {
            ds.Add(DicomTag.PatientID, "RT001");
            ds.Add(DicomTag.PatientName, "Roundtrip^Test");
        });
        var wrapper = new DicomFileWrapper(dicomFile);
        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"dicom_roundtrip_{Guid.NewGuid():N}.dcm");

        try
        {
            var writeResult = await DicomFileIO.WriteAsync(wrapper, outputPath);
            writeResult.IsSuccess.Should().BeTrue();

            var readResult = await DicomFileIO.ReadAsync(outputPath);
            readResult.IsSuccess.Should().BeTrue();
            readResult.Value.PatientName.Should().Be("Roundtrip^Test");
            readResult.Value.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    // ── GetTagValueAsync Success and Error Paths ──────────────────────────────

    [Fact]
    public async Task GetTagValueAsync_ValidDicomFile_KnownTag_ReturnsValue()
    {
        var tempFile = await WriteValidDicomToTempAsync(ds =>
        {
            ds.Add(DicomTag.PatientName, "Tag^Test");
        });
        try
        {
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "PatientName");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("Tag^Test");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetTagValueAsync_ValidDicomFile_PatientIdTag_ReturnsValue()
    {
        var tempFile = await WriteValidDicomToTempAsync(ds =>
        {
            ds.Add(DicomTag.PatientID, "PID-999");
        });
        try
        {
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "PatientID");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be("PID-999");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetTagValueAsync_ValidDicomFile_UnknownTag_ReturnsNull()
    {
        var tempFile = await WriteValidDicomToTempAsync();
        try
        {
            // Query for a tag not present in the dataset
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "InstitutionName");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetTagValueAsync_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.GetTagValueAsync(
            "C:/nonexistent_abcd_xyz/test.dcm",
            "PatientName");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task GetTagValueAsync_InvalidDicomFile_ReturnsImageProcessingFailed()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFile, "garbage data not dicom");

            var result = await DicomFileIO.GetTagValueAsync(tempFile, "PatientName");

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.ImageProcessingFailed);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetTagValueAsync_InvalidTagKeyword_ReturnsFailure()
    {
        var tempFile = await WriteValidDicomToTempAsync();
        try
        {
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "NotAValidDicomTag_At_All");

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(ErrorCode.Unknown);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task GetTagValueAsync_EmptyTagValue_ReturnsNull()
    {
        var tempFile = await WriteValidDicomToTempAsync(ds =>
        {
            ds.Add(DicomTag.InstitutionName, string.Empty);
        });
        try
        {
            var result = await DicomFileIO.GetTagValueAsync(tempFile, "InstitutionName");

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeNull();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    // ── DicomFileWrapper Extended Coverage ─────────────────────────────────────

    [Fact]
    public void DicomFileWrapper_AllProperties_WithFullDataset_ReturnCorrectValues()
    {
        var dicomFile = CreateValidDicomFile(ds =>
        {
            ds.Add(DicomTag.StudyInstanceUID, "1.2.3.4.5.6.7.8");
            ds.Add(DicomTag.PatientName, "Wrapper^Test");
        });
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        wrapper.StudyInstanceUid.Should().Be("1.2.3.4.5.6.7.8");
        wrapper.PatientName.Should().Be("Wrapper^Test");
        wrapper.DicomFile.Should().BeSameAs(dicomFile);
    }

    [Fact]
    public void DicomFileWrapper_NoOptionalTags_AllOptionalPropertiesReturnNull()
    {
        var dicomFile = CreateValidDicomFile(); // Only SOPClassUID + SOPInstanceUID
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.SopInstanceUid.Should().NotBeNullOrWhiteSpace();
        wrapper.StudyInstanceUid.Should().BeNull();
        wrapper.PatientName.Should().BeNull();
    }

    [Fact]
    public void DicomFileWrapper_NullDicomFile_ThrowsArgumentNullException()
    {
        var act = () => new DicomFileWrapper(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DicomFileWrapper_DicomFileProperty_ReturnsUnderlyingFile()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);

        wrapper.DicomFile.Should().BeSameAs(dicomFile);
        wrapper.DicomFile.Dataset.Should().BeSameAs(dicomFile.Dataset);
    }

    [Fact]
    public void DicomFileWrapper_SopInstanceUid_IsConsistentAcrossAccesses()
    {
        var dicomFile = CreateValidDicomFile();
        var wrapper = new DicomFileWrapper(dicomFile);

        var first = wrapper.SopInstanceUid;
        var second = wrapper.SopInstanceUid;

        first.Should().Be(second);
    }
}
