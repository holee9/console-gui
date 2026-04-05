using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IPatientRepository"/> backed by the encrypted SQLite database.
/// </summary>
internal sealed class PatientRepository(HnVueDbContext context) : IPatientRepository
{
    /// <inheritdoc/>
    public async Task<Result<PatientRecord>> AddAsync(PatientRecord patient, CancellationToken ct = default)
    {
        try
        {
            var entity = EntityMapper.ToEntity(patient);
            context.Patients.Add(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success(EntityMapper.ToRecord(entity));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<PatientRecord>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<PatientRecord?>> FindByIdAsync(string patientId, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PatientId == patientId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.SuccessNullable<PatientRecord?>(null);

            return Result.Success<PatientRecord?>(EntityMapper.ToRecord(entity));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<PatientRecord?>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<IReadOnlyList<PatientRecord>>> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var entities = await context.Patients
                .AsNoTracking()
                .Where(p => EF.Functions.Like(p.PatientId, $"%{query}%")
                         || EF.Functions.Like(p.Name, $"%{query}%"))
                .ToListAsync(ct)
                .ConfigureAwait(false);

            IReadOnlyList<PatientRecord> records = entities
                .Select(EntityMapper.ToRecord)
                .ToList();

            return Result.Success(records);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<PatientRecord>>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateAsync(PatientRecord patient, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"Patient '{patient.PatientId}' not found.");

            EntityMapper.ApplyUpdate(entity, patient);
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

    /// <inheritdoc/>
    public async Task<Result> DeleteAsync(string patientId, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patientId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"Patient '{patientId}' not found.");

            context.Patients.Remove(entity);
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
