using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR ICDDVDBurnService - @MX:REASON: ISO 9660 disc burning with verification, medical record export
/// <summary>
/// Defines optical disc (CD/DVD) burn and verification operations.
/// Implemented by the HnVue.CDBurning module.
/// </summary>
public interface ICDDVDBurnService
{
    /// <summary>
    /// Burns all DICOM images belonging to the specified study to the inserted disc.
    /// The disc must be blank and writable; the operation verifies the disc after writing.
    /// </summary>
    /// <param name="studyInstanceUid">DICOM Study Instance UID identifying the study to burn.</param>
    /// <param name="outputLabel">Volume label to write to the disc (max 32 characters for ISO 9660).</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.BurnFailed"/>.</returns>
    Task<Result> BurnStudyAsync(
        string studyInstanceUid,
        string outputLabel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the integrity of the previously burned disc by comparing a checksum
    /// of its contents against the source data.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with <c>true</c> when verification passes;
    /// <c>false</c> when the disc contents do not match;
    /// or a failure with <see cref="ErrorCode.DiscVerificationFailed"/> when the check cannot complete.
    /// </returns>
    Task<Result<bool>> VerifyDiscAsync(
        CancellationToken cancellationToken = default);
}
