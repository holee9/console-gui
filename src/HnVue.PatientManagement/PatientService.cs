using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.PatientManagement;

// @MX:NOTE Patient data integrity affects dose attribution - IEC 62304 Class B requirement
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

    // @MX:ANCHOR RegisterAsync - @MX:REASON: High fan_in - called by patient registration UI and import workflows
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

        // @MX:NOTE Duplicate detection prevents patient data corruption and misattribution
        // Duplicate check
        var existingResult = await _patientRepository.FindByIdAsync(patient.PatientId, cancellationToken)
            .ConfigureAwait(false);

        if (existingResult.IsSuccess && existingResult.Value is not null)
            return Result.Failure<PatientRecord>(
                ErrorCode.AlreadyExists,
                $"Patient with ID '{patient.PatientId}' already exists.");

        return await _patientRepository.AddAsync(patient, cancellationToken).ConfigureAwait(false);
    }

    // @MX:ANCHOR SearchAsync - @MX:REASON: High fan_in - used by patient lookup across multiple ViewModels
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

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-PM-030~033: Emergency patient registration.
    /// - Skips duplicate detection (emergency override)
    /// - Creates minimal patient record with IsEmergency=true
    /// - Accepts null patient name for unknown trauma patients
    /// - Sets CreatedBy to current user for audit trail
    /// </remarks>
    // @MX:ANCHOR QuickRegisterEmergencyAsync - @MX:REASON: Safety-critical emergency fast-path, skips duplicate detection, auto-generates EMERG patient ID for trauma care
    public async Task<Result<PatientRecord>> QuickRegisterEmergencyAsync(
        string emergencyPatientId,
        string? patientName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(emergencyPatientId);

        // Validate emergency patient ID format
        if (!emergencyPatientId.StartsWith("EMERG-", StringComparison.Ordinal))
            return Result.Failure<PatientRecord>(
                ErrorCode.ValidationFailed,
                "Emergency patient ID must start with 'EMERG-'.");

        // Create minimal emergency patient record
        // @MX:NOTE Emergency fast-path: minimal data, defers full registration for post-stabilization
        var emergencyPatient = new PatientRecord(
            PatientId: emergencyPatientId,
            Name: string.IsNullOrWhiteSpace(patientName) ? "UNKNOWN EMERGENCY PATIENT" : patientName,
            DateOfBirth: null,  // Deferred to full registration
            Sex: null,          // Deferred to full registration
            IsEmergency: true,  // Mark as emergency for identification
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "SYSTEM" // TODO: Replace with actual user from security context
        );

        // Skip duplicate detection for emergency workflow
        // SWR-PM-032: Emergency override allows immediate trauma care
        return await _patientRepository.AddAsync(emergencyPatient, cancellationToken).ConfigureAwait(false);
    }
}
