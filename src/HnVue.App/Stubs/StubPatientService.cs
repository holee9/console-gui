using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="IPatientService"/> used until the HnVue.PatientManagement module
/// is integrated in Wave 3.
/// Search returns an empty list; all mutating operations return failure results.
/// </summary>
internal sealed class StubPatientService : IPatientService
{
    private const string NotImplementedMessage =
        "PatientService not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public Task<Result<PatientRecord>> RegisterAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<PatientRecord>(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<PatientRecord> empty = Array.Empty<PatientRecord>();
        return Task.FromResult(Result.Success<IReadOnlyList<PatientRecord>>(empty));
    }

    /// <inheritdoc/>
    public Task<Result> UpdateAsync(
        PatientRecord patient,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<PatientRecord?>> GetByIdAsync(
        string patientId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.SuccessNullable<PatientRecord?>(null));

    /// <inheritdoc/>
    public Task<Result> DeleteAsync(
        string patientId,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<PatientRecord>> QuickRegisterEmergencyAsync(
        string emergencyPatientId,
        string? patientName,
        CancellationToken cancellationToken = default)
    {
        var record = new PatientRecord(
            PatientId: emergencyPatientId,
            Name: patientName ?? string.Empty,
            DateOfBirth: null,
            Sex: null,
            IsEmergency: true,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: "STUB");
        return Task.FromResult(Result.Success(record));
    }
}
