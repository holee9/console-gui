using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR IDicomService - @MX:REASON: DICOM networking contract with 8+ consumers, PACS integration boundary
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

    /// <summary>
    /// Requests Storage Commitment from the PACS after a successful C-STORE.
    /// Sends N-ACTION and awaits N-EVENT-REPORT to confirm persistent storage.
    /// SWR-DC-057 (N-ACTION SCU) / SWR-DC-058 (N-EVENT-REPORT). Issue #23.
    /// </summary>
    /// <param name="sopClassUid">SOP Class UID of the stored instance.</param>
    /// <param name="sopInstanceUid">SOP Instance UID of the stored instance.</param>
    /// <param name="pacsAeTitle">Called AE title of the PACS that received the C-STORE.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the PACS confirms commitment,
    /// or a failure with <see cref="ErrorCode.DicomStoreFailed"/> when commitment is refused.
    /// </returns>
    Task<Result> RequestStorageCommitmentAsync(
        string sopClassUid,
        string sopInstanceUid,
        string pacsAeTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls the print job status via N-GET after a print action has been submitted.
    /// Queries the Print Job SOP Instance on the printer SCP until Done or Failure.
    /// </summary>
    /// <param name="filmSessionUid">SOP Instance UID of the Basic Film Session.</param>
    /// <param name="printerAeTitle">Called AE title of the DICOM printer.</param>
    /// <param name="cancellationToken">Token to cancel the polling operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing the <see cref="PrintJobStatus"/>,
    /// or a failure with <see cref="ErrorCode.DicomPrintFailed"/>.
    /// </returns>
    Task<Result<PrintJobStatus>> GetPrintJobStatusAsync(
        string filmSessionUid,
        string printerAeTitle,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an RDSR (Radiation Dose Structured Report) from the specified dose record
    /// and sends it to the PACS via C-STORE.
    /// SWR-DC-060~065: RDSR generation and transmission.
    /// </summary>
    /// <param name="doseRecord">Dose record with exposure metrics.</param>
    /// <param name="patientInfo">Patient demographic information for the RDSR.</param>
    /// <param name="studyInfo">Study-level DICOM information for the RDSR.</param>
    /// <param name="pacsAeTitle">Called AE title of the destination PACS or dose registry.</param>
    /// <param name="exposureParams">Exposure technique parameters (kVp, mAs, exposure time). Null = omitted from RDSR.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the RDSR is generated and accepted by PACS,
    /// or a failure with <see cref="ErrorCode.DicomStoreFailed"/>.
    /// </returns>
    Task<Result> SendRdsrAsync(
        DoseRecord doseRecord,
        RdsrPatientInfo patientInfo,
        RdsrStudyInfo studyInfo,
        string pacsAeTitle,
        RdsrExposureParams? exposureParams = null,
        CancellationToken cancellationToken = default);
}
