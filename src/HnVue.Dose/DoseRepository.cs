using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Dose;

/// <summary>
/// EF Core implementation of <see cref="IDoseRepository"/>.
/// Persists and retrieves radiation dose records via <see cref="HnVueDbContext"/>.
/// </summary>
public sealed class DoseRepository : IDoseRepository
{
    private readonly HnVueDbContext _dbContext;

    /// <summary>
    /// Initialises a new <see cref="DoseRepository"/>.
    /// </summary>
    /// <param name="dbContext">EF Core database context.</param>
    public DoseRepository(HnVueDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-001: Dose records must be persisted immediately after each exposure event.</remarks>
    public async Task<Result> SaveAsync(DoseRecord dose, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dose);

        try
        {
            var entity = ToEntity(dose);
            await _dbContext.DoseRecords.AddAsync(entity, cancellationToken).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure(ErrorCode.DatabaseError,
                $"Failed to save dose record '{dose.DoseId}': {ex.Message}");
        }
    }

    /// <inheritdoc/>
    /// <remarks>SWR-DA-002: Dose records can be retrieved by study UID for reporting and audit.</remarks>
    public async Task<Result<DoseRecord?>> GetByStudyAsync(
        string studyInstanceUid, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(studyInstanceUid);

        try
        {
            var entity = await _dbContext.DoseRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.StudyInstanceUid == studyInstanceUid, cancellationToken)
                .ConfigureAwait(false);

            DoseRecord? record = entity is null ? null : ToRecord(entity);
            return Result.SuccessNullable<DoseRecord?>(record);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<DoseRecord?>(ErrorCode.DatabaseError,
                $"Failed to query dose record for study '{studyInstanceUid}': {ex.Message}");
        }
    }

    // ── Mapping helpers ────────────────────────────────────────────────────────

    private static DoseRecordEntity ToEntity(DoseRecord record) =>
        new()
        {
            DoseId = record.DoseId,
            StudyInstanceUid = record.StudyInstanceUid,
            Dap = record.Dap,
            Ei = record.Ei,
            EffectiveDose = record.EffectiveDose,
            BodyPart = record.BodyPart,
            RecordedAtTicks = record.RecordedAt.UtcTicks,
            RecordedAtOffsetMinutes = (int)record.RecordedAt.Offset.TotalMinutes,
        };

    private static DoseRecord ToRecord(DoseRecordEntity entity) =>
        new(
            DoseId: entity.DoseId,
            StudyInstanceUid: entity.StudyInstanceUid,
            Dap: entity.Dap,
            Ei: entity.Ei,
            EffectiveDose: entity.EffectiveDose,
            BodyPart: entity.BodyPart,
            RecordedAt: new DateTimeOffset(entity.RecordedAtTicks,
                TimeSpan.FromMinutes(entity.RecordedAtOffsetMinutes)));
}
