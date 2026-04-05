namespace HnVue.Dicom;

/// <summary>
/// Strongly-typed configuration options for the DICOM module.
/// Bound from the "Dicom" section of appsettings.json.
/// </summary>
public sealed class DicomOptions
{
    /// <summary>Gets or sets the local AE title used as the SCU identity for all DICOM associations.</summary>
    public string LocalAeTitle { get; set; } = "HNVUE";

    // ── PACS (C-STORE destination) ─────────────────────────────────────────────

    /// <summary>Gets or sets the called AE title of the primary PACS.</summary>
    public string PacsAeTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the hostname or IP address of the primary PACS.</summary>
    public string PacsHost { get; set; } = string.Empty;

    /// <summary>Gets or sets the TCP port of the primary PACS.</summary>
    public int PacsPort { get; set; } = 104;

    // ── Modality Worklist (C-FIND destination) ─────────────────────────────────

    /// <summary>Gets or sets the called AE title of the Modality Worklist SCP.</summary>
    public string MwlAeTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the hostname or IP address of the Modality Worklist SCP.</summary>
    public string MwlHost { get; set; } = string.Empty;

    /// <summary>Gets or sets the TCP port of the Modality Worklist SCP.</summary>
    public int MwlPort { get; set; } = 104;

    // ── Printer (N-CREATE / N-SET / N-ACTION destination) ─────────────────────

    /// <summary>Gets or sets the called AE title of the DICOM print SCP.</summary>
    public string PrinterAeTitle { get; set; } = string.Empty;

    /// <summary>Gets or sets the hostname or IP address of the DICOM print SCP.</summary>
    public string PrinterHost { get; set; } = string.Empty;

    /// <summary>Gets or sets the TCP port of the DICOM print SCP.</summary>
    public int PrinterPort { get; set; } = 104;

    // ── TLS ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets or sets a value indicating whether TLS should be negotiated for all DICOM associations.
    /// </summary>
    public bool TlsEnabled { get; set; }
}
