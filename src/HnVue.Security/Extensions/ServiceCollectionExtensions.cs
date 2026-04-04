using HnVue.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Security.Extensions;

/// <summary>
/// Extension methods for registering HnVue.Security services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all HnVue.Security services including <see cref="ISecurityService"/> and
    /// <see cref="IAuditService"/> implementations.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <param name="jwtOptions">
    /// Optional JWT configuration. When <see langword="null"/>, default development options are used.
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddHnVueSecurity(
        this IServiceCollection services,
        JwtOptions? jwtOptions = null)
    {
        var opts = jwtOptions ?? new JwtOptions();
        services.AddSingleton(opts);
        services.AddSingleton<JwtTokenService>();
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
