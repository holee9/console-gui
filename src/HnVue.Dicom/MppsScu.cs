using System.Globalization;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using HnVue.Common.Results;

namespace HnVue.Dicom;

/// <summary>
/// Implements DICOM MPPS SCU (Modality Performed Procedure Step).
/// Sends N-CREATE (In-Progress) and N-SET (Completed/Discontinued) messages
/// to the MPPS SCP to track acquisition lifecycle.
/// SWR-DC-055 (N-CREATE) / SWR-DC-056 (N-SET). Issue #22.
/// SOP Class: 1.2.840.10008.3.1.2.3.3 (Modality Performed Procedure Step SOP Class)
/// </summary>
public sealed class MppsScu
{
    private static readonly DicomUID MppsSopClass =
        DicomUID.ModalityPerformedProcedureStep;

    private readonly DicomOptions _options;

    /// <summary>Initialises a new <see cref="MppsScu"/>.</summary>
    public MppsScu(DicomOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    // @MX:NOTE N-CREATE sets PerformedProcedureStepStatus IN PROGRESS, Modality DX, SOP Class 1.2.840.10008.3.1.2.3.3
    /// <summary>
    /// Sends MPPS N-CREATE to mark a procedure step as In-Progress.
    /// SWR-DC-055.
    /// </summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID.</param>
    /// <param name="patientId">Patient identifier.</param>
    /// <param name="bodyPart">Body part examined code (e.g., "CHEST").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The MPPS SOP Instance UID on success. Failure when network connection or
    /// SCP response indicates an error.
    /// </returns>
    public async Task<Result<string>> SendInProgressAsync(
        string studyInstanceUid,
        string patientId,
        string bodyPart,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.MppsHost))
            return Result.Failure<string>(ErrorCode.DicomConnectionFailed,
                "MPPS host is not configured. Set Dicom:MppsHost in configuration.");

        var mppsUid = DicomUID.Generate();
        var mppsInstanceUid = mppsUid.UID;

        var dataset = new DicomDataset
        {
            { DicomTag.SOPClassUID, MppsSopClass },
            { DicomTag.SOPInstanceUID, mppsUid },
            { DicomTag.StudyInstanceUID, studyInstanceUid },
            { DicomTag.PatientID, patientId },
            { DicomTag.BodyPartExamined, bodyPart.ToUpperInvariant() },
            { DicomTag.PerformedProcedureStepStatus, "IN PROGRESS" },
            { DicomTag.PerformedProcedureStepStartDate, DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) },
            { DicomTag.PerformedProcedureStepStartTime, DateTime.UtcNow.ToString("HHmmss", CultureInfo.InvariantCulture) },
            { DicomTag.Modality, "DX" },
        };

        string? errorMessage = null;

        var request = new DicomNCreateRequest(MppsSopClass, mppsUid)
        {
            Dataset = dataset
        };
        request.OnResponseReceived = (_, response) =>
        {
            if (response.Status != DicomStatus.Success)
                errorMessage = $"MPPS N-CREATE SCP returned: {response.Status}";
        };

        try
        {
            var client = DicomClientFactory.Create(
                _options.MppsHost, _options.MppsPort,
                _options.TlsEnabled, _options.LocalAeTitle, _options.MppsAeTitle);
            await client.AddRequestAsync(request).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            if (errorMessage is not null)
                return Result.Failure<string>(ErrorCode.DicomConnectionFailed, errorMessage);

            return Result.Success(mppsInstanceUid);
        }
        catch (DicomNetworkException ex)
        {
            return Result.Failure<string>(ErrorCode.DicomConnectionFailed,
                $"MPPS N-CREATE network error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<string>(ErrorCode.OperationCancelled, "MPPS N-CREATE was cancelled.");
        }
        catch (Exception ex) when (ex is not DicomNetworkException
                                    and not OperationCanceledException
                                    and not OutOfMemoryException)
        {
            return Result.Failure<string>(
                ErrorCode.DicomConnectionFailed,
                $"MPPS N-CREATE failed: {ex.GetBaseException().Message}");
        }
    }

    // @MX:NOTE N-SET updates PerformedProcedureStepStatus to COMPLETED or DISCONTINUED with UTC end timestamp
    /// <summary>
    /// Sends MPPS N-SET to mark a procedure step as Completed or Discontinued.
    /// SWR-DC-056.
    /// </summary>
    /// <param name="mppsInstanceUid">The SOP Instance UID returned by <see cref="SendInProgressAsync"/>.</param>
    /// <param name="completed">
    /// When <see langword="true"/>, status is set to "COMPLETED".
    /// When <see langword="false"/>, status is set to "DISCONTINUED".
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task<Result> SendCompletedAsync(
        string mppsInstanceUid,
        bool completed = true,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.MppsHost))
            return Result.Failure(ErrorCode.DicomConnectionFailed,
                "MPPS host is not configured. Set Dicom:MppsHost in configuration.");

        ArgumentNullException.ThrowIfNull(mppsInstanceUid);

        var status = completed ? "COMPLETED" : "DISCONTINUED";

        var modification = new DicomDataset
        {
            { DicomTag.PerformedProcedureStepStatus, status },
            { DicomTag.PerformedProcedureStepEndDate, DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture) },
            { DicomTag.PerformedProcedureStepEndTime, DateTime.UtcNow.ToString("HHmmss", CultureInfo.InvariantCulture) },
        };

        string? errorMessage = null;

        var mppsUid = new DicomUID(mppsInstanceUid, "MPPS Instance", DicomUidType.SOPInstance);
        var request = new DicomNSetRequest(MppsSopClass, mppsUid)
        {
            Dataset = modification
        };
        request.OnResponseReceived = (_, response) =>
        {
            if (response.Status != DicomStatus.Success)
                errorMessage = $"MPPS N-SET SCP returned: {response.Status}";
        };

        try
        {
            var client = DicomClientFactory.Create(
                _options.MppsHost, _options.MppsPort,
                _options.TlsEnabled, _options.LocalAeTitle, _options.MppsAeTitle);
            await client.AddRequestAsync(request).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            return errorMessage is not null
                ? Result.Failure(ErrorCode.DicomConnectionFailed, errorMessage)
                : Result.Success();
        }
        catch (DicomNetworkException ex)
        {
            return Result.Failure(ErrorCode.DicomConnectionFailed,
                $"MPPS N-SET network error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "MPPS N-SET was cancelled.");
        }
        catch (Exception ex) when (ex is not DicomNetworkException
                                    and not OperationCanceledException
                                    and not OutOfMemoryException)
        {
            return Result.Failure(
                ErrorCode.DicomConnectionFailed,
                $"MPPS N-SET failed: {ex.GetBaseException().Message}");
        }
    }
}
