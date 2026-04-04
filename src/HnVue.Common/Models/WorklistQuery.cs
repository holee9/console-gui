namespace HnVue.Common.Models;

/// <summary>
/// Defines the parameters for a DICOM Modality Worklist C-FIND query.
/// </summary>
/// <param name="PatientId">Optional patient ID filter; null to query for all patients.</param>
/// <param name="DateFrom">Optional start of the scheduled study date range (inclusive); null for no lower bound.</param>
/// <param name="DateTo">Optional end of the scheduled study date range (inclusive); null for no upper bound.</param>
/// <param name="AeTitle">Called AE title of the Modality Worklist SCP to query.</param>
public sealed record WorklistQuery(
    string? PatientId,
    DateOnly? DateFrom,
    DateOnly? DateTo,
    string AeTitle);
