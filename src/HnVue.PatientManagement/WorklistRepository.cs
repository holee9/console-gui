using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;

namespace HnVue.PatientManagement;

/// <summary>
/// DICOM MWL-based implementation of <see cref="IWorklistRepository"/>.
/// Performs a C-FIND query against the configured Modality Worklist SCP via <see cref="IDicomService"/>.
/// Connection failures return an empty list because the worklist is advisory only.
/// Issue #24: Refactored from direct DicomFindScu usage to IDicomService single entry point.
/// </summary>
public sealed class WorklistRepository : IWorklistRepository
{
    private readonly IDicomService _dicomService;
    private readonly IDicomNetworkConfig _config;

    /// <summary>
    /// Initialises a new <see cref="WorklistRepository"/>.
    /// </summary>
    /// <param name="dicomService">DICOM service used as single entry point for all DICOM operations.</param>
    /// <param name="config">DICOM network configuration providing MWL endpoint details.</param>
    public WorklistRepository(IDicomService dicomService, IDicomNetworkConfig config)
    {
        _dicomService = dicomService ?? throw new ArgumentNullException(nameof(dicomService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-DA-010: Worklist is queried for today's scheduled procedures.
    /// Network failures are swallowed and an empty list is returned so that
    /// the acquisition workflow is never blocked by an unavailable MWL SCP.
    /// </remarks>
    public async Task<Result<IReadOnlyList<WorklistItem>>> QueryTodayAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var query = new WorklistQuery(
            PatientId: null,
            DateFrom: today,
            DateTo: today,
            AeTitle: _config.PacsAeTitle);

        var queryResult = await _dicomService
            .QueryWorklistAsync(query, cancellationToken)
            .ConfigureAwait(false);

        // Worklist is advisory: return empty list on network/DICOM failure.
        if (queryResult.IsFailure)
        {
            IReadOnlyList<WorklistItem> empty = Array.Empty<WorklistItem>();
            return Result.Success(empty);
        }

        return queryResult;
    }
}
