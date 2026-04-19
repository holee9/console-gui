using HnVue.Common.Enums;

namespace HnVue.Common.Models;

// @MX:NOTE DoseValidationResult record - IEC 60601-2-54 dose check outcome with IsAllowed/Level/EstimatedDap/Ei/WarnLevel/CumulativeDap
/// <summary>
/// Represents the outcome of a dose validation check performed by <c>IDoseService</c>.
/// </summary>
/// <param name="IsAllowed">True when the exposure is permitted to proceed (Allow or Warn levels).</param>
/// <param name="Level">Categorised validation level indicating the action to take.</param>
/// <param name="Message">Optional human-readable explanation, especially for Warn and Block levels.</param>
/// <param name="EstimatedDap">Estimated dose-area product in mGy·cm².</param>
/// <param name="EstimatedEsd">Estimated entrance surface dose in mGy. SWR-DM-042~043.</param>
/// <param name="ExposureIndex">Calculated exposure index per IEC 62494-1. SWR-DM-047~048.</param>
/// <param name="WarnLevel">
/// Sub-level within the Warn tier. <see cref="DoseWarnLevel.None"/> when Level is not Warn.
/// <see cref="DoseWarnLevel.Low"/> for 1x-1.5x DRL (informational), <see cref="DoseWarnLevel.High"/> for 1.5x-2x DRL (acknowledgment required).
/// </param>
/// <param name="CumulativeDap">
/// Cumulative DAP (mGy·cm²) for the patient+body part within the configured time window (default 24h).
/// Zero when patient is not specified or no prior records exist.
/// </param>
public sealed record DoseValidationResult(
    bool IsAllowed,
    DoseValidationLevel Level,
    string? Message,
    double EstimatedDap,
    double EstimatedEsd,
    double ExposureIndex,
    DoseWarnLevel WarnLevel = DoseWarnLevel.None,
    double CumulativeDap = 0.0);
