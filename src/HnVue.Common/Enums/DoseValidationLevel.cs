namespace HnVue.Common.Enums;

// @MX:ANCHOR DoseValidationLevel enum - @MX:REASON: IEC 60601-2-54 dose interlock levels (Allow/Warn/Block/Emergency)
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

    /// <summary>Dose exceeds the critical emergency threshold; immediate safety interlock activation required.</summary>
    Emergency,
}
