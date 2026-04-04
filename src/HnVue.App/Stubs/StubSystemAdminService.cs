using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.App.Stubs;

/// <summary>
/// Stub implementation of <see cref="ISystemAdminService"/> used until the HnVue.SystemAdmin module
/// is integrated in Wave 3.
/// GetSettings returns a default <see cref="SystemSettings"/> instance;
/// mutating operations return failure results.
/// </summary>
internal sealed class StubSystemAdminService : ISystemAdminService
{
    private const string NotImplementedMessage =
        "SystemAdminService not implemented in Wave 2. Available from Wave 3.";

    /// <inheritdoc/>
    public Task<Result<SystemSettings>> GetSettingsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success<SystemSettings>(new SystemSettings()));

    /// <inheritdoc/>
    public Task<Result> UpdateSettingsAsync(
        SystemSettings settings,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));

    /// <inheritdoc/>
    public Task<Result> ExportAuditLogAsync(
        string outputPath,
        CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure(ErrorCode.Unknown, NotImplementedMessage));
}
