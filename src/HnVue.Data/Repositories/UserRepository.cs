using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Data.Mappers;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Repositories;

// @MX:ANCHOR UserRepository - @MX:REASON: Credential persistence with 10 operations, lockout & PIN tracking
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
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
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<IReadOnlyList<UserRecord>>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> SetQuickPinHashAsync(string userId, string? pinHash, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

            entity.QuickPinHash = pinHash;
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
    public async Task<Result<string?>> GetQuickPinHashAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure<string?>(ErrorCode.NotFound, $"User '{userId}' not found.");

            return Result.SuccessNullable<string?>(entity.QuickPinHash);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Result.Failure<string?>(
                ErrorCode.DatabaseError,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> AddAsync(UserRecord user, CancellationToken ct = default)
    {
        try
        {
            var exists = await context.Users.AnyAsync(u => u.Username == user.Username, ct).ConfigureAwait(false);
            if (exists)
            {
                return Result.Failure(ErrorCode.AlreadyExists, $"Username '{user.Username}' already exists.");
            }

            var entity = EntityMapper.ToEntity(user);
            await context.Users.AddAsync(entity, ct).ConfigureAwait(false);
            await context.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            return Result.Failure(
                ErrorCode.AlreadyExists,
                ex.InnerException?.Message ?? ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result> UpdateQuickPinFailureAsync(
        string userId,
        int failedCount,
        DateTimeOffset? lockedUntil,
        CancellationToken ct = default)
    {
        try
        {
            var entity = await context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId, ct)
                .ConfigureAwait(false);

            if (entity is null)
                return Result.Failure(ErrorCode.NotFound, $"User '{userId}' not found.");

            entity.QuickPinFailedCount = failedCount;
            entity.QuickPinLockedUntilTicks = lockedUntil?.UtcTicks;
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
