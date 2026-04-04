using System.IO;
using FluentAssertions;
using HnVue.Common.Results;
using HnVue.Dicom;
using Xunit;

namespace HnVue.Dicom.Tests;

[Trait("SWR", "SWR-DICOM-010")]
public sealed class DicomFileIOTests : IDisposable
{
    private readonly string _tempDir;

    public DicomFileIOTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"DicomTest_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true);
    }

    private string CreateTestDicomFile(string filename = "test.dcm")
    {
        // Create a minimal valid DICOM file using fo-dicom
        var dataset = new FellowOakDicom.DicomDataset();
        dataset.Add(FellowOakDicom.DicomTag.SOPClassUID, FellowOakDicom.DicomUID.DigitalXRayImageStorageForPresentation);
        dataset.Add(FellowOakDicom.DicomTag.SOPInstanceUID, FellowOakDicom.DicomUID.Generate());
        dataset.Add(FellowOakDicom.DicomTag.StudyInstanceUID, FellowOakDicom.DicomUID.Generate());
        dataset.Add(FellowOakDicom.DicomTag.PatientName, "Test^Patient");
        dataset.Add(FellowOakDicom.DicomTag.PatientID, "P001");

        var dcmFile = new FellowOakDicom.DicomFile(dataset);
        var path = Path.Combine(_tempDir, filename);
        dcmFile.Save(path);
        return path;
    }

    // ── ReadAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Read_ValidDicomFile_ReturnsWrapper()
    {
        var path = CreateTestDicomFile();

        var result = await DicomFileIO.ReadAsync(path);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Read_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.ReadAsync("C:/nonexistent/test.dcm");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task Read_NullPath_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.ReadAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Read_ValidFile_WrapperHasCorrectPatientName()
    {
        var path = CreateTestDicomFile();

        var result = await DicomFileIO.ReadAsync(path);

        result.Value.PatientName.Should().Be("Test^Patient");
    }

    [Fact]
    public async Task Read_ValidFile_WrapperHasStudyInstanceUid()
    {
        var path = CreateTestDicomFile();

        var result = await DicomFileIO.ReadAsync(path);

        result.Value.StudyInstanceUid.Should().NotBeNullOrEmpty();
    }

    // ── WriteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task Write_ValidWrapper_CreatesFile()
    {
        var sourcePath = CreateTestDicomFile();
        var readResult = await DicomFileIO.ReadAsync(sourcePath);
        var outputPath = Path.Combine(_tempDir, "output.dcm");

        var writeResult = await DicomFileIO.WriteAsync(readResult.Value, outputPath);

        writeResult.IsSuccess.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public async Task Write_CreatesDirectoryIfNotExists()
    {
        var sourcePath = CreateTestDicomFile();
        var readResult = await DicomFileIO.ReadAsync(sourcePath);
        var outputPath = Path.Combine(_tempDir, "subdir", "output.dcm");

        var writeResult = await DicomFileIO.WriteAsync(readResult.Value, outputPath);

        writeResult.IsSuccess.Should().BeTrue();
        File.Exists(outputPath).Should().BeTrue();
    }

    [Fact]
    public async Task Write_NullWrapper_ThrowsArgumentNullException()
    {
        var act = async () => await DicomFileIO.WriteAsync(null!, "output.dcm");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Write_NullPath_ThrowsArgumentNullException()
    {
        var sourcePath = CreateTestDicomFile();
        var readResult = await DicomFileIO.ReadAsync(sourcePath);

        var act = async () => await DicomFileIO.WriteAsync(readResult.Value, null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── GetTagValueAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetTagValue_ExistingTag_ReturnsValue()
    {
        var path = CreateTestDicomFile();

        var result = await DicomFileIO.GetTagValueAsync(path, "PatientID");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("P001");
    }

    [Fact]
    public async Task GetTagValue_NonExistentFile_ReturnsNotFound()
    {
        var result = await DicomFileIO.GetTagValueAsync("C:/nonexistent.dcm", "PatientID");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }
}
