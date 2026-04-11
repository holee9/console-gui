using HnVue.Common.Results;

namespace HnVue.CDBurning;

/// <summary>
/// Abstracts retrieval of DICOM study file paths for CD burning via the EF Core data layer.
/// Used by <see cref="EfCdStudyRepository"/> in HnVue.Data for cross-layer DI bridging.
/// REQ-COORD-006: SPEC-COORDINATOR-001 EF Core CD study file path query.
/// </summary>
public interface ICdStudyRepository
{
    /// <summary>Returns absolute paths to all DICOM files in the specified study.</summary>
    Task<Result<IReadOnlyList<string>>> GetFilesForStudyAsync(
        string studyInstanceUid, CancellationToken cancellationToken = default);
}
