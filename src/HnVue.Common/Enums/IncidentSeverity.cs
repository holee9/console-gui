namespace HnVue.Common.Enums;

/// <summary>
/// Classifies the severity of an incident recorded by the incident management module.
/// Severity determines escalation and notification behaviour per IEC 62304 risk management.
/// </summary>
public enum IncidentSeverity
{
    /// <summary>Immediate patient safety impact; requires immediate system stop and investigation.</summary>
    Critical,

    /// <summary>Significant risk to patient or operator; requires urgent attention.</summary>
    High,

    /// <summary>Moderate risk; does not require immediate stop but must be investigated.</summary>
    Medium,

    /// <summary>Minor issue with no immediate risk; logged for periodic review.</summary>
    Low,
}
