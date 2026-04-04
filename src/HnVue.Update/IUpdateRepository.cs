using HnVue.Common.Models;
using HnVue.Common.Results;

namespace HnVue.Update;

/// <summary>
/// Abstracts the update server communication layer.
/// </summary>
public interface IUpdateRepository
{
    /// <summary>Queries the update server for a newer version.</summary>
    Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>Retrieves package metadata (including expected hash) for the given package path.</summary>
    Task<Result<UpdateInfo>> GetPackageInfoAsync(string packagePath, CancellationToken cancellationToken = default);

    /// <summary>Extracts and installs the update package.</summary>
    Task<Result> ApplyPackageAsync(string packagePath, CancellationToken cancellationToken = default);
}
