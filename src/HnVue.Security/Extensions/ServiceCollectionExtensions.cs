using HnVue.Common.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
    /// <param name="auditOptions">
    /// Optional audit configuration. When <see langword="null"/>, default options are used
    /// (which will require HMAC key to be set before AuditService is resolved).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddHnVueSecurity(
        this IServiceCollection services,
        JwtOptions? jwtOptions = null,
        AuditOptions? auditOptions = null)
    {
        var opts = jwtOptions ?? new JwtOptions();
        services.AddSingleton(opts);
        services.AddSingleton<JwtTokenService>();
        services.AddScoped<ISecurityService, SecurityService>();

        var audit = auditOptions ?? new AuditOptions();
        services.AddSingleton(Options.Create(audit));
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }
}
