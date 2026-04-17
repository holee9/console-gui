using System.IO;
using HnVue.Common.Results;

namespace HnVue.Dicom;

/// <summary>
/// Implements DICOM C-STORE SCU (Service Class User) for sending images to a PACS.
/// </summary>
/// <remarks>
/// Uses fo-dicom for DICOM protocol handling.
/// Production use: configure PacsHost and PacsPort from <see cref="IDicomNetworkConfig"/>.
/// IEC 62304 Class B — data transfer to clinical archive.
/// </remarks>
public sealed class DicomStoreScu
{
    private readonly IDicomNetworkConfig _config;

    /// <summary>
    /// Initialises a new <see cref="DicomStoreScu"/>.
    /// </summary>
    public DicomStoreScu(IDicomNetworkConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Sends the DICOM file at <paramref name="filePath"/> to the configured PACS via C-STORE.
    /// </summary>
    /// <param name="filePath">Absolute path to the DICOM file to transmit.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Success if the C-STORE completes with a success status;
    /// otherwise failure with <see cref="ErrorCode.DicomStoreFailed"/>.
    /// </returns>
    public async Task<Result> StoreAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (!File.Exists(filePath))
            return Result.Failure(ErrorCode.DicomStoreFailed,
                $"DICOM 파일을 찾을 수 없습니다: '{filePath}'.");

        const int maxRetries = 2; // DicomStoreScu uses fixed retry policy
        const int retryDelayMs = 1000;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Load DICOM file using fo-dicom
                var dcmFile = await FellowOakDicom.DicomFile.OpenAsync(filePath).ConfigureAwait(false);

                var client = FellowOakDicom.Network.Client.DicomClientFactory.Create(
                    _config.PacsHost,
                    _config.PacsPort,
                    false,
                    _config.LocalAeTitle,
                    _config.PacsAeTitle);

                // NegotiateAsyncOps() is a synchronous fo-dicom configuration call; no await needed. Issue #33.
                client.NegotiateAsyncOps();

                var storageRequest = new FellowOakDicom.Network.DicomCStoreRequest(dcmFile);

                // Add response handler for better error messages
                var requestResult = Result.Success();
                storageRequest.OnResponseReceived = (req, response) =>
                {
                    if (response.Status != FellowOakDicom.DicomStatus.Success)
                    {
                        requestResult = Result.Failure(
                            ErrorCode.DicomStoreFailed,
                            $"C-STORE 실패 ({_config.PacsAeTitle}): {GetUserFriendlyStatus(response.Status)}");
                    }
                };

                await client.AddRequestAsync(storageRequest).ConfigureAwait(false);
                await client.SendAsync(cancellationToken).ConfigureAwait(false);

                if (requestResult.IsFailure)
                {
                    if (attempt < maxRetries && IsTransientNetworkError(requestResult.ErrorMessage))
                    {
                        await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    return requestResult;
                }

                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (System.IO.IOException ex)
            {
                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                return Result.Failure(ErrorCode.DicomStoreFailed,
                    $"파일 읽기 오류: {ex.Message}");
            }
            catch (FellowOakDicom.Network.DicomNetworkException ex)
            {
                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                return Result.Failure(ErrorCode.DicomStoreFailed,
                    $"PACS 연결 실패 ({_config.PacsAeTitle}): 네트워크 오류가 발생했습니다. PACS 서버 상태를 확인해주세요.");
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
            {
                var baseException = ex.GetBaseException();
                if (baseException is System.Net.Sockets.SocketException socketException)
                {
                    if (attempt < maxRetries)
                    {
                        await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    return Result.Failure(ErrorCode.DicomStoreFailed,
                        $"PACS 연결 실패 ({_config.PacsAeTitle}): 서버에 연결할 수 없습니다. 네트워크 연결과 PACS 서버 구동을 확인해주세요.");
                }

                if (attempt < maxRetries)
                {
                    await Task.Delay(retryDelayMs, cancellationToken).ConfigureAwait(false);
                    continue;
                }
                return Result.Failure(ErrorCode.DicomStoreFailed,
                    $"C-STORE 실패: {baseException.Message}");
            }
        }

        return Result.Failure(ErrorCode.DicomStoreFailed,
            $"C-STORE 실패: {maxRetries}회 재시도 후에도 성공하지 못했습니다.");
    }

    /// <summary>
    /// Converts DICOM status codes to user-friendly Korean error messages.
    /// </summary>
    private static string GetUserFriendlyStatus(FellowOakDicom.DicomStatus status)
    {
        var code = status.Code;
        var description = status.Description;

        return code switch
        {
            0x0000 => "성공",
            0xA700 => $"일시적 오류: {description}",
            0xA900 => $"데이터 오류: {description}",
            0xAA00 => $"일시적 오류: {description}",
            0xC000 => $"오류: {description}",
            0xFE00 => $"일시적 오류: {description}",
            _ => $"상태 코드 0x{code:X4}: {description}"
        };
    }

    /// <summary>
    /// Determines if an error message indicates a transient network failure.
    /// </summary>
    private static bool IsTransientNetworkError(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return false;

        return errorMessage.Contains("timeout", System.StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("network", System.StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("일시적", System.StringComparison.OrdinalIgnoreCase);
    }
}
