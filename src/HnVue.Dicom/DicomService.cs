using System.Globalization;
using System.IO;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HnVue.Dicom;

/// <summary>
/// Implements <see cref="HnVue.Common.Abstractions.IDicomService"/> using the fo-dicom 5.x async client API.
/// Provides C-STORE SCU, Modality Worklist C-FIND SCU, and DICOM Print SCU operations.
/// </summary>
public sealed partial class DicomService : HnVue.Common.Abstractions.IDicomService
{
    private readonly DicomOptions _options;
    private readonly ILogger<DicomService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="DicomService"/>.
    /// </summary>
    /// <param name="options">Bound DICOM configuration options.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    public DicomService(IOptions<DicomOptions> options, ILogger<DicomService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result> StoreAsync(
        string dicomFilePath,
        string pacsAeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dicomFilePath))
            return Result.Failure(ErrorCode.DicomStoreFailed, "DICOM file path must not be empty.");

        if (string.IsNullOrWhiteSpace(pacsAeTitle))
            return Result.Failure(ErrorCode.DicomStoreFailed, "PACS AE title must not be empty.");

        if (!System.IO.File.Exists(dicomFilePath))
            return Result.Failure(ErrorCode.DicomStoreFailed, $"DICOM file not found: {dicomFilePath}");

        try
        {
            var dicomFile = await DicomFile.OpenAsync(dicomFilePath).ConfigureAwait(false);

            var storeResult = Result.Success();
            var request = new DicomCStoreRequest(dicomFile);
            request.OnResponseReceived = (req, response) =>
            {
                if (response.Status != DicomStatus.Success)
                {
                    storeResult = Result.Failure(
                        ErrorCode.DicomStoreFailed,
                        $"C-STORE failed with status: {response.Status}");
                }
            };

            var client = CreateClient(_options.PacsHost, _options.PacsPort, _options.LocalAeTitle, pacsAeTitle);
            await client.AddRequestAsync(request).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            if (storeResult.IsFailure)
                LogStoreWarning(_logger, pacsAeTitle, dicomFilePath);
            else
                LogStoreSuccess(_logger, pacsAeTitle, dicomFilePath);

            return storeResult;
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "C-STORE operation was cancelled.");
        }
        catch (IOException ex)
        {
            LogStoreIoError(_logger, ex, dicomFilePath);
            return Result.Failure(ErrorCode.DicomStoreFailed, $"I/O error: {ex.Message}");
        }
        catch (DicomNetworkException ex)
        {
            LogNetworkError(_logger, ex, "C-STORE", pacsAeTitle);
            return Result.Failure(ErrorCode.DicomConnectionFailed, $"Network error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<WorklistItem>>> QueryWorklistAsync(
        WorklistQuery query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query.AeTitle))
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed, "Worklist AE title must not be empty.");

        var results = new List<WorklistItem>();

        try
        {
            var request = BuildWorklistRequest(query);
            request.OnResponseReceived = (req, response) =>
            {
                if (response.Status == DicomStatus.Pending && response.Dataset != null)
                {
                    var item = MapToWorklistItem(response.Dataset);
                    results.Add(item);
                }
            };

            var client = CreateClient(_options.MwlHost, _options.MwlPort, _options.LocalAeTitle, query.AeTitle);
            await client.AddRequestAsync(request).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            LogQuerySuccess(_logger, query.AeTitle, results.Count);

            return Result.Success<IReadOnlyList<WorklistItem>>(results.AsReadOnly());
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.OperationCancelled, "Worklist query was cancelled.");
        }
        catch (DicomNetworkException ex)
        {
            LogNetworkError(_logger, ex, "C-FIND", query.AeTitle);
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomConnectionFailed, $"Network error: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<Result> PrintAsync(
        string dicomFilePath,
        string printerAeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dicomFilePath))
            return Result.Failure(ErrorCode.DicomPrintFailed, "DICOM file path must not be empty.");

        if (string.IsNullOrWhiteSpace(printerAeTitle))
            return Result.Failure(ErrorCode.DicomPrintFailed, "Printer AE title must not be empty.");

        if (!System.IO.File.Exists(dicomFilePath))
            return Result.Failure(ErrorCode.DicomPrintFailed, $"DICOM file not found: {dicomFilePath}");

        try
        {
            // N-CREATE: Basic Film Session
            var filmSessionUid = DicomUID.Generate();
            var filmSessionDataset = new DicomDataset
            {
                { DicomTag.NumberOfCopies, "1" },
                { DicomTag.PrintPriority, "MED" },
                { DicomTag.MediumType, "BLUE FILM" },
                { DicomTag.FilmDestination, "MAGAZINE" }
            };

            var createFilmSession = new DicomNCreateRequest(DicomUID.BasicFilmSession, filmSessionUid)
            {
                Dataset = filmSessionDataset
            };

            bool sessionCreated = false;
            string createFailMessage = string.Empty;
            createFilmSession.OnResponseReceived = (req, response) =>
            {
                if (response.Status == DicomStatus.Success)
                    sessionCreated = true;
                else
                    createFailMessage = $"N-CREATE Basic Film Session failed: {response.Status}";
            };

            var client = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
            await client.AddRequestAsync(createFilmSession).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            if (!sessionCreated)
            {
                LogPrintWarning(_logger, dicomFilePath, printerAeTitle, "N-CREATE");
                return Result.Failure(ErrorCode.DicomPrintFailed,
                    string.IsNullOrEmpty(createFailMessage) ? "N-CREATE did not succeed." : createFailMessage);
            }

            // N-ACTION: Print
            var actionRequest = new DicomNActionRequest(
                DicomUID.BasicFilmSession, filmSessionUid, 0x0001)
            {
                Dataset = new DicomDataset()
            };

            bool actionSucceeded = false;
            string actionFailMessage = string.Empty;
            actionRequest.OnResponseReceived = (req, response) =>
            {
                if (response.Status == DicomStatus.Success)
                    actionSucceeded = true;
                else
                    actionFailMessage = $"N-ACTION Print failed: {response.Status}";
            };

            var client2 = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
            await client2.AddRequestAsync(actionRequest).ConfigureAwait(false);
            await client2.SendAsync(cancellationToken).ConfigureAwait(false);

            if (!actionSucceeded)
            {
                LogPrintWarning(_logger, dicomFilePath, printerAeTitle, "N-ACTION");
                return Result.Failure(ErrorCode.DicomPrintFailed,
                    string.IsNullOrEmpty(actionFailMessage) ? "N-ACTION did not succeed." : actionFailMessage);
            }

            LogPrintSuccess(_logger, dicomFilePath, printerAeTitle);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "Print operation was cancelled.");
        }
        catch (IOException ex)
        {
            LogPrintIoError(_logger, ex, dicomFilePath);
            return Result.Failure(ErrorCode.DicomPrintFailed, $"I/O error: {ex.Message}");
        }
        catch (DicomNetworkException ex)
        {
            LogNetworkError(_logger, ex, "Print", printerAeTitle);
            return Result.Failure(ErrorCode.DicomConnectionFailed, $"Network error: {ex.Message}");
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private IDicomClient CreateClient(string host, int port, string callingAeTitle, string calledAeTitle)
    {
        return DicomClientFactory.Create(host, port, _options.TlsEnabled, callingAeTitle, calledAeTitle);
    }

    internal static DicomCFindRequest BuildWorklistRequest(WorklistQuery query)
    {
        // Build date range for scheduled procedure step start date
        DicomDateRange? dateRange = null;
        if (query.DateFrom.HasValue || query.DateTo.HasValue)
        {
            var from = query.DateFrom.HasValue
                ? DateTime.ParseExact(query.DateFrom.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    "yyyyMMdd", CultureInfo.InvariantCulture)
                : (DateTime?)null;
            var to = query.DateTo.HasValue
                ? DateTime.ParseExact(query.DateTo.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture),
                    "yyyyMMdd", CultureInfo.InvariantCulture)
                : (DateTime?)null;

            dateRange = (from, to) switch
            {
                ({ } f, { } t) => new DicomDateRange(f, t),
                ({ } f, null) => new DicomDateRange(f, DateTime.MaxValue),
                (null, { } t) => new DicomDateRange(DateTime.MinValue, t),
                _ => null
            };
        }

        return DicomCFindRequest.CreateWorklistQuery(
            patientId: query.PatientId,
            patientName: null,
            stationAE: null,
            stationName: null,
            modality: null,
            scheduledDateTime: dateRange);
    }

    internal static WorklistItem MapToWorklistItem(DicomDataset dataset)
    {
        var accessionNumber = dataset.GetSingleValueOrDefault(DicomTag.AccessionNumber, string.Empty);
        var patientId = dataset.GetSingleValueOrDefault(DicomTag.PatientID, string.Empty);
        var patientName = dataset.GetSingleValueOrDefault(DicomTag.PatientName, string.Empty);

        DateOnly? studyDate = null;
        var dateStr = dataset.GetSingleValueOrDefault(DicomTag.StudyDate, string.Empty);
        if (!string.IsNullOrEmpty(dateStr) && dateStr.Length == 8 &&
            int.TryParse(dateStr[..4], out var year) &&
            int.TryParse(dateStr[4..6], out var month) &&
            int.TryParse(dateStr[6..8], out var day))
        {
            studyDate = new DateOnly(year, month, day);
        }

        string? bodyPart = null;
        string? requestedProcedure = dataset.GetSingleValueOrDefault<string>(DicomTag.RequestedProcedureDescription, null!);
        if (string.IsNullOrEmpty(requestedProcedure))
            requestedProcedure = null;

        if (dataset.TryGetSequence(DicomTag.ScheduledProcedureStepSequence, out var spsSequence)
            && spsSequence.Items.Count > 0)
        {
            var spsItem = spsSequence.Items[0];
            var bp = spsItem.GetSingleValueOrDefault<string>(DicomTag.ScheduledProtocolCodeSequence, null!);
            bodyPart = string.IsNullOrEmpty(bp) ? null : bp;
        }

        return new WorklistItem(accessionNumber, patientId, patientName, studyDate, bodyPart, requestedProcedure);
    }

    // ── LoggerMessage definitions (CA1848 compliance) ─────────────────────────

    [LoggerMessage(Level = LogLevel.Warning, Message = "C-STORE to {AeTitle} failed for {FilePath}.")]
    private static partial void LogStoreWarning(ILogger logger, string aeTitle, string filePath);

    [LoggerMessage(Level = LogLevel.Information, Message = "C-STORE to {AeTitle} succeeded for {FilePath}.")]
    private static partial void LogStoreSuccess(ILogger logger, string aeTitle, string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "I/O error reading DICOM file {FilePath} for C-STORE.")]
    private static partial void LogStoreIoError(ILogger logger, IOException ex, string filePath);

    [LoggerMessage(Level = LogLevel.Error, Message = "DICOM network error during {Operation} to {AeTitle}.")]
    private static partial void LogNetworkError(ILogger logger, DicomNetworkException ex, string operation, string aeTitle);

    [LoggerMessage(Level = LogLevel.Information, Message = "Worklist C-FIND to {AeTitle} returned {Count} items.")]
    private static partial void LogQuerySuccess(ILogger logger, string aeTitle, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Print {Phase} failed for {FilePath} to {AeTitle}.")]
    private static partial void LogPrintWarning(ILogger logger, string filePath, string aeTitle, string phase);

    [LoggerMessage(Level = LogLevel.Information, Message = "Print succeeded for {FilePath} to {AeTitle}.")]
    private static partial void LogPrintSuccess(ILogger logger, string filePath, string aeTitle);

    [LoggerMessage(Level = LogLevel.Error, Message = "I/O error reading DICOM file {FilePath} for Print.")]
    private static partial void LogPrintIoError(ILogger logger, IOException ex, string filePath);
}
