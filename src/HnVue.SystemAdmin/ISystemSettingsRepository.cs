using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.SystemAdmin;

/// <summary>
/// Abstracts persistence of system settings.
/// </summary>
public interface ISystemSettingsRepository
{
    /// <summary>Retrieves the current system settings.</summary>
    /// <returns>A <see cref="Result{T}"/> containing the settings on success.</returns>
    Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists updated system settings.</summary>
    /// <param name="settings">The settings to save.</param>
    /// <param name="cancellationToken">Token to cancel the async operation.</param>
    /// <returns>A <see cref="Result"/> indicating success or failure.</returns>
    Task<Result> SaveAsync(SystemSettings settings, CancellationToken cancellationToken = default);
}
