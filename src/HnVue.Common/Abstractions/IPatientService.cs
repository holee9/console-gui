using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines patient management business-logic operations.
/// Implemented by the HnVue.PatientManagement module.
/// </summary>
public interface IPatientService
{
    /// <summary>
    /// Registers a new patient in the system after validating demographics and checking for duplicates.
    /// </summary>
    /// <param name="patient">Patient demographics to register.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the persisted <see cref="PatientRecord"/>,
    /// or a failure with <see cref="ErrorCode.AlreadyExists"/> or <see cref="ErrorCode.ValidationFailed"/>.
    /// </returns>
    Task<Result<PatientRecord>> RegisterAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default);

    /// <summary>Searches for patients matching the provided query string against name and ID fields.</summary>
    /// <param name="query">Free-text search term (case-insensitive).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default);

    /// <summary>Updates mutable demographic fields for an existing patient.</summary>
    /// <param name="patient">Updated patient record; the <c>PatientId</c> identifies the row.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdateAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the patient with the given identifier, or <see langword="null"/> if not found.</summary>
    /// <param name="patientId">Patient identifier to look up.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result<PatientRecord?>> GetByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a patient and all associated studies. Restricted to Admin role.
    /// </summary>
    /// <remarks>Requires active study check before deletion. SWR-PM-050.</remarks>
    Task<Result> DeleteAsync(string patientId, CancellationToken cancellationToken = default);
}
