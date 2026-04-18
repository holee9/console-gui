using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Common.Extensions;

/// <summary>
/// Extension methods for registering HnVue.Common services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all services provided by HnVue.Common with the dependency injection container.
    /// Call this method in each module's composition root as part of bootstrapping.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for fluent chaining.</returns>
    public static IServiceCollection AddHnVueCommon(
        this IServiceCollection services)
    {
        // Register ISecurityContext as singleton so that all modules share the same
        // authenticated-user state within a single application process.
        services.AddSingleton<ISecurityContext, ThreadLocalSecurityContext>();

        return services;
    }
}

/// <summary>
/// Thread-safe in-memory implementation of <see cref="ISecurityContext"/>.
/// Stores the authenticated user for the duration of the current application session.
/// Uses <see cref="ReaderWriterLockSlim"/> for concurrent read access and exclusive write access.
/// </summary>
internal sealed class ThreadLocalSecurityContext : ISecurityContext, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
    private AuthenticatedUser? _currentUser;

    /// <inheritdoc/>
    public string? CurrentUserId
    {
        get
        {
            _lock.EnterReadLock();
            try { return _currentUser?.UserId; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <inheritdoc/>
    public string? CurrentUsername
    {
        get
        {
            _lock.EnterReadLock();
            try { return _currentUser?.Username; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <inheritdoc/>
    public UserRole? CurrentRole
    {
        get
        {
            _lock.EnterReadLock();
            try { return _currentUser?.Role; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <inheritdoc/>
    public bool IsAuthenticated
    {
        get
        {
            _lock.EnterReadLock();
            try { return _currentUser != null; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <inheritdoc/>
    public string? CurrentJti
    {
        get
        {
            _lock.EnterReadLock();
            try { return _currentUser?.Jti; }
            finally { _lock.ExitReadLock(); }
        }
    }

    /// <inheritdoc/>
    public bool HasRole(UserRole role)
    {
        _lock.EnterReadLock();
        try { return _currentUser?.Role == role; }
        finally { _lock.ExitReadLock(); }
    }

    /// <inheritdoc/>
    public void SetCurrentUser(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _lock.EnterWriteLock();
        try
        {
            _currentUser = user;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public void ClearCurrentUser()
    {
        _lock.EnterWriteLock();
        try
        {
            _currentUser = null;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <inheritdoc/>
    public void Dispose() => _lock.Dispose();
}
