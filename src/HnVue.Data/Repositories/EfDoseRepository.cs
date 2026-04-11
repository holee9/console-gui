using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core repository for radiation dose records in the HnVue.Data layer.
/// Implements CRUD and aggregation operations against the <see cref="HnVueDbContext.DoseRecords"/> DbSet.
/// For DI registration, use the <c>HnVue.Dose.EfDoseRepository</c> which implements <c>IDoseRepository</c>.
/// REQ-COORD-001: SPEC-COORDINATOR-001 EF Core dose persistence.
/// </summary>
public sealed class EfDoseRepository(HnVueDbContext context)
{
    /// <summary>Persists a dose record to the database.</summary>
    public async Task<Result> SaveAsync(DoseRecord dose, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dose);

        try
        {
            var entity = new DoseRecordEntity
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

    /// <summary>Returns the dose record for the specified study instance UID, or null if not found.</summary>
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

    /// <summary>
    /// Returns dose records for the specified patient within an optional date range,
    /// ordered by recording time ascending.
    /// </summary>
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

    private static DoseRecord ToRecord(DoseRecordEntity entity) =>
        new(
            DoseId: entity.DoseId,
            StudyInstanceUid: entity.StudyInstanceUid,
            Dap: entity.Dap,
            Ei: entity.Ei,
            EffectiveDose: entity.EffectiveDose,
            BodyPart: entity.BodyPart,
            RecordedAt: new DateTimeOffset(entity.RecordedAtTicks, TimeSpan.FromMinutes(entity.RecordedAtOffsetMinutes)));
}
