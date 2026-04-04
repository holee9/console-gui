using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines DICOM Modality Worklist (MWL) polling and patient-import operations.
/// Implemented by the HnVue.PatientManagement module using <c>IDicomService</c> for transport.
/// </summary>
public interface IWorklistService
{
    /// <summary>
    /// Polls the configured Modality Worklist SCP and returns all available orders
    /// scheduled for today's date.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the list of available worklist items,
    /// or a failure with <see cref="ErrorCode.DicomQueryFailed"/>.
    /// </returns>
    Task<Result<IReadOnlyList<WorklistItem>>> PollAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="PatientRecord"/> from a <see cref="WorklistItem"/> and registers it locally
    /// if no matching patient already exists.
    /// </summary>
    /// <param name="item">Worklist item to import.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the resulting <see cref="PatientRecord"/>,
    /// or a failure when the import or registration fails.
    /// </returns>
    Task<Result<PatientRecord>> ImportFromMwlAsync(
        WorklistItem item,
        CancellationToken cancellationToken = default);
}
