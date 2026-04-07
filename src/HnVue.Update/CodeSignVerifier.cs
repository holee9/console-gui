using System.IO;
using System.Security.Cryptography;
using HnVue.Common.Results;

namespace HnVue.Update;

// @MX:TODO Integration test required with real Authenticode certificate to verify P/Invoke behavior
// @MX:NOTE SHA-256 verification prevents tampered updates - IEC 62304 §6.2.5 requirement
/// <summary>
/// Verifies the integrity of software update packages using SHA-256 hash comparison.
/// </summary>
/// <remarks>
/// Full Authenticode/code-signing certificate verification would require P/Invoke to WinTrust API.
/// This implementation performs hash-based integrity verification as the primary guard.
/// IEC 62304 §6.2.5: software integrity verification before installation.
/// </remarks>
public sealed class CodeSignVerifier
{
    // @MX:ANCHOR VerifyHashAsync - @MX:REASON: Called by SWUpdateService for integrity verification
    /// <summary>
    /// Verifies that the file at <paramref name="filePath"/> matches the expected
    /// <paramref name="expectedSha256"/> hash (hex-encoded, case-insensitive).
    /// </summary>
    /// <param name="filePath">Absolute path to the file to verify.</param>
    /// <param name="expectedSha256">Expected SHA-256 hash as a 64-character hex string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// Success if the hash matches; failure with
    /// <see cref="ErrorCode.SignatureVerificationFailed"/> otherwise.
    /// </returns>
    public static async Task<Result> VerifyHashAsync(
        string filePath,
        string expectedSha256,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(expectedSha256);

        if (!File.Exists(filePath))
            return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                $"Package file not found: '{filePath}'.");

        try
        {
            using var stream = new FileStream(
                filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 81920, useAsync: true);

            var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false);
            var actualHash = Convert.ToHexString(hashBytes);

            if (!string.Equals(actualHash, expectedSha256, StringComparison.OrdinalIgnoreCase))
                return Result.Failure(
                    ErrorCode.SignatureVerificationFailed,
                    $"Hash mismatch. Expected: {expectedSha256}, Actual: {actualHash}.");

            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.UpdatePackageCorrupt,
                $"Failed to compute hash for '{filePath}': {ex.Message}");
        }
    }
}
