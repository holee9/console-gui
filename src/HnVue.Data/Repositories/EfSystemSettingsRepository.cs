using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for system configuration settings in the HnVue.Data layer.
/// Persists settings as a single-row <see cref="HnVueDbContext.SystemSettings"/> table.
/// For DI registration, use <c>HnVue.SystemAdmin.EfSystemSettingsRepository</c> which implements <c>ISystemSettingsRepository</c>.
/// REQ-COORD-005: SPEC-COORDINATOR-001 EF Core system settings persistence.
/// </summary>
public sealed class EfSystemSettingsRepository(HnVueDbContext context)
{
    /// <summary>Returns current system settings, or default values if none are persisted.</summary>
    public async Task<Result<SystemSettings>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await context.SystemSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == 1, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Success(BuildDefaults());

            return Result.Success(ToSettings(entity));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<SystemSettings>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <summary>Persists system settings, inserting or updating the singleton row.</summary>
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
                entity = new SystemSettingsEntity { Id = 1 };
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

    private static SystemSettings BuildDefaults() => new()
    {
        Dicom = new DicomSettings
        {
            LocalAeTitle = "HNVUE",
        },
        Security = new SecuritySettings
        {
            SessionTimeoutMinutes = 15,
            MaxFailedLogins = 5,
        },
    };

    private static SystemSettings ToSettings(SystemSettingsEntity entity) =>
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

    private static void ApplySettings(SystemSettingsEntity entity, SystemSettings settings)
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
