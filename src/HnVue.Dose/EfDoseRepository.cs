using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Dose;

/// <summary>
/// EF Core implementation of <see cref="IDoseRepository"/>.
/// Persists and queries radiation dose records via the encrypted SQLite database.
/// </summary>
public sealed class EfDoseRepository(HnVueDbContext context) : IDoseRepository
{
    /// <inheritdoc/>
    public async Task<Result> SaveAsync(DoseRecord dose, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dose);

        try
        {
            var entity = new Data.Entities.DoseRecordEntity
            {
                DoseId = dose.DoseId,
                StudyInstanceUid = dose.StudyInstanceUid,
                Dap = dose.Dap,
                Ei = dose.Ei,
                EffectiveDose = dose.EffectiveDose,
                BodyPart = dose.BodyPart,
                RecordedAtTicks = dose.RecordedAt.UtcTicks,
                RecordedAtOffsetMinutes = (int)dose.RecordedAt.Offset.TotalMinutes,
            };

            context.DoseRecords.Add(entity);
            await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<DoseRecord?>> GetByStudyAsync(string studyInstanceUid, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        try
        {
            var entity = await context.DoseRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.StudyInstanceUid == studyInstanceUid, cancellationToken)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.SuccessNullable<DoseRecord?>(null);

            return Result.Success<DoseRecord?>(ToRecord(entity));
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<DoseRecord?>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<DoseRecord>>> GetByPatientAsync(
        string patientId,
        DateTimeOffset? from,
        DateTimeOffset? until,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(patientId);

        try
        {
            var studyUids = await context.Studies
                .AsNoTracking()
                .Where(s => s.PatientId == patientId)
                .Select(s => s.StudyInstanceUid)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            var query = context.DoseRecords
                .AsNoTracking()
                .Where(d => studyUids.Contains(d.StudyInstanceUid));

            if (from.HasValue)
            {
                var fromTicks = from.Value.UtcTicks;
                query = query.Where(d => d.RecordedAtTicks >= fromTicks);
            }

            if (until.HasValue)
            {
                var untilTicks = until.Value.UtcTicks;
                query = query.Where(d => d.RecordedAtTicks <= untilTicks);
            }

            var entities = await query
                .OrderBy(d => d.RecordedAtTicks)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            IReadOnlyList<DoseRecord> records = entities.Select(ToRecord).ToList();
            return Result.Success(records);
        }
        catch (OperationCanceledException) { throw; }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<DoseRecord>>(ErrorCode.DatabaseError, ex.InnerException?.Message ?? ex.Message);
        }
    }

    private static DoseRecord ToRecord(Data.Entities.DoseRecordEntity entity) =>
        new(
            DoseId: entity.DoseId,
            StudyInstanceUid: entity.StudyInstanceUid,
            Dap: entity.Dap,
            Ei: entity.Ei,
            EffectiveDose: entity.EffectiveDose,
            BodyPart: entity.BodyPart,
            RecordedAt: new DateTimeOffset(entity.RecordedAtTicks, TimeSpan.FromMinutes(entity.RecordedAtOffsetMinutes)));
}
