namespace HnVue.Dicom;

/// <summary>
/// Provides DICOM network endpoint configuration for SCU operations.
/// </summary>
public interface IDicomNetworkConfig
{
    string PacsHost { get; }
    int PacsPort { get; }
    string PacsAeTitle { get; }
    string LocalAeTitle { get; }
    string MwlHost { get; }
    int MwlPort { get; }
}
