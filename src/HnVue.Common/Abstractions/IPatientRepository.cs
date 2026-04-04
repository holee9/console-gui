using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines CRUD operations for patient demographic records.
/// Implemented by the HnVue.Data module.
/// </summary>
public interface IPatientRepository
{
    /// <summary>Persists a new patient record and returns it with any generated fields populated.</summary>
    /// <param name="patient">The patient to add.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<PatientRecord>> AddAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the patient with the given identifier, or <see langword="null"/> if not found.</summary>
    /// <param name="patientId">Patient identifier to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<PatientRecord?>> FindByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all patients whose name or ID contains the search query (case-insensitive).
    /// </summary>
    /// <param name="query">Free-text search term.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>Replaces all mutable fields of an existing patient record.</summary>
    /// <param name="patient">Updated patient record; the <c>PatientId</c> identifies the row.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdateAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default);

    /// <summary>Permanently removes a patient record from the store.</summary>
    /// <param name="patientId">Identifier of the patient to remove.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> DeleteAsync(
        string patientId,
        CancellationToken cancellationToken = default);
}
