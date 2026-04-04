using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IStudyRepository"/> backed by the encrypted SQLite database.
/// </summary>
internal sealed class StudyRepository(HnVueDbContext context) : IStudyRepository
{
    /// <inheritdoc/>
    public async Task<Result<StudyRecord>> AddAsync(StudyRecord study, CancellationToken ct = default)
    {
        try
        {
            var entity = EntityMapper.ToEntity(study);
            context.Studies.Add(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success(EntityMapper.ToRecord(entity));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<StudyRecord>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<StudyRecord>>> GetByPatientAsync(string patientId, CancellationToken ct = default)
    {
        try
        {
            var entities = await context.Studies
                .AsNoTracking()
                .Where(s => s.PatientId == patientId)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            IReadOnlyList<StudyRecord> records = entities
                .Select(EntityMapper.ToRecord)
                .ToList();

            return Result.Success(records);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<StudyRecord>>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<StudyRecord?>> GetByUidAsync(string studyInstanceUid, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Studies
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.StudyInstanceUid == studyInstanceUid, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.SuccessNullable<StudyRecord?>(null);

            return Result.Success<StudyRecord?>(EntityMapper.ToRecord(entity));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Result.Failure<StudyRecord?>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateAsync(StudyRecord study, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Studies
                .FirstOrDefaultAsync(s => s.StudyInstanceUid == study.StudyInstanceUid, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"Study '{study.StudyInstanceUid}' not found.");

            EntityMapper.ApplyUpdate(entity, study);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }
}
