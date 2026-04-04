using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/> backed by the encrypted SQLite database.
/// </summary>
internal sealed class UserRepository(HnVueDbContext context) : IUserRepository
{
    /// <inheritdoc/>
    public async Task<Result<UserRecord>> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure<UserRecord>(ErrorCode.NotFound, $"User '{username}' not found.");

            return Result.Success(EntityMapper.ToRecord(entity));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<UserRecord>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<UserRecord>> GetByIdAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure<UserRecord>(ErrorCode.NotFound, $"User '{userId}' not found.");

            return Result.Success(EntityMapper.ToRecord(entity));
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<UserRecord>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateFailedLoginCountAsync(string userId, int count, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

            entity.FailedLoginCount = count;
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
    public async Task<Result> SetLockedAsync(string userId, bool isLocked, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

            entity.IsLocked = isLocked;
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
    public async Task<Result> UpdatePasswordHashAsync(string userId, string passwordHash, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

            entity.PasswordHash = passwordHash;
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
    public async Task<Result<IReadOnlyList<UserRecord>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var entities = await context.Users
                .AsNoTracking()
                .ToListAsync(ct)
                .ConfigureAwait(false);

            IReadOnlyList<UserRecord> records = entities
                .Select(EntityMapper.ToRecord)
                .ToList();

            return Result.Success(records);
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure<IReadOnlyList<UserRecord>>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }
}
