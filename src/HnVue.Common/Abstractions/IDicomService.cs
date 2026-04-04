using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines DICOM networking operations: store to PACS, worklist query, and print.
/// Implemented by the HnVue.Dicom module.
/// </summary>
public interface IDicomService
{
    /// <summary>
    /// Sends a DICOM file to the specified PACS AE via C-STORE.
    /// </summary>
    /// <param name="dicomFilePath">Absolute path to the DICOM file to transmit.</param>
    /// <param name="pacsAeTitle">Called AE title of the destination PACS.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.DicomStoreFailed"/>.</returns>
    Task<Result> StoreAsync(
        string dicomFilePath,
        string pacsAeTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries a Modality Worklist SCP using C-FIND and returns the matching items.
    /// </summary>
    /// <param name="query">Filter criteria for the worklist query.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the list of matching <see cref="WorklistItem"/> objects,
    /// or a failure with <see cref="ErrorCode.DicomQueryFailed"/>.
    /// </returns>
    Task<Result<IReadOnlyList<WorklistItem>>> QueryWorklistAsync(
        WorklistQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a DICOM file to the specified DICOM printer AE.
    /// </summary>
    /// <param name="dicomFilePath">Absolute path to the DICOM file to print.</param>
    /// <param name="printerAeTitle">Called AE title of the DICOM printer.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.DicomPrintFailed"/>.</returns>
    Task<Result> PrintAsync(
        string dicomFilePath,
        string printerAeTitle,
        CancellationToken cancellationToken = default);
}
