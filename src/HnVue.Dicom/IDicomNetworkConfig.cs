namespace HnVue.Dicom;

/// <summary>
/// Provides DICOM network endpoint configuration for SCU operations.
/// </summary>
public interface IDicomNetworkConfig
{
    /// <summary>Gets the PACS server hostname or IP address.</summary>
    string PacsHost { get; }

    /// <summary>Gets the PACS DICOM port (typically 11112).</summary>
    int PacsPort { get; }

    /// <summary>Gets the PACS Application Entity title.</summary>
    string PacsAeTitle { get; }

    /// <summary>Gets the local (SCU) Application Entity title.</summary>
    string LocalAeTitle { get; }

    /// <summary>Gets the MWL server hostname or IP address.</summary>
    string MwlHost { get; }

    /// <summary>Gets the MWL server DICOM port (typically 11113).</summary>
    int MwlPort { get; }
}
