using System.Globalization;
using System.IO;
using FellowOakDicom;
using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HnVue.Dicom;

/// <summary>
/// Implements <see cref="HnVue.Common.Abstractions.IDicomService"/> using the fo-dicom 5.x async client API.
/// Provides C-STORE SCU, Modality Worklist C-FIND SCU, and DICOM Print SCU operations.
/// </summary>
public partial class DicomService : HnVue.Common.Abstractions.IDicomService
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
    // @MX:ANCHOR StoreAsync - @MX:REASON: C-STORE SCU for sending images to PACS, called by DicomOutbox and directly, implements fo-dicom async client pattern with OnResponseReceived callback
    public async Task<Result> StoreAsync(
        string dicomFilePath,
        string pacsAeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dicomFilePath))
            return Result.Failure(ErrorCode.DicomStoreFailed, "DICOM 파일 경로가 비어있습니다.");

        if (string.IsNullOrWhiteSpace(pacsAeTitle))
            return Result.Failure(ErrorCode.DicomStoreFailed, "PACS AE Title이 비어있습니다.");

        if (!System.IO.File.Exists(dicomFilePath))
            return Result.Failure(ErrorCode.DicomStoreFailed, $"DICOM 파일을 찾을 수 없습니다: {dicomFilePath}");

        int maxRetries = _options.StoreRetryCount;
        int retryDelayMs = _options.StoreRetryDelayMs;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                var dicomFile = await DicomFile.OpenAsync(dicomFilePath).ConfigureAwait(false);

                var storeResult = Result.Success();
                var request = new DicomCStoreRequest(dicomFile);
                // @MX:NOTE OnResponseReceived callback handles DICOM association response, Success status indicates PACS accepted the C-STORE
                request.OnResponseReceived = (req, response) =>
                {
                    if (response.Status != DicomStatus.Success)
                    {
                        storeResult = Result.Failure(
                            ErrorCode.DicomStoreFailed,
                            $"C-STORE 실패: {GetUserFriendlyStatus(response.Status)}");
                    }
                };

                var client = CreateClient(_options.PacsHost, _options.PacsPort, _options.LocalAeTitle, pacsAeTitle);
                await client.AddRequestAsync(request).ConfigureAwait(false);
                await client.SendAsync(cancellationToken).ConfigureAwait(false);

                if (storeResult.IsFailure)
                {
                    LogStoreWarning(_logger, pacsAeTitle, dicomFilePath);

                    // Check if this is a transient error that should be retried
                    if (attempt < maxRetries && IsTransientError(storeResult.ErrorMessage))
                    {
                        _logger.LogWarning("C-STORE transient error detected, retrying ({Attempt}/{MaxRetries}) after {Delay}ms...",
                            attempt + 1, maxRetries, retryDelayMs);
                        await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                }
                else
                {
                    LogStoreSuccess(_logger, pacsAeTitle, dicomFilePath);
                }

                return storeResult;
            }
            catch (OperationCanceledException)
            {
                return Result.Failure(ErrorCode.OperationCancelled, "C-STORE 작업이 취소되었습니다.");
            }
            catch (IOException ex)
            {
                LogStoreIoError(_logger, ex, dicomFilePath);

                if (attempt < maxRetries)
                {
                    _logger.LogWarning("C-STORE I/O error detected, retrying ({Attempt}/{MaxRetries}) after {Delay}ms...",
                        attempt + 1, maxRetries, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                return Result.Failure(ErrorCode.DicomStoreFailed, $"파일 읽기 오류: {ex.Message}");
            }
            catch (DicomNetworkException ex)
            {
                LogNetworkError(_logger, ex, "C-STORE", pacsAeTitle);

                if (attempt < maxRetries)
                {
                    _logger.LogWarning("C-STORE network error detected, retrying ({Attempt}/{MaxRetries}) after {Delay}ms...",
                        attempt + 1, maxRetries, retryDelayMs);
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                return Result.Failure(ErrorCode.DicomConnectionFailed, $"PACS 연결 실패 ({pacsAeTitle}): 네트워크 오류가 발생했습니다. PACS 서버 상태를 확인해주세요.");
            }
            catch (Exception ex) when (ex is not OperationCanceledException
                                        and not IOException
                                        and not DicomNetworkException
                                        and not OutOfMemoryException)
            {
                var baseException = ex.GetBaseException();
                if (baseException is System.Net.Sockets.SocketException socketException)
                {
                    LogConnectionError(_logger, socketException, pacsAeTitle);

                    if (attempt < maxRetries)
                    {
                        _logger.LogWarning("C-STORE socket error detected, retrying ({Attempt}/{MaxRetries}) after {Delay}ms...",
                            attempt + 1, maxRetries, retryDelayMs);
                        await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }

                    return Result.Failure(ErrorCode.DicomConnectionFailed, $"PACS 연결 실패 ({pacsAeTitle}): 서버에 연결할 수 없습니다. 네트워크 연결과 PACS 서버 구동을 확인해주세요.");
                }

                _logger.LogError(ex, "Unexpected C-STORE failure for AE {AeTitle}.", pacsAeTitle);
                return Result.Failure(ErrorCode.DicomStoreFailed, $"C-STORE 실패: {baseException.Message}");
            }
        }

        // All retries exhausted
        return Result.Failure(ErrorCode.DicomStoreFailed, $"C-STORE 실패: {maxRetries}회 재시도 후에도 성공하지 못했습니다.");
    }

    /// <inheritdoc/>
    // @MX:NOTE QueryWorklistAsync builds CFindRequest with date range filter, collects Pending responses via callback, returns read-only list
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
            // @MX:NOTE OnResponseReceived callback collects Pending responses (C-FIND pending matches), Success status indicates final response
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
        catch (System.Net.Sockets.SocketException ex)
        {
            LogConnectionError(_logger, ex, query.AeTitle);
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomConnectionFailed, $"Connection failed: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException
                                    and not DicomNetworkException
                                    and not System.Net.Sockets.SocketException)
        {
            LogUnexpectedQueryError(_logger, ex, query.AeTitle);
            return Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomConnectionFailed, $"Query failed: {ex.Message}");
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
            // Step 1: N-CREATE Basic Film Session
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

            // Step 2: N-CREATE Basic Film Box
            var filmBoxResult = await CreateFilmBoxAsync(
                filmSessionUid, printerAeTitle, cancellationToken).ConfigureAwait(false);

            if (filmBoxResult.IsFailure)
            {
                LogPrintWarning(_logger, dicomFilePath, printerAeTitle, "N-CREATE FilmBox");
                return Result.Failure(filmBoxResult.Error ?? ErrorCode.DicomPrintFailed,
                    filmBoxResult.ErrorMessage ?? "Film Box creation failed.");
            }

            var filmBoxUid = filmBoxResult.Value;

            // Step 3: N-SET Film Box (configure image position, resolution, magnification)
            var setResult = await SetFilmBoxAsync(
                filmBoxUid, printerAeTitle, cancellationToken).ConfigureAwait(false);

            if (setResult.IsFailure)
            {
                LogPrintWarning(_logger, dicomFilePath, printerAeTitle, "N-SET FilmBox");
                return setResult;
            }

            // Step 4: N-ACTION Print
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

            var client3 = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
            await client3.AddRequestAsync(actionRequest).ConfigureAwait(false);
            await client3.SendAsync(cancellationToken).ConfigureAwait(false);

            if (!actionSucceeded)
            {
                LogPrintWarning(_logger, dicomFilePath, printerAeTitle, "N-ACTION");
                return Result.Failure(ErrorCode.DicomPrintFailed,
                    string.IsNullOrEmpty(actionFailMessage) ? "N-ACTION did not succeed." : actionFailMessage);
            }

            // Step 5: N-GET Status Poll
            var statusResult = await GetPrintJobStatusAsync(
                filmSessionUid.UID, printerAeTitle, cancellationToken).ConfigureAwait(false);

            if (statusResult.IsFailure)
            {
                // Print action itself succeeded; status poll failure is non-fatal
                _logger.LogWarning("Print job status poll failed for session {SessionUid}: {Error}",
                    filmSessionUid, statusResult.ErrorMessage);
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
        catch (Exception ex) when (ex is not OperationCanceledException
                                    and not IOException
                                    and not DicomNetworkException
                                    and not OutOfMemoryException)
        {
            var baseException = ex.GetBaseException();
            if (baseException is System.Net.Sockets.SocketException socketException)
            {
                LogConnectionError(_logger, socketException, printerAeTitle);
                return Result.Failure(ErrorCode.DicomConnectionFailed, $"Connection failed: {socketException.Message}");
            }

            _logger.LogError(ex, "Unexpected DICOM print failure for AE {AeTitle}.", printerAeTitle);
            return Result.Failure(ErrorCode.DicomPrintFailed, $"Print failed: {baseException.Message}");
        }
    }

    /// <summary>
    /// Creates a Basic Film Box SOP instance via N-CREATE, associating it with the given Film Session.
    /// DICOM PS3.4 Print Management: Basic Film Box belongs to the Film Session.
    /// </summary>
    /// <param name="filmSessionUid">SOP Instance UID of the parent Basic Film Session.</param>
    /// <param name="printerAeTitle">Called AE title of the DICOM printer SCP.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The generated Film Box SOP Instance UID on success, or a failure result.</returns>
    internal async Task<Result<DicomUID>> CreateFilmBoxAsync(
        DicomUID filmSessionUid,
        string printerAeTitle,
        CancellationToken cancellationToken)
    {
        var filmBoxUid = DicomUID.Generate();

        var filmBoxDataset = new DicomDataset
        {
            { DicomTag.ImageDisplayFormat, "STANDARD\\1,1" },
            { DicomTag.FilmOrientation, "PORTRAIT" },
            { DicomTag.FilmSizeID, "14IN X 17IN" },
            { DicomTag.MagnificationType, "BILINEAR" },
            { DicomTag.SmoothingType, "MEDIUM" },
            { DicomTag.BorderDensity, "BLACK" },
            { DicomTag.EmptyImageDensity, "BLACK" },
        };

        // Reference the parent Film Session
        filmBoxDataset.Add(new DicomSequence(DicomTag.ReferencedFilmSessionSequence,
            new DicomDataset
            {
                { DicomTag.ReferencedSOPClassUID, DicomUID.BasicFilmSession.UID },
                { DicomTag.ReferencedSOPInstanceUID, filmSessionUid.UID },
            }));

        var createRequest = new DicomNCreateRequest(DicomUID.BasicFilmBox, filmBoxUid)
        {
            Dataset = filmBoxDataset
        };

        bool boxCreated = false;
        string failMessage = string.Empty;
        createRequest.OnResponseReceived = (req, response) =>
        {
            if (response.Status == DicomStatus.Success)
                boxCreated = true;
            else
                failMessage = $"N-CREATE Basic Film Box failed: {response.Status}";
        };

        var client = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
        await client.AddRequestAsync(createRequest).ConfigureAwait(false);
        await client.SendAsync(cancellationToken).ConfigureAwait(false);

        if (!boxCreated)
            return Result.Failure<DicomUID>(ErrorCode.DicomPrintFailed,
                string.IsNullOrEmpty(failMessage) ? "Film Box creation did not succeed." : failMessage);

        LogFilmBoxCreated(_logger, filmBoxUid.UID);
        return Result.Success(filmBoxUid);
    }

    /// <summary>
    /// Configures an existing Basic Film Box via N-SET, setting image position, requested resolution,
    /// and magnification type for each image box in the film layout.
    /// </summary>
    /// <param name="filmBoxUid">SOP Instance UID of the Basic Film Box to configure.</param>
    /// <param name="printerAeTitle">Called AE title of the DICOM printer SCP.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.DicomPrintFailed"/>.</returns>
    internal async Task<Result> SetFilmBoxAsync(
        DicomUID filmBoxUid,
        string printerAeTitle,
        CancellationToken cancellationToken)
    {
        var setDataset = new DicomDataset
        {
            { DicomTag.RequestedResolutionID, "200" },
            { DicomTag.MagnificationType, "BILINEAR" },
            { DicomTag.SmoothingType, "MEDIUM" },
        };

        // Image Box Position (2020,0010) — specified per DICOM PS3.3 C.13.5.2
        var imageBoxPositionTag = new DicomTag(0x2020, 0x0010);
        setDataset.Add(imageBoxPositionTag, "1");

        // Polarity (2020,0020) — specified per DICOM PS3.3 C.13.4.1
        var polarityTag = new DicomTag(0x2020, 0x0020);
        setDataset.Add(polarityTag, "NORMAL");

        var setRequest = new DicomNSetRequest(DicomUID.BasicFilmBox, filmBoxUid)
        {
            Dataset = setDataset
        };

        bool setSucceeded = false;
        string failMessage = string.Empty;
        setRequest.OnResponseReceived = (req, response) =>
        {
            if (response.Status == DicomStatus.Success)
                setSucceeded = true;
            else
                failMessage = $"N-SET Basic Film Box failed: {response.Status}";
        };

        var client = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
        await client.AddRequestAsync(setRequest).ConfigureAwait(false);
        await client.SendAsync(cancellationToken).ConfigureAwait(false);

        if (!setSucceeded)
            return Result.Failure(ErrorCode.DicomPrintFailed,
                string.IsNullOrEmpty(failMessage) ? "N-SET Film Box did not succeed." : failMessage);

        LogFilmBoxSet(_logger, filmBoxUid.UID);
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<PrintJobStatus>> GetPrintJobStatusAsync(
        string filmSessionUid,
        string printerAeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filmSessionUid))
            return Result.Failure<PrintJobStatus>(ErrorCode.DicomPrintFailed,
                "Film Session UID must not be empty.");

        if (string.IsNullOrWhiteSpace(printerAeTitle))
            return Result.Failure<PrintJobStatus>(ErrorCode.DicomPrintFailed,
                "Printer AE title must not be empty.");

        const int maxPollAttempts = 10;
        const int pollIntervalMs = 1000;

        for (int attempt = 0; attempt < maxPollAttempts; attempt++)
        {
            if (cancellationToken.IsCancellationRequested)
                return Result.Failure<PrintJobStatus>(ErrorCode.OperationCancelled,
                    "Print job status poll was cancelled.");

            try
            {
                var sopInstanceUid = new DicomUID(filmSessionUid, filmSessionUid, DicomUidType.SOPInstance);

                // Request the Execution Status attribute from the Print Job
                var requestedAttributes = new DicomDataset
                {
                    { DicomTag.ExecutionStatus, string.Empty },
                };

                var getRequest = new DicomNGetRequest(DicomUID.PrintJob, sopInstanceUid)
                {
                    Dataset = requestedAttributes
                };

                DicomDataset? responseDataset = null;
                bool getCompleted = false;
                string getFailMessage = string.Empty;
                getRequest.OnResponseReceived = (req, response) =>
                {
                    if (response.Status == DicomStatus.Success)
                    {
                        responseDataset = response.Dataset;
                        getCompleted = true;
                    }
                    else
                    {
                        getFailMessage = $"N-GET Print Job status failed: {response.Status}";
                    }
                };

                var client = CreateClient(_options.PrinterHost, _options.PrinterPort, _options.LocalAeTitle, printerAeTitle);
                await client.AddRequestAsync(getRequest).ConfigureAwait(false);
                await client.SendAsync(cancellationToken).ConfigureAwait(false);

                if (!getCompleted)
                {
                    LogPrintStatusPollWarning(_logger, filmSessionUid, attempt + 1);
                    return Result.Failure<PrintJobStatus>(ErrorCode.DicomPrintFailed,
                        string.IsNullOrEmpty(getFailMessage) ? "N-GET Print Job did not succeed." : getFailMessage);
                }

                var status = MapExecutionStatus(responseDataset);

                if (status == PrintJobStatus.Done || status == PrintJobStatus.Failure)
                {
                    LogPrintStatusComplete(_logger, filmSessionUid, status.ToString());
                    return Result.Success(status);
                }

                // Status is Pending or Printing — wait and poll again
                _logger.LogDebug("Print job {SessionUid} status={Status}, polling ({Attempt}/{Max})...",
                    filmSessionUid, status, attempt + 1, maxPollAttempts);

                await Task.Delay(pollIntervalMs, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                return Result.Failure<PrintJobStatus>(ErrorCode.OperationCancelled,
                    "Print job status poll was cancelled.");
            }
            catch (DicomNetworkException ex)
            {
                LogNetworkError(_logger, ex, "N-GET PrintJob", printerAeTitle);
                return Result.Failure<PrintJobStatus>(ErrorCode.DicomConnectionFailed,
                    $"Network error during status poll: {ex.Message}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException
                                        and not DicomNetworkException
                                        and not OutOfMemoryException)
            {
                _logger.LogError(ex, "Unexpected error during print job status poll for session {SessionUid}.",
                    filmSessionUid);
                return Result.Failure<PrintJobStatus>(ErrorCode.DicomPrintFailed,
                    $"Status poll failed: {ex.Message}");
            }
        }

        // Polling exhausted — return Pending as the last known state
        _logger.LogWarning("Print job status poll exhausted for session {SessionUid} after {Attempts} attempts.",
            filmSessionUid, maxPollAttempts);
        return Result.Success(PrintJobStatus.Pending);
    }

    /// <summary>
    /// Maps the Execution Status attribute from the N-GET response dataset to <see cref="PrintJobStatus"/>.
    /// </summary>
    private static PrintJobStatus MapExecutionStatus(DicomDataset? dataset)
    {
        if (dataset is null)
            return PrintJobStatus.Pending;

        var status = dataset.GetSingleValueOrDefault(DicomTag.ExecutionStatus, string.Empty);

        return status.ToUpperInvariant() switch
        {
            "PENDING" => PrintJobStatus.Pending,
            "PRINTING" => PrintJobStatus.Printing,
            "DONE" => PrintJobStatus.Done,
            "FAILURE" => PrintJobStatus.Failure,
            _ => PrintJobStatus.Pending
        };
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-DC-057: Sends N-ACTION (Action Type 1) to request Storage Commitment.
    /// SWR-DC-058: Awaits N-EVENT-REPORT on inbound association (simplified: uses N-ACTION response status).
    /// Note: Full N-EVENT-REPORT reception requires a listener SCP, which is out of scope for Phase 1.
    /// Issue #23.
    /// </remarks>
    public async Task<Result> RequestStorageCommitmentAsync(
        string sopClassUid,
        string sopInstanceUid,
        string pacsAeTitle,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_options.PacsHost))
            return Result.Failure(ErrorCode.DicomConnectionFailed,
                "PACS host is not configured for Storage Commitment.");

        ArgumentNullException.ThrowIfNull(sopClassUid);
        ArgumentNullException.ThrowIfNull(sopInstanceUid);
        ArgumentNullException.ThrowIfNull(pacsAeTitle);

        var transactionUid = DicomUID.Generate();
        var commitDataset = new DicomDataset
        {
            { DicomTag.TransactionUID, transactionUid },
        };

        // Add the referenced SOP instance
        var refSequence = new DicomSequence(DicomTag.ReferencedSOPSequence,
            new DicomDataset
            {
                { DicomTag.ReferencedSOPClassUID, sopClassUid },
                { DicomTag.ReferencedSOPInstanceUID, sopInstanceUid },
            });
        commitDataset.Add(refSequence);

        string? errorMessage = null;

        var actionRequest = new DicomNActionRequest(
            DicomUID.StorageCommitmentPushModel,
            DicomUID.StorageCommitmentPushModel,
            actionTypeId: 1)
        {
            Dataset = commitDataset
        };
        actionRequest.OnResponseReceived = (_, response) =>
        {
            if (response.Status != DicomStatus.Success)
                errorMessage = $"Storage Commitment N-ACTION failed: {response.Status}";
        };

        try
        {
            var client = CreateClient(_options.PacsHost, _options.PacsPort, _options.LocalAeTitle, pacsAeTitle);
            await client.AddRequestAsync(actionRequest).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            return errorMessage is not null
                ? Result.Failure(ErrorCode.DicomStoreFailed, errorMessage)
                : Result.Success();
        }
        catch (DicomNetworkException ex)
        {
            return Result.Failure(ErrorCode.DicomConnectionFailed,
                $"Storage Commitment network error: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "Storage Commitment was cancelled.");
        }
        catch (Exception ex) when (ex is not DicomNetworkException
                                    and not OperationCanceledException
                                    and not OutOfMemoryException)
        {
            var baseException = ex.GetBaseException();
            if (baseException is System.Net.Sockets.SocketException)
            {
                return Result.Failure(ErrorCode.DicomConnectionFailed,
                    $"Storage Commitment connection failed: {baseException.Message}");
            }

            _logger.LogError(ex, "Unexpected Storage Commitment failure for AE {AeTitle}.", pacsAeTitle);
            return Result.Failure(ErrorCode.DicomStoreFailed,
                $"Storage Commitment failed: {baseException.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// SWR-DC-060~065: Generates RDSR from dose record using <see cref="RdsrBuilder"/>,
    /// wraps it in a DicomCStoreRequest, and sends via the existing C-STORE flow.
    /// </remarks>
    public async Task<Result> SendRdsrAsync(
        DoseRecord doseRecord,
        RdsrPatientInfo patientInfo,
        RdsrStudyInfo studyInfo,
        string pacsAeTitle,
        RdsrExposureParams? exposureParams = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(doseRecord);
        ArgumentNullException.ThrowIfNull(patientInfo);
        ArgumentNullException.ThrowIfNull(studyInfo);

        if (string.IsNullOrWhiteSpace(pacsAeTitle))
            return Result.Failure(ErrorCode.DicomStoreFailed, "PACS AE Title must not be empty.");

        try
        {
            // Build the RDSR DICOM dataset
            var builder = new RdsrBuilder(doseRecord, patientInfo, studyInfo, exposureParams);
            var rdsrDataset = builder.Build();

            // Wrap in C-STORE request
            var storeResult = Result.Success();
            var request = new DicomCStoreRequest(rdsrDataset);
            request.OnResponseReceived = (_, response) =>
            {
                if (response.Status != DicomStatus.Success)
                {
                    storeResult = Result.Failure(
                        ErrorCode.DicomStoreFailed,
                        $"RDSR C-STORE failed: {GetUserFriendlyStatus(response.Status)}");
                }
            };

            var client = CreateClient(_options.PacsHost, _options.PacsPort, _options.LocalAeTitle, pacsAeTitle);
            await client.AddRequestAsync(request).ConfigureAwait(false);
            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            if (storeResult.IsSuccess)
            {
                LogRdsrSuccess(_logger, pacsAeTitle, doseRecord.StudyInstanceUid);
            }
            else
            {
                LogRdsrWarning(_logger, pacsAeTitle, doseRecord.StudyInstanceUid);
            }

            return storeResult;
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(ErrorCode.OperationCancelled, "RDSR C-STORE was cancelled.");
        }
        catch (DicomNetworkException ex)
        {
            LogNetworkError(_logger, ex, "RDSR C-STORE", pacsAeTitle);
            return Result.Failure(ErrorCode.DicomConnectionFailed,
                $"RDSR network error: {ex.Message}");
        }
        catch (Exception ex) when (ex is not OperationCanceledException
                                    and not OutOfMemoryException)
        {
            var baseException = ex.GetBaseException();
            if (baseException is System.Net.Sockets.SocketException socketException)
            {
                LogConnectionError(_logger, socketException, pacsAeTitle);
                return Result.Failure(ErrorCode.DicomConnectionFailed,
                    $"RDSR connection failed: {socketException.Message}");
            }

            _logger.LogError(ex, "Unexpected RDSR transmission failure for AE {AeTitle}, study {StudyUid}.",
                pacsAeTitle, doseRecord.StudyInstanceUid);
            return Result.Failure(ErrorCode.DicomStoreFailed,
                $"RDSR transmission failed: {baseException.Message}");
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    internal virtual IDicomClient CreateClient(string host, int port, string callingAeTitle, string calledAeTitle)
    {
        return DicomClientFactory.Create(host, port, _options.TlsEnabled, callingAeTitle, calledAeTitle);
    }

    /// <summary>
    /// Converts DICOM status codes to user-friendly Korean error messages.
    /// </summary>
    private static string GetUserFriendlyStatus(DicomStatus status)
    {
        var code = status.Code;
        var description = status.Description;

        // Common DICOM status codes with user-friendly messages
        return code switch
        {
            0x0000 => "성공", // Success
            0xA700 => $"일시적 오류: {description}", // Refused: Out of resources
            0xA900 => $"데이터 오류: {description}", // Error: Data set does not match SOP class
            0xAA00 => $"일시적 오류: {description}", // Refused: NP association timeout
            0xC000 => $"오류: {description}", // Error: Cannot understand
            0xC112 => $"오류: {description}", // Error: SOP class not supported
            0xC123 => $"오류: {description}", // Error: SOP instance not recognized
            0xC200 => $"호환되지 않음: {description}", // Error: No such SOP class
            0xC301 => $"리소스 부족: {description}", // Warning: Not enough memory
            0xFE00 => $"일시적 오류: {description}", // Warning: Coercion of data elements
            _ => $"상태 코드 0x{code:X4}: {description}"
        };
    }

    /// <summary>
    /// Determines if an error message indicates a transient failure that should be retried.
    /// </summary>
    private static bool IsTransientError(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return false;

        // Transient error indicators
        return errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("temporary", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("일시적", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("0xA700", StringComparison.OrdinalIgnoreCase) // Out of resources
            || errorMessage.Contains("0xAA00", StringComparison.OrdinalIgnoreCase) // Association timeout
            || errorMessage.Contains("0xFE00", StringComparison.OrdinalIgnoreCase); // Coercion warning
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
        if (DateOnly.TryParseExact(
                dateStr,
                "yyyyMMdd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var parsedStudyDate))
        {
            studyDate = parsedStudyDate;
        }

        string? bodyPart = null;
        var requestedProcedure = GetOptionalString(dataset, DicomTag.RequestedProcedureDescription);

        if (dataset.TryGetSequence(DicomTag.ScheduledProcedureStepSequence, out var spsSequence)
            && spsSequence.Items.Count > 0)
        {
            var spsItem = spsSequence.Items[0];
            bodyPart =
                GetOptionalString(spsItem, DicomTag.BodyPartExamined)
                ?? GetOptionalString(spsItem, DicomTag.ScheduledProcedureStepDescription);

            if (bodyPart is null
                && spsItem.TryGetSequence(DicomTag.ScheduledProtocolCodeSequence, out var protocolSequence)
                && protocolSequence.Items.Count > 0)
            {
                var protocolItem = protocolSequence.Items[0];
                bodyPart =
                    GetOptionalString(protocolItem, DicomTag.CodeMeaning)
                    ?? GetOptionalString(protocolItem, DicomTag.CodeValue);
            }
        }

        return new WorklistItem(accessionNumber, patientId, patientName, studyDate, bodyPart, requestedProcedure);
    }

    private static string? GetOptionalString(DicomDataset dataset, DicomTag tag)
    {
        var value = dataset.GetSingleValueOrDefault<string>(tag, null!);
        return string.IsNullOrWhiteSpace(value) ? null : value;
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

    [LoggerMessage(Level = LogLevel.Error, Message = "DICOM socket connection failed for AE {AeTitle}.")]
    private static partial void LogConnectionError(ILogger logger, System.Net.Sockets.SocketException ex, string aeTitle);

    [LoggerMessage(Level = LogLevel.Error, Message = "DICOM worklist query failed unexpectedly for AE {AeTitle}.")]
    private static partial void LogUnexpectedQueryError(ILogger logger, Exception ex, string aeTitle);

    [LoggerMessage(Level = LogLevel.Information, Message = "Worklist C-FIND to {AeTitle} returned {Count} items.")]
    private static partial void LogQuerySuccess(ILogger logger, string aeTitle, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Print {Phase} failed for {FilePath} to {AeTitle}.")]
    private static partial void LogPrintWarning(ILogger logger, string filePath, string aeTitle, string phase);

    [LoggerMessage(Level = LogLevel.Information, Message = "Print succeeded for {FilePath} to {AeTitle}.")]
    private static partial void LogPrintSuccess(ILogger logger, string filePath, string aeTitle);

    [LoggerMessage(Level = LogLevel.Error, Message = "I/O error reading DICOM file {FilePath} for Print.")]
    private static partial void LogPrintIoError(ILogger logger, IOException ex, string filePath);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Basic Film Box created with UID {FilmBoxUid}.")]
    private static partial void LogFilmBoxCreated(ILogger logger, string filmBoxUid);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Basic Film Box N-SET succeeded for UID {FilmBoxUid}.")]
    private static partial void LogFilmBoxSet(ILogger logger, string filmBoxUid);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Print job status poll failed for session {SessionUid}, attempt {Attempt}.")]
    private static partial void LogPrintStatusPollWarning(ILogger logger, string sessionUid, int attempt);

    [LoggerMessage(Level = LogLevel.Information, Message = "Print job {SessionUid} completed with status {Status}.")]
    private static partial void LogPrintStatusComplete(ILogger logger, string sessionUid, string status);

    [LoggerMessage(Level = LogLevel.Information, Message = "RDSR C-STORE to {AeTitle} succeeded for study {StudyUid}.")]
    private static partial void LogRdsrSuccess(ILogger logger, string aeTitle, string studyUid);

    [LoggerMessage(Level = LogLevel.Warning, Message = "RDSR C-STORE to {AeTitle} failed for study {StudyUid}.")]
    private static partial void LogRdsrWarning(ILogger logger, string aeTitle, string studyUid);
}
