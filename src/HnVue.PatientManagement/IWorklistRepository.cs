using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.PatientManagement;

/// <summary>
/// Abstracts the DICOM Modality Worklist (MWL) query transport layer.
/// Implementations perform C-FIND against a configured MWL SCP.
/// </summary>
public interface IWorklistRepository
{
    /// <summary>
    /// Queries the MWL SCP for all scheduled procedures for today's date.
    /// </summary>
    Task<Result<IReadOnlyList<WorklistItem>>> QueryTodayAsync(
        CancellationToken cancellationToken = default);
}
