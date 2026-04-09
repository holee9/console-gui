using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IStudyRepository - @MX:REASON: DICOM study persistence with patient association, 4 CRUD operations
/// <summary>
/// Defines persistence operations for DICOM study records.
/// Implemented by the HnVue.Data module.
/// </summary>
public interface IStudyRepository
{
    /// <summary>Persists a new study record.</summary>
    /// <param name="study">The study to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<StudyRecord>> AddAsync(
        StudyRecord study,
        CancellationToken cancellationToken = default);

    /// <summary>Returns all studies associated with the specified patient.</summary>
    /// <param name="patientId">Patient identifier to filter by.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<IReadOnlyList<StudyRecord>>> GetByPatientAsync(
        string patientId,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the study with the given DICOM Study Instance UID, or <see langword="null"/>.</summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<StudyRecord?>> GetByUidAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces all mutable fields of an existing study record.</summary>
    /// <param name="study">Updated study record; the <c>StudyInstanceUid</c> identifies the row.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdateAsync(
        StudyRecord study,
        CancellationToken cancellationToken = default);
}
