using System.IO;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Provides TLS 1.3 secure network connections for DICOM and device communication.
/// Implements WBS 5.1.5 network encryption baseline.
/// </summary>
public interface ITlsConnectionService
{
    /// <summary>
    /// Establishes an authenticated TLS connection to the specified endpoint.
    /// </summary>
    /// <param name="host">Remote hostname or IP address.</param>
    /// <param name="port">Remote port number.</param>
    /// <param name="cancellationToken">Token to cancel the connection attempt.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the authenticated <see cref="Stream"/>,
    /// or a failure with <see cref="ErrorCode.TlsConnectionFailed"/>.</returns>
    Task<Result<Stream>> ConnectAsync(string host, int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a server certificate against the configured trust store.
    /// </summary>
    /// <param name="certificateBytes">DER-encoded certificate to validate.</param>
    /// <param name="expectedHost">Expected hostname for SAN/CN matching.</param>
    /// <returns>Success if the certificate is valid and trusted; failure otherwise.</returns>
    Result ValidateCertificate(byte[] certificateBytes, string expectedHost);
}
