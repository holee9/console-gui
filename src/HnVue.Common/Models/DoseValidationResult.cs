namespace HnVue.Common.Models;

/// <summary>
/// Specifies the action recommended after evaluating proposed exposure parameters against dose limits.
/// </summary>
public enum DoseValidationLevel
{
    /// <summary>Dose is within normal operating range; proceed without warning.</summary>
    Allow,

    /// <summary>Dose is elevated; display a warning to the operator before proceeding.</summary>
    Warn,

    /// <summary>Dose exceeds the safety limit; the exposure must be blocked.</summary>
    Block,
}

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
