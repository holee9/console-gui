using HnVue.Common.Results;

namespace HnVue.CDBurning;

/// <summary>
/// Abstracts the optical disc burning hardware session.
/// Implemented by <see cref="IMAPIComWrapper"/> for production
/// and by test doubles for unit testing.
/// </summary>
public interface IBurnSession
{
    /// <summary>Returns true if a disc is currently inserted in the drive.</summary>
    Task<Result<bool>> IsDiscInsertedAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns true if the inserted disc is blank and writable.</summary>
    Task<Result<bool>> IsDiscBlankAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the writable capacity of the inserted disc in bytes.</summary>
    Task<Result<long>> GetDiscCapacityBytesAsync(CancellationToken cancellationToken = default);

    /// <summary>Burns the specified files to the disc with the given volume label.</summary>
    Task<Result> BurnFilesAsync(
        IEnumerable<BurnFileEntry> files,
        string volumeLabel,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>Verifies the disc contents match the expected files.</summary>
    Task<Result<bool>> VerifyAsync(
        IEnumerable<BurnFileEntry> expectedFiles,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Describes a single file to burn to disc.
/// </summary>
/// <param name="SourcePath">Absolute path to the source file.</param>
/// <param name="DiscPath">Relative path on the disc (ISO 9660 format).</param>
/// <param name="SizeBytes">Size of the file in bytes.</param>
public sealed record BurnFileEntry(string SourcePath, string DiscPath, long SizeBytes);
