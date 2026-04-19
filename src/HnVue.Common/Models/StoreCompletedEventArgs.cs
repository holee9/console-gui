namespace HnVue.Common.Models;

/// <summary>
/// Event data raised when a C-STORE operation completes (either success or final failure).
/// </summary>
/// <param name="FilePath">Path to the DICOM file that was transmitted.</param>
/// <param name="Success">Whether the transmission succeeded.</param>
/// <param name="ErrorMessage">Error description on failure; null on success.</param>
/// <param name="Attempts">Total number of attempts including retries.</param>
/// <param name="Duration">Wall-clock time from first attempt to completion.</param>
public sealed class StoreCompletedEventArgs(
    string FilePath,
    bool Success,
    string? ErrorMessage,
    int Attempts,
    TimeSpan Duration) : EventArgs
{
    /// <summary>Path to the DICOM file that was transmitted.</summary>
    public string FilePath { get; } = FilePath;

    /// <summary>Whether the transmission ultimately succeeded.</summary>
    public bool Success { get; } = Success;

    /// <summary>Error description on failure; <see langword="null"/> on success.</summary>
    public string? ErrorMessage { get; } = ErrorMessage;

    /// <summary>Total number of C-STORE attempts including retries.</summary>
    public int Attempts { get; } = Attempts;

    /// <summary>Wall-clock time from first attempt to completion.</summary>
    public TimeSpan Duration { get; } = Duration;
}
