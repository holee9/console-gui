using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Dose;

/// <summary>
/// Implements dose validation and recording for the HnVue X-ray console.
/// Enforces a 4-level interlock: Allow / Warn / Block / Emergency.
/// </summary>
/// <remarks>
/// Dose Reference Levels (DRL) are per body part and follow IEC 60601-2-54 recommendations.
/// Emergency level triggers a system-wide safety interlock and requires physical reset.
///
/// Interlock thresholds (multiples of DRL):
///   Allow     → DAP &lt;= 1.0 × DRL
///   Warn      → DAP &lt;= 2.0 × DRL
///   Block     → DAP &lt;= 5.0 × DRL
///   Emergency → DAP &gt;  5.0 × DRL
///
/// IEC 62304 Class B — safety-critical radiation protection module.
/// </remarks>
public sealed class DoseService : IDoseService
{
    // ── Dose Reference Levels (mGy·cm²) ──────────────────────────────────────
    // Values based on European DRL guidelines (EC RP 185)

    private static readonly Dictionary<string, double> DoseReferenceLevels
        = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["CHEST"]    = 10.0,
            ["ABDOMEN"]  = 25.0,
            ["PELVIS"]   = 25.0,
            ["HAND"]     = 3.0,
            ["FOOT"]     = 3.0,
            ["SPINE"]    = 40.0,
            ["SKULL"]    = 30.0,
            ["KNEE"]     = 5.0,
            ["SHOULDER"] = 15.0,
        };

    private const double DefaultDrl       = 20.0;  // mGy·cm² fallback for unlisted body parts
    private const double WarnMultiplier   = 2.0;
    private const double BlockMultiplier  = 5.0;

    // ── kVp/mAs to DAP conversion constants (simplified linear model) ─────────
    // DAP (mGy·cm²) ≈ (kVp^2 × mAs) / normFactor — calibration constant per IEC 60601-2-54
    private const double DapNormalisationFactor = 500_000.0;

    private readonly IDoseRepository _doseRepository;

    /// <summary>
    /// Initialises a new <see cref="DoseService"/>.
    /// </summary>
    /// <param name="doseRepository">Persistence layer for dose records.</param>
    public DoseService(IDoseRepository doseRepository)
    {
        _doseRepository = doseRepository ?? throw new ArgumentNullException(nameof(doseRepository));
    }

    // ── IDoseService ──────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<Result<DoseValidationResult>> ValidateExposureAsync(
        ExposureParameters parameters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (parameters.Kvp <= 0)
            return Result.Failure<DoseValidationResult>(
                ErrorCode.ValidationFailed, "kVp must be positive.");

        if (parameters.Mas <= 0)
            return Result.Failure<DoseValidationResult>(
                ErrorCode.ValidationFailed, "mAs must be positive.");

        var estimatedDap = EstimateDap(parameters.Kvp, parameters.Mas);
        var drl = GetDrl(parameters.BodyPart);

        var level = ClassifyDose(estimatedDap, drl);
        var isAllowed = level is DoseValidationLevel.Allow or DoseValidationLevel.Warn;

        string? message = level switch
        {
            DoseValidationLevel.Allow     => null,
            DoseValidationLevel.Warn      => $"Estimated DAP {estimatedDap:F1} mGy·cm² exceeds DRL ({drl} mGy·cm²). Proceed with caution.",
            DoseValidationLevel.Block     => $"Estimated DAP {estimatedDap:F1} mGy·cm² exceeds block threshold ({drl * BlockMultiplier:F1} mGy·cm²). Exposure blocked.",
            DoseValidationLevel.Emergency => $"Estimated DAP {estimatedDap:F1} mGy·cm² exceeds EMERGENCY threshold. Safety interlock activated.",
            _                              => null,
        };

        var validationResult = new DoseValidationResult(isAllowed, level, message);
        return await Task.FromResult(Result.Success(validationResult)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result> RecordDoseAsync(
        DoseRecord dose,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dose);
        return await _doseRepository.SaveAsync(dose, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Result<DoseRecord?>> GetDoseByStudyAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);
        return await _doseRepository.GetByStudyAsync(studyInstanceUid, cancellationToken).ConfigureAwait(false);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static double EstimateDap(double kvp, double mas)
        => (kvp * kvp * mas) / DapNormalisationFactor;

    private static double GetDrl(string bodyPart)
        => DoseReferenceLevels.TryGetValue(bodyPart, out var drl) ? drl : DefaultDrl;

    private static DoseValidationLevel ClassifyDose(double estimatedDap, double drl)
    {
        if (estimatedDap <= drl)
            return DoseValidationLevel.Allow;

        if (estimatedDap <= drl * WarnMultiplier)
            return DoseValidationLevel.Warn;

        if (estimatedDap <= drl * BlockMultiplier)
            return DoseValidationLevel.Block;

        return DoseValidationLevel.Emergency;
    }
}
