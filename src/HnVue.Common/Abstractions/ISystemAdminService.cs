using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Common.Abstractions;

/// <summary>
/// Defines system administration operations: settings management and audit export.
/// Implemented by the HnVue.SystemAdmin module.
/// </summary>
public interface ISystemAdminService
{
    /// <summary>Retrieves the current system-wide settings.</summary>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    /// <returns>A successful <see cref="Result{T}"/> containing the <see cref="SystemSettings"/>.</returns>
    Task<Result<SystemSettings>> GetSettingsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates and persists updated system settings.
    /// Changes take effect immediately; a restart may be required for some settings (e.g., COM port).
    /// </summary>
    /// <param name="settings">Updated settings to validate and store.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> UpdateSettingsAsync(
        SystemSettings settings,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports all audit log entries to a signed CSV or PDF file at the specified path.
    /// </summary>
    /// <param name="outputPath">Absolute path of the output file to create.</param>
    /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
    Task<Result> ExportAuditLogAsync(
        string outputPath,
        CancellationToken cancellationToken = default);
}
