using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="IDoseService"/> used until the HnVue.Dose module
/// is integrated in Wave 3.
/// All operations return failure results with a descriptive message.
/// </summary>
internal sealed class StubDoseService : IDoseService
{
    private const string NotImplementedMessage =
        "DoseService not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public Task<Result<DoseValidationResult>> ValidateExposureAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<DoseValidationResult>(
            ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result> RecordDoseAsync(
        DoseRecord dose,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<DoseRecord?>> GetDoseByStudyAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.SuccessNullable<DoseRecord?>(null));
}
