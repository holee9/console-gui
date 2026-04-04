using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.SystemAdmin;

/// <summary>
/// Abstracts persistence of system settings.
/// </summary>
public interface ISystemSettingsRepository
{
    /// <summary>Retrieves the current system settings.</summary>
    Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists updated system settings.</summary>
    Task<Result> SaveAsync(SystemSettings settings, CancellationToken cancellationToken = default);
}
