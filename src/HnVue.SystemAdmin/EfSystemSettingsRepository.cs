using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.SystemAdmin;

/// <summary>
/// EF Core implementation of <see cref="ISystemSettingsRepository"/>.
/// Persists system settings in a single-row <c>SystemSettings</c> table.
/// </summary>
public sealed class EfSystemSettingsRepository(HnVueDbContext context) : ISystemSettingsRepository
{
    /// <inheritdoc/>
    public async Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Success(new SystemSettings());

            return Result.Success(ToSettings(entity));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<SystemSettings>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> SaveAsync(SystemSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var entity = await context.SystemSettings
                .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
            {
                entity = new Data.Entities.SystemSettingsEntity { Id = 1 };
                context.SystemSettings.Add(entity);
            }

            ApplySettings(entity, settings);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static SystemSettings ToSettings(Data.Entities.SystemSettingsEntity entity) =>
        new()
        {
            Dicom = new DicomSettings
            {
                PacsAeTitle = entity.PacsAeTitle,
                PacsHost = entity.PacsHost,
                PacsPort = entity.PacsPort,
                LocalAeTitle = entity.LocalAeTitle,
            },
            Generator = new GeneratorSettings
            {
                ComPort = entity.ComPort,
                BaudRate = entity.BaudRate,
                TimeoutMs = entity.TimeoutMs,
            },
            Security = new SecuritySettings
            {
                SessionTimeoutMinutes = entity.SessionTimeoutMinutes,
                MaxFailedLogins = entity.MaxFailedLogins,
            },
        };

    private static void ApplySettings(Data.Entities.SystemSettingsEntity entity, SystemSettings settings)
    {
        entity.PacsAeTitle = settings.Dicom.PacsAeTitle;
        entity.PacsHost = settings.Dicom.PacsHost;
        entity.PacsPort = settings.Dicom.PacsPort;
        entity.LocalAeTitle = settings.Dicom.LocalAeTitle;
        entity.ComPort = settings.Generator.ComPort;
        entity.BaudRate = settings.Generator.BaudRate;
        entity.TimeoutMs = settings.Generator.TimeoutMs;
        entity.SessionTimeoutMinutes = settings.Security.SessionTimeoutMinutes;
        entity.MaxFailedLogins = settings.Security.MaxFailedLogins;
    }
}
