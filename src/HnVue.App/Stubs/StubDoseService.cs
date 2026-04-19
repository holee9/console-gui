using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
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

    /// <inheritdoc/>
    public double CalculateEsd(double dap, double fieldAreaCm2, double backscatterFactor = 1.35)
        => 0.0;

    /// <inheritdoc/>
    public double CalculateExposureIndex(double meanPixelValue, double targetPixelValue)
        => targetPixelValue > 0.0 ? meanPixelValue / targetPixelValue * 1000.0 : 0.0;

    /// <inheritdoc/>
    public Task<Result<DoseRecord>> GenerateRdsrSummaryAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<DoseRecord>(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<IReadOnlyList<DoseRecord>>> GetDoseHistoryAsync(
        string patientId,
        DateTimeOffset? from = null,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<IReadOnlyList<DoseRecord>>(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<double> GetCumulativeDapAsync(
        string patientId,
        string bodyPart,
        double windowHours = 24.0,
        CancellationToken cancellationToken = default)
        => Task.FromResult(0.0);

    /// <inheritdoc/>
    public Task<Result> TriggerInterlockAsync(
        DoseValidationLevel level,
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success());

    /// <inheritdoc/>
    public event EventHandler<DoseInterlockEventArgs>? InterlockTriggered;
}
