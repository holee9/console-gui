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
                $"DICOM file not found: '{filePath}'.");

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

            client.NegotiateAsyncOps();

            var storageRequest = new FellowOakDicom.Network.DicomCStoreRequest(dcmFile);
            await client.AddRequestAsync(storageRequest).ConfigureAwait(false);

            await client.SendAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.DicomStoreFailed,
                $"C-STORE to '{_config.PacsAeTitle}' failed: {ex.Message}");
        }
    }
}
