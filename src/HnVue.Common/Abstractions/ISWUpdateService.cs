using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

// @MX:ANCHOR ISWUpdateService - @MX:REASON: IEC 62304 §6.2.5 signature verification, rollback lifecycle, safety-critical update
/// <summary>
/// Defines software update lifecycle operations: check, apply, and rollback.
/// Implemented by the HnVue.Update module.
/// All update packages must pass signature verification before installation (IEC 62304 §6.2.5).
/// </summary>
public interface ISWUpdateService
{
    /// <summary>
    /// Queries the configured update server for a newer software version.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> containing <see cref="UpdateInfo"/> when an update is available,
    /// or <see langword="null"/> when the software is already up to date.
    /// Returns a failure when the server cannot be reached.
    /// </returns>
    Task<Result<UpdateInfo?>> CheckUpdateAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies the digital signature of the package at <paramref name="packagePath"/> and,
    /// if valid, applies the update.
    /// </summary>
    /// <param name="packagePath">Absolute path to the downloaded update package file.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>
    /// A successful <see cref="Result"/> on completion,
    /// or a failure with <see cref="ErrorCode.SignatureVerificationFailed"/> or <see cref="ErrorCode.UpdatePackageCorrupt"/>.
    /// </returns>
    Task<Result> ApplyUpdateAsync(
        string packagePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current software version to the previous installed version.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result"/>, or a failure with <see cref="ErrorCode.RollbackFailed"/>.</returns>
    Task<Result> RollbackAsync(
        CancellationToken cancellationToken = default);
}
