using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for software update history in the HnVue.Data layer.
/// Tracks installation records in <see cref="HnVueDbContext.UpdateHistories"/>.
/// For DI registration, use <c>HnVue.Update.EfUpdateRepository</c> which implements <c>IUpdateRepository</c>.
/// REQ-COORD-004: SPEC-COORDINATOR-001 EF Core update history persistence.
/// </summary>
public sealed class EfUpdateRepository(HnVueDbContext context)
{
    /// <summary>Returns the latest installed update, or null if no history exists.</summary>
    public async Task<Result<UpdateInfo?>> CheckForUpdateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var latest = await context.UpdateHistories
                .AsNoTracking()
                .Where(h => h.Status == "Installed")
                .OrderByDescending(h => h.Timestamp)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            if (latest is null)
                return Result.SuccessNullable<UpdateInfo?>(null);

            var info = new UpdateInfo(
                Version: latest.ToVersion,
                ReleaseNotes: $"Updated from {latest.FromVersion}",
                PackageUrl: string.Empty,
                Sha256Hash: latest.PackageHash);

            return Result.SuccessNullable<UpdateInfo?>(info);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<UpdateInfo?>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <summary>Records a software update installation history entry.</summary>
    public async Task<Result> RecordInstallationAsync(
        string fromVersion,
        string toVersion,
        string packageHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(fromVersion);
        ArgumentNullException.ThrowIfNull(toVersion);

        try
        {
            var entry = new UpdateHistoryEntity
            {
                Timestamp = DateTime.UtcNow,
                FromVersion = fromVersion,
                ToVersion = toVersion,
                Status = "Installed",
                InstalledBy = Environment.UserName,
                PackageHash = packageHash ?? string.Empty,
            };

            context.UpdateHistories.Add(entry);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }
}
