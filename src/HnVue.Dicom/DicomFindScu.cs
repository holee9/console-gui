using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Dicom;

/// <summary>
/// Implements DICOM C-FIND SCU for Modality Worklist queries.
/// </summary>
public sealed class DicomFindScu
{
    private readonly IDicomNetworkConfig _config;

    /// <summary>
    /// Initialises a new <see cref="DicomFindScu"/>.
    /// </summary>
    public DicomFindScu(IDicomNetworkConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Performs a DICOM Modality Worklist C-FIND query.
    /// </summary>
    /// <param name="query">Query parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching worklist items, or a failure on network error.</returns>
    public async Task<Result<IReadOnlyList<WorklistItem>>> QueryWorklistAsync(
        WorklistQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        try
        {
            var results = new List<WorklistItem>();

            var client = FellowOakDicom.Network.Client.DicomClientFactory.Create(
                _config.MwlHost,
                _config.MwlPort,
                false,
                _config.LocalAeTitle,
                query.AeTitle);

            client.NegotiateAsyncOps();

            var cfindRequest = BuildWorklistCFindRequest(query);
            cfindRequest.OnResponseReceived = (_, response) =>
            {
                if (response.Status == FellowOakDicom.Network.DicomStatus.Pending
                    && response.Dataset is not null)
                {
                    var item = MapDatasetToWorklistItem(response.Dataset);
                    if (item is not null)
                        results.Add(item);
                }
            };

            await client.AddRequestAsync(cfindRequest).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success<IReadOnlyList<WorklistItem>>(results);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed,
                $"MWL C-FIND to '{query.AeTitle}' failed: {ex.Message}");
        }
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static FellowOakDicom.Network.DicomCFindRequest BuildWorklistCFindRequest(WorklistQuery query)
    {
        var dataset = new FellowOakDicom.DicomDataset();

        // Study date filter
        if (query.DateFrom.HasValue || query.DateTo.HasValue)
        {
            var from = query.DateFrom?.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) ?? "";
            var to = query.DateTo?.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture) ?? "";
            dataset.AddOrUpdate(FellowOakDicom.DicomTag.ScheduledProcedureStepStartDate,
                from == to ? from : $"{from}-{to}");
        }

        // Patient ID filter
        if (!string.IsNullOrEmpty(query.PatientId))
            dataset.AddOrUpdate(FellowOakDicom.DicomTag.PatientID, query.PatientId);

        // Required return fields
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.AccessionNumber, "");
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.PatientName, "");
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.RequestedProcedureDescription, "");
        dataset.AddOrUpdate(FellowOakDicom.DicomTag.BodyPartExamined, "");

        var request = new FellowOakDicom.Network.DicomCFindRequest(
            FellowOakDicom.Network.DicomQueryRetrieveLevel.NotApplicable)
        {
            Dataset = dataset,
        };

        return request;
    }

    private static WorklistItem? MapDatasetToWorklistItem(FellowOakDicom.DicomDataset dataset)
    {
        try
        {
            var accession = dataset.GetSingleValueOrDefault(FellowOakDicom.DicomTag.AccessionNumber, "");
            var patientId = dataset.GetSingleValueOrDefault(FellowOakDicom.DicomTag.PatientID, "");
            var patientName = dataset.GetSingleValueOrDefault(FellowOakDicom.DicomTag.PatientName, "");
            var bodyPart = dataset.GetSingleValueOrDefault(FellowOakDicom.DicomTag.BodyPartExamined, (string?)null);
            var procedure = dataset.GetSingleValueOrDefault(FellowOakDicom.DicomTag.RequestedProcedureDescription, (string?)null);

            return new WorklistItem(accession, patientId, patientName, null, bodyPart, procedure);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return null;
        }
    }
}
