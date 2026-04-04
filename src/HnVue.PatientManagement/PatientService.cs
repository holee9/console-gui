using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.PatientManagement;

/// <summary>
/// Implements patient registration, search, and management business logic.
/// </summary>
/// <remarks>
/// Validates demographics before persistence and guards against duplicate registrations.
/// IEC 62304 Class B — patient data integrity is safety-relevant for correct dose attribution.
/// </remarks>
public sealed class PatientService : IPatientService
{
    private readonly IPatientRepository _patientRepository;

    /// <summary>
    /// Initialises a new <see cref="PatientService"/>.
    /// </summary>
    /// <param name="patientRepository">Persistence layer for patient records.</param>
    public PatientService(IPatientRepository patientRepository)
    {
        _patientRepository = patientRepository
            ?? throw new ArgumentNullException(nameof(patientRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<PatientRecord>> RegisterAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patient);

        // Validate required fields
        if (string.IsNullOrWhiteSpace(patient.PatientId))
            return Result.Failure<PatientRecord>(
                ErrorCode.ValidationFailed, "PatientId is required.");

        if (string.IsNullOrWhiteSpace(patient.Name))
            return Result.Failure<PatientRecord>(
                ErrorCode.ValidationFailed, "Patient name is required.");

        // Duplicate check
        var existingResult = await _patientRepository.FindByIdAsync(patient.PatientId, cancellationToken)
            .ConfigureAwait(false);

        if (existingResult.IsSuccess && existingResult.Value is not null)
            return Result.Failure<PatientRecord>(
                ErrorCode.AlreadyExists,
                $"Patient with ID '{patient.PatientId}' already exists.");

        return await _patientRepository.AddAsync(patient, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query))
            return Result.Failure<IReadOnlyList<PatientRecord>>(
                ErrorCode.ValidationFailed, "Search query cannot be empty.");

        return await _patientRepository.SearchAsync(query, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patient);

        if (string.IsNullOrWhiteSpace(patient.PatientId))
            return Result.Failure(ErrorCode.ValidationFailed, "PatientId is required.");

        return await _patientRepository.UpdateAsync(patient, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<PatientRecord?>> GetByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patientId);
        return await _patientRepository.FindByIdAsync(patientId, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(
        string patientId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patientId);

        var existing = await _patientRepository.FindByIdAsync(patientId, cancellationToken)
            .ConfigureAwait(false);

        if (existing.IsFailure || existing.Value is null)
            return Result.Failure(ErrorCode.NotFound, $"Patient '{patientId}' not found.");

        return await _patientRepository.DeleteAsync(patientId, cancellationToken).ConfigureAwait(false);
    }
}
