using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Represents the outcome of a dose validation check performed by <c>IDoseService</c>.
/// </summary>
/// <param name="IsAllowed">True when the exposure is permitted to proceed (Allow or Warn levels).</param>
/// <param name="Level">Categorised validation level indicating the action to take.</param>
/// <param name="Message">Optional human-readable explanation, especially for Warn and Block levels.</param>
/// <param name="EstimatedDap">Estimated dose-area product in mGy·cm².</param>
/// <param name="EstimatedEsd">Estimated entrance surface dose in mGy. SWR-DM-042~043.</param>
/// <param name="ExposureIndex">Calculated exposure index per IEC 62494-1. SWR-DM-047~048.</param>
public sealed record DoseValidationResult(
    bool IsAllowed,
    DoseValidationLevel Level,
    string? Message,
    double EstimatedDap,
    double EstimatedEsd,
    double ExposureIndex);
