using HnVue.Common.Abstractions;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="ICDDVDBurnService"/> used until the HnVue.CDBurning module
/// is integrated in Wave 3.
/// All operations return failure results with a descriptive message.
/// </summary>
internal sealed class StubCDDVDBurnService : ICDDVDBurnService
{
    private const string NotImplementedMessage =
        "CDDVDBurnService not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public Task<Result> BurnStudyAsync(
        string studyInstanceUid,
        string outputLabel,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.BurnFailed, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result<bool>> VerifyDiscAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<bool>(
            ErrorCode.DiscVerificationFailed, NotImplementedMessage));
}
