using System.IO;
using HnVue.Common.Results;

namespace HnVue.Dicom;

/// <summary>
/// Provides DICOM file read and write operations using fo-dicom.
/// </summary>
public sealed class DicomFileIO
{
    /// <summary>
    /// Reads a DICOM file and returns the parsed dataset as a wrapper.
    /// </summary>
    public static async Task<Result<DicomFileWrapper>> ReadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
            return Result.Failure<DicomFileWrapper>(
                ErrorCode.NotFound, $"DICOM file not found: '{filePath}'.");

        try
        {
            var dcmFile = await FellowOakDicom.DicomFile.OpenAsync(filePath).ConfigureAwait(false);
            return Result.Success(new DicomFileWrapper(dcmFile));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<DicomFileWrapper>(
                ErrorCode.ImageProcessingFailed, $"Failed to read DICOM file: {ex.Message}");
        }
    }

    /// <summary>
    /// Saves a DICOM file wrapper to the specified path.
    /// </summary>
    public static async Task<Result> WriteAsync(
        DicomFileWrapper wrapper,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(wrapper);
        ArgumentNullException.ThrowIfNull(outputPath);

        try
        {
            var dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await wrapper.DicomFile.SaveAsync(outputPath).ConfigureAwait(false);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.ImageProcessingFailed,
                $"Failed to write DICOM file: {ex.Message}");
        }
    }

    /// <summary>
    /// Reads a tag value from a DICOM file without loading the entire dataset.
    /// </summary>
    public static async Task<Result<string?>> GetTagValueAsync(
        string filePath,
        string tagKeyword,
        CancellationToken cancellationToken = default)
    {
        var readResult = await ReadAsync(filePath, cancellationToken).ConfigureAwait(false);
        if (readResult.IsFailure)
            return Result.Failure<string?>(readResult.Error!.Value, readResult.ErrorMessage!);

        try
        {
            var tag = FellowOakDicom.DicomDictionary.Default[tagKeyword];
            var value = readResult.Value.DicomFile.Dataset.GetSingleValueOrDefault<string>(tag, string.Empty);
            return Result.SuccessNullable<string?>(string.IsNullOrEmpty(value) ? null : value);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<string?>(ErrorCode.Unknown, $"Failed to read tag '{tagKeyword}': {ex.Message}");
        }
    }
}

/// <summary>
/// Wraps a fo-dicom <see cref="FellowOakDicom.DicomFile"/> for use in the HnVue domain layer.
/// </summary>
public sealed class DicomFileWrapper
{
    internal DicomFileWrapper(FellowOakDicom.DicomFile dicomFile)
    {
        DicomFile = dicomFile ?? throw new ArgumentNullException(nameof(dicomFile));
    }

    /// <summary>Gets the underlying fo-dicom file instance.</summary>
    public FellowOakDicom.DicomFile DicomFile { get; }

    /// <summary>Returns the SOP Instance UID from the dataset.</summary>
    public string? SopInstanceUid =>
        DicomFile.Dataset.GetSingleValueOrDefault<string?>(
            FellowOakDicom.DicomTag.SOPInstanceUID, null);

    /// <summary>Returns the Study Instance UID from the dataset.</summary>
    public string? StudyInstanceUid =>
        DicomFile.Dataset.GetSingleValueOrDefault<string?>(
            FellowOakDicom.DicomTag.StudyInstanceUID, null);

    /// <summary>Returns the patient name from the dataset.</summary>
    public string? PatientName =>
        DicomFile.Dataset.GetSingleValueOrDefault<string?>(
            FellowOakDicom.DicomTag.PatientName, null);
}
