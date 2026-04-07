using HnVue.Common.Results;

namespace HnVue.CDBurning;

// @MX:WARN IMAPIComWrapper - @MX:REASON: COM interop to IMAPI2, simulation mode for testing only
// @MX:TODO Production implementation requires real IMAPI2 IDiscFormat2Data P/Invoke calls
/// <summary>
/// Wraps the Windows IMAPI2 COM interface for optical disc burning operations.
/// </summary>
/// <remarks>
/// IMAPI2 (Image Mastering API v2) is the Windows platform API for CD/DVD burning.
/// This class is a thin façade over the COM interop, making the actual burn hardware
/// mockable for unit tests.
///
/// In production, this calls IDiscFormat2Data COM methods via the IBurnSession interface.
/// </remarks>
public sealed class IMAPIComWrapper : IBurnSession
{
    private bool _discInserted;
    private bool _discIsBlank = true;
    private long _discCapacityBytes = 700L * 1024 * 1024; // 700 MB CD

    // @MX:NOTE Test helper method - not for production use
    /// <summary>
    /// Injects disc state for simulation/testing.
    /// Call this to simulate a disc being inserted.
    /// </summary>
    public void SimulateDiscInserted(bool blank = true, long capacityBytes = 700L * 1024 * 1024)
    {
        _discInserted = true;
        _discIsBlank = blank;
        _discCapacityBytes = capacityBytes;
    }

    /// <inheritdoc/>
    public Task<Result<bool>> IsDiscInsertedAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(_discInserted));

    /// <inheritdoc/>
    public Task<Result<bool>> IsDiscBlankAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(_discIsBlank));

    /// <inheritdoc/>
    public Task<Result<long>> GetDiscCapacityBytesAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(_discCapacityBytes));

    /// <inheritdoc/>
    public async Task<Result> BurnFilesAsync(
        IEnumerable<BurnFileEntry> files,
        string volumeLabel,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(volumeLabel);

        if (!_discInserted)
            return Result.Failure(ErrorCode.BurnFailed, "No disc is inserted.");

        if (!_discIsBlank)
            return Result.Failure(ErrorCode.BurnFailed, "Disc is not blank.");

        var fileList = files.ToList();
        var totalSize = fileList.Sum(f => f.SizeBytes);

        if (totalSize > _discCapacityBytes)
            return Result.Failure(ErrorCode.BurnFailed,
                $"Total file size ({totalSize:N0} bytes) exceeds disc capacity ({_discCapacityBytes:N0} bytes).");

        // @MX:WARN Task.Delay simulates burn time - production requires async IMAPI2 progress events
        // Simulate burn progress
        for (var i = 0; i < fileList.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report((double)(i + 1) / fileList.Count * 100.0);
            await Task.Delay(1, cancellationToken).ConfigureAwait(false); // Simulated burn time
        }

        _discIsBlank = false; // Disc is now written
        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> VerifyAsync(
        IEnumerable<BurnFileEntry> expectedFiles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(expectedFiles);

        if (!_discInserted)
            return Result.Failure<bool>(ErrorCode.DiscVerificationFailed, "No disc is inserted.");

        // Simulate successful verification (real implementation compares checksums)
        await Task.Delay(1, cancellationToken).ConfigureAwait(false);
        return Result.Success(true);
    }
}
