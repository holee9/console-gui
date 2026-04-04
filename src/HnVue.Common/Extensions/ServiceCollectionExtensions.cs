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
/// In-memory implementation of <see cref="ISecurityContext"/>.
/// Stores the authenticated user for the duration of the current application session.
/// Thread-safe for read access; write access must be serialised by the caller.
/// </summary>
internal sealed class ThreadLocalSecurityContext : ISecurityContext
{
    private AuthenticatedUser? _currentUser;

    /// <inheritdoc/>
    public string? CurrentUserId => _currentUser?.UserId;

    /// <inheritdoc/>
    public string? CurrentUsername => _currentUser?.Username;

    /// <inheritdoc/>
    public UserRole? CurrentRole => _currentUser?.Role;

    /// <inheritdoc/>
    public bool IsAuthenticated => _currentUser != null;

    /// <inheritdoc/>
    public bool HasRole(UserRole role) => _currentUser?.Role == role;

    /// <inheritdoc/>
    public void SetCurrentUser(AuthenticatedUser user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _currentUser = user;
    }

    /// <inheritdoc/>
    public void ClearCurrentUser() => _currentUser = null;
}
