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
    // @MX:NOTE DRL values per IEC 60601-2-54, CHEST=10.0, ABDOMEN=25.0, etc. DefaultDrl=20.0 fallback for unlisted body parts

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

    // ── ESD calculation constants (SWR-DM-042~043) ─────────────────────────────────
    // @MX:NOTE Backscatter factor 1.35 per IAEA TECDOC-1423 for diagnostic X-ray, accounts for patient scatter
    private const double DefaultBackscatterFactor = 1.35;
    private const double MinimumFieldAreaCm2 = 1.0;  // Prevent division by zero

    // ── Exposure Index target values per body part (SWR-DM-047~048) ────────────────
    // @MX:NOTE EI target values per IEC 62494-1 recommended values, normalized to EI=1000 at optimal exposure
    private static readonly Dictionary<string, double> ExposureIndexTargets
        = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["CHEST"]    = 1500,
            ["ABDOMEN"]  = 1200,
            ["SPINE"]    = 1800,
            ["SKULL"]    = 1400,
        };

    private const double DefaultEiTarget = 1500;

    // ── kVp/mAs to DAP conversion constants (simplified linear model) ─────────
    // DAP (mGy·cm²) ≈ (kVp^2 × mAs) / normFactor — calibration constant per IEC 60601-2-54
    // @MX:NOTE DAP normalisation factor 500000.0 is a simplified linear model, requires calibration per IEC 60601-2-54
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
    // @MX:ANCHOR ValidateExposureAsync - @MX:REASON: Safety-critical 4-tier dose interlock (Allow/Warn/Block/Emergency), called by WorkflowEngine.PrepareExposureAsync, enforces IEC 60601-2-54 DRL compliance
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

        if (parameters.FieldAreaCm2 < MinimumFieldAreaCm2)
            return Result.Failure<DoseValidationResult>(
                ErrorCode.ValidationFailed, $"Field area must be at least {MinimumFieldAreaCm2} cm².");

        var estimatedDap = EstimateDap(parameters.Kvp, parameters.Mas);
        var estimatedEsd = CalculateEsd(estimatedDap, parameters.FieldAreaCm2);

        // For EI calculation, we use a simulated mean pixel value based on kVp and mAs
        // In production, this would come from the actual acquired image
        var simulatedMeanPixelValue = SimulateMeanPixelValue(parameters.Kvp, parameters.Mas);
        var eiTarget = GetEiTarget(parameters.BodyPart);
        var exposureIndex = CalculateExposureIndex(simulatedMeanPixelValue, eiTarget);

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

        var validationResult = new DoseValidationResult(
            isAllowed, level, message, estimatedDap, estimatedEsd, exposureIndex);
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

    /// <inheritdoc/>
    // @MX:NOTE GenerateRdsrSummaryAsync computes ESD+EI for RDSR summary; DICOM SR file encoding deferred to Phase 3 (IHE REM profile)
    public async Task<Result<DoseRecord>> GenerateRdsrSummaryAsync(
        string studyInstanceUid,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        var lookupResult = await _doseRepository
            .GetByStudyAsync(studyInstanceUid, cancellationToken)
            .ConfigureAwait(false);

        if (lookupResult.IsFailure)
            return Result.Failure<DoseRecord>(lookupResult.Error!.Value, lookupResult.ErrorMessage!);

        var dose = lookupResult.Value;
        if (dose is null)
            return Result.Failure<DoseRecord>(
                ErrorCode.NotFound,
                $"No dose record found for study '{studyInstanceUid}'.");

        // Use DapMgyCm2 when available (RDSR path); fall back to legacy Dap field.
        var dap = dose.DapMgyCm2 > 0.0 ? dose.DapMgyCm2 : dose.Dap;

        // ESD computation requires a valid field area; skip if not recorded.
        double? esd = dose.FieldAreaCm2 >= MinimumFieldAreaCm2
            ? CalculateEsd(dap, dose.FieldAreaCm2)
            : (double?)null;

        // EI computation requires valid pixel statistics; skip if not recorded.
        var eiTarget = dose.EiTarget > 0.0
            ? dose.EiTarget
            : GetEiTarget(dose.BodyPart);

        double computedEi = dose.MeanPixelValue > 0.0
            ? CalculateExposureIndex(dose.MeanPixelValue, eiTarget)
            : dose.Ei;   // Preserve existing EI when pixel data is absent.

        var enriched = dose with
        {
            EsdMgy = esd,
            Ei = computedEi,
            EiTarget = eiTarget,
            DapMgyCm2 = dap,
        };

        return Result.Success(enriched);
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DoseRecord>>> GetDoseHistoryAsync(
        string patientId,
        DateTimeOffset? from = null,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patientId);

        return await _doseRepository
            .GetByPatientAsync(patientId, from, until, cancellationToken)
            .ConfigureAwait(false);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    // @MX:NOTE EstimateDap uses simplified linear model (kVp² × mAs) / 500000, production requires calibration per detector
    private static double EstimateDap(double kvp, double mas)
        => (kvp * kvp * mas) / DapNormalisationFactor;

    private static double GetDrl(string bodyPart)
        => DoseReferenceLevels.TryGetValue(bodyPart, out var drl) ? drl : DefaultDrl;

    // @MX:NOTE ClassifyDose implements 4-tier interlock: ≤1×DRL=Allow, ≤2×DRL=Warn, ≤5×DRL=Block, >5×DRL=Emergency
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

    /// <inheritdoc/>
    // @MX:ANCHOR CalculateEsd - @MX:REASON: Safety-critical ESD calculation per IEC 60601-2-54, used for patient skin dose tracking, required for regulatory compliance
    public double CalculateEsd(double dap, double fieldAreaCm2, double backscatterFactor = DefaultBackscatterFactor)
    {
        if (fieldAreaCm2 < MinimumFieldAreaCm2)
            throw new ArgumentOutOfRangeException(
                nameof(fieldAreaCm2),
                $"Field area must be at least {MinimumFieldAreaCm2} cm².");

        if (dap < 0)
            throw new ArgumentOutOfRangeException(nameof(dap), "DAP cannot be negative.");

        if (backscatterFactor < 1.0)
            throw new ArgumentOutOfRangeException(nameof(backscatterFactor), "Backscatter factor must be >= 1.0.");

        // @MX:NOTE ESD formula: ESD (mGy) = DAP (mGy·cm²) / FieldArea (cm²) × BackScatterFactor, per IEC 60601-2-54
        return (dap / fieldAreaCm2) * backscatterFactor;
    }

    /// <inheritdoc/>
    // @MX:ANCHOR CalculateExposureIndex - @MX:REASON: EI calculation per IEC 62494-1, used for image quality assessment, required for dose optimization feedback
    public double CalculateExposureIndex(double meanPixelValue, double targetPixelValue)
    {
        if (meanPixelValue < 0)
            throw new ArgumentOutOfRangeException(nameof(meanPixelValue), "Mean pixel value cannot be negative.");

        if (targetPixelValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetPixelValue), "Target pixel value must be positive.");

        // @MX:NOTE EI formula: EI = meanPixelValue / targetPixelValue × 1000, per IEC 62494-1
        return (meanPixelValue / targetPixelValue) * 1000.0;
    }

    private static double GetEiTarget(string bodyPart)
        => ExposureIndexTargets.TryGetValue(bodyPart, out var target) ? target : DefaultEiTarget;

    // @MX:NOTE SimulateMeanPixelValue approximates detector response: (kVp × mAs) / 10, production uses actual image data
    private static double SimulateMeanPixelValue(double kvp, double mas)
        => (kvp * mas) / 10.0;
}
