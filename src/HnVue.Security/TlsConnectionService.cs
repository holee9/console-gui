using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using HnVue.Common.Abstractions;
using HnVue.Common.Results;

namespace HnVue.Security;

/// <summary>
/// TLS 1.3 connection service using SslStream.
/// Implements WBS 5.1.5 network encryption baseline for DICOM and device communication.
/// </summary>
public sealed class TlsConnectionService : ITlsConnectionService
{
    private readonly StoreName _certificateStoreName;
    private readonly StoreLocation _certificateStoreLocation;

    /// <summary>
    /// Creates a TLS connection service with default certificate store (Root/CurrentUser).
    /// </summary>
    public TlsConnectionService()
        : this(StoreName.Root, StoreLocation.CurrentUser) { }

    /// <summary>
    /// Creates a TLS connection service with a specific certificate store.
    /// </summary>
    public TlsConnectionService(StoreName storeName, StoreLocation storeLocation)
    {
        _certificateStoreName = storeName;
        _certificateStoreLocation = storeLocation;
    }

    /// <inheritdoc/>
    public async Task<Result<Stream>> ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        if (port is < 1 or > 65535)
            return Result.Failure<Stream>(ErrorCode.TlsConnectionFailed, "Port must be between 1 and 65535.");

        try
        {
            var tcpClient = new System.Net.Sockets.TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken).ConfigureAwait(false);

            var sslStream = new SslStream(
                tcpClient.GetStream(),
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: (sender, certificate, chain, sslPolicyErrors) =>
                    ValidateCertificateInternal(certificate, chain, sslPolicyErrors, host));

            var clientCertificates = LoadClientCertificates();
            await sslStream.AuthenticateAsClientAsync(
                targetHost: host,
                clientCertificates: clientCertificates,
                enabledSslProtocols: System.Security.Authentication.SslProtocols.Tls13,
                checkCertificateRevocation: true).ConfigureAwait(false);

            if (sslStream.SslProtocol != System.Security.Authentication.SslProtocols.Tls13)
                return Result.Failure<Stream>(ErrorCode.TlsConnectionFailed,
                    $"TLS 1.3 required but negotiated {sslStream.SslProtocol}.");

            return Result.Success<Stream>(sslStream);
        }
        catch (System.Security.Authentication.AuthenticationException ex)
        {
            return Result.Failure<Stream>(ErrorCode.TlsConnectionFailed, $"TLS authentication failed: {ex.Message}");
        }
        catch (System.Net.Sockets.SocketException ex)
        {
            return Result.Failure<Stream>(ErrorCode.TlsConnectionFailed, $"Connection failed: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            return Result.Failure<Stream>(ErrorCode.TlsConnectionFailed, "TLS connection timed out.");
        }
    }

    /// <inheritdoc/>
    public Result ValidateCertificate(byte[] certificateBytes, string expectedHost)
    {
        ArgumentNullException.ThrowIfNull(certificateBytes);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedHost);

        try
        {
            using var cert = new X509Certificate2(certificateBytes);

            if (cert.NotBefore > DateTimeOffset.UtcNow || cert.NotAfter < DateTimeOffset.UtcNow)
                return Result.Failure(ErrorCode.TlsConnectionFailed,
                    $"Certificate expired or not yet valid. Valid: {cert.NotBefore:O} to {cert.NotAfter:O}.");

            using var chain = new X509Chain { ChainPolicy = { RevocationMode = X509RevocationMode.Online } };
            var chainResult = chain.Build(cert);
            if (!chainResult)
                return Result.Failure(ErrorCode.TlsConnectionFailed, "Certificate chain validation failed.");

            if (!MatchesHost(cert, expectedHost))
                return Result.Failure(ErrorCode.TlsConnectionFailed,
                    $"Certificate does not match expected host '{expectedHost}'.");

            return Result.Success();
        }
        catch (Exception ex) when (ex is ArgumentException or System.Security.Cryptography.CryptographicException)
        {
            return Result.Failure(ErrorCode.TlsConnectionFailed, $"Certificate parsing failed: {ex.Message}");
        }
    }

    private bool ValidateCertificateInternal(
        object? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors,
        string expectedHost)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            return false;

        // Allow self-signed certs in development (chain errors only, no other errors)
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            return IsDevelopmentEnvironment();

        return false;
    }

    private static bool MatchesHost(X509Certificate2 cert, string expectedHost)
    {
        var sanExtension = cert.Extensions.OfType<X509SubjectAlternativeNameExtension>().FirstOrDefault();
        if (sanExtension != null)
        {
            foreach (var name in sanExtension.EnumerateDnsNames())
            {
                if (MatchesPattern(name, expectedHost))
                    return true;
            }
        }

        // Fallback to CN
        var cn = cert.GetNameInfo(X509NameType.SimpleName, false);
        return MatchesPattern(cn, expectedHost);
    }

    private static bool MatchesPattern(string pattern, string host)
    {
        if (string.Equals(pattern, host, StringComparison.OrdinalIgnoreCase))
            return true;

        // Wildcard: *.example.com matches sub.example.com
        if (pattern.StartsWith("*.", StringComparison.Ordinal) &&
            host.Length > pattern.Length - 1 &&
            host.EndsWith(pattern[1..], StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    private X509CertificateCollection? LoadClientCertificates()
    {
        try
        {
            using var store = new X509Store(_certificateStoreName, _certificateStoreLocation);
            store.Open(OpenFlags.ReadOnly);
            var certs = store.Certificates.Find(X509FindType.FindByKeyUsage, "Digital Signature", validOnly: true);
            return certs.Count > 0 ? certs : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool IsDevelopmentEnvironment()
        => Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development"
           || Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
}
