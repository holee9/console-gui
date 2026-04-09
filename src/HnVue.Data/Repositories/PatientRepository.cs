using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

// @MX:ANCHOR PatientRepository - @MX:REASON: PHI persistence with 5 CRUD operations, encrypted SQLite backend
/// <summary>
/// EF Core implementation of <see cref="IPatientRepository"/> backed by the encrypted SQLite database.
/// Supports column-level PHI encryption per SWR-CS-080.
/// </summary>
internal sealed class PatientRepository(
    HnVueDbContext context,
    IAuditRepository auditRepository,
    IPhiEncryptionService? phiEncryptionService,
    ISecurityContext? securityContext = null) : IPatientRepository
{
    /// <inheritdoc/>
    public async Task<Result<PatientRecord>> AddAsync(PatientRecord patient, CancellationToken ct = default)
    {
        try
        {
            var entity = EntityMapper.ToEntity(patient, phiEncryptionService);
            context.Patients.Add(entity);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);

            // SWR-PM-012: Audit trail for patient creation
            var lastHashResult = await auditRepository.GetLastHashAsync(ct).ConfigureAwait(false);
            var previousHash = lastHashResult.IsSuccess ? lastHashResult.Value : null;
            var userName = securityContext?.CurrentUserId ?? "anonymous";
            await auditRepository.AppendAsync(new AuditEntry(
                DateTimeOffset.UtcNow,
                userName,
                "PatientCreated",
                "pending",
                $"Patient {patient.Name} ({entity.PatientId})",
                previousHash), ct).ConfigureAwait(false);

            return Result.Success(EntityMapper.ToRecord(entity, phiEncryptionService));
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
                .Where(p => !p.IsDeleted)
                .FirstOrDefaultAsync(p => p.PatientId == patientId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.SuccessNullable<PatientRecord?>(null);

            return Result.Success<PatientRecord?>(EntityMapper.ToRecord(entity, phiEncryptionService));
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
                .Where(p => !p.IsDeleted)
                .Where(p => EF.Functions.Like(p.PatientId, $"%{query}%")
                         || EF.Functions.Like(p.Name, $"%{query}%"))
                .ToListAsync(ct)
                .ConfigureAwait(false);

            IReadOnlyList<PatientRecord> records = entities
                .Select(e => EntityMapper.ToRecord(e, phiEncryptionService))
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

            EntityMapper.ApplyUpdate(entity, patient, phiEncryptionService);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);

            // SWR-PM-012: Audit trail for patient update
            var lastHashResult = await auditRepository.GetLastHashAsync(ct).ConfigureAwait(false);
            var previousHash = lastHashResult.IsSuccess ? lastHashResult.Value : null;
            await auditRepository.AppendAsync(new AuditEntry(
                DateTimeOffset.UtcNow,
                (securityContext?.CurrentUserId ?? "anonymous")!, // REQ-DATA-002: Use actual user from ISecurityContext
                "PatientUpdated",
                "pending",
                $"Patient {patient.Name} ({entity.PatientId})",
                previousHash), ct).ConfigureAwait(false);

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

            if (entity.IsDeleted)
                return Result.Success(); // Idempotent: already soft-deleted is OK

            entity.IsDeleted = true;
            await context.SaveChangesAsync(ct).ConfigureAwait(false);

            // SWR-PM-012: Audit trail for patient deletion (soft delete)
            var lastHashResult = await auditRepository.GetLastHashAsync(ct).ConfigureAwait(false);
            var previousHash = lastHashResult.IsSuccess ? lastHashResult.Value : null;
            await auditRepository.AppendAsync(new AuditEntry(
                DateTimeOffset.UtcNow,
                (securityContext?.CurrentUserId ?? "anonymous")!, // REQ-DATA-002: Use actual user from ISecurityContext
                "PatientDeleted",
                "pending",
                $"Patient {entity.Name} ({entity.PatientId}) - soft delete",
                previousHash), ct).ConfigureAwait(false);

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
