using HnVue.Common.Results;

namespace HnVue.CDBurning;

/// <summary>
/// Abstracts retrieval of DICOM study file paths for burning.
/// </summary>
public interface IStudyRepository
{
    /// <summary>Returns absolute paths to all DICOM files in the specified study.</summary>
    Task<Result<IReadOnlyList<string>>> GetFilesForStudyAsync(
        string studyInstanceUid, CancellationToken cancellationToken = default);
}
