using HnVue.Common.Enums;

namespace HnVue.Common.Models;

/// <summary>
/// Represents the outcome of a dose validation check performed by <c>IDoseService</c>.
/// </summary>
/// <param name="IsAllowed">True when the exposure is permitted to proceed (Allow or Warn levels).</param>
/// <param name="Level">Categorised validation level indicating the action to take.</param>
/// <param name="Message">Optional human-readable explanation, especially for Warn and Block levels.</param>
public sealed record DoseValidationResult(
    bool IsAllowed,
    DoseValidationLevel Level,
    string? Message);
