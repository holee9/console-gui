using HnVue.Common.Abstractions;
using HnVue.Common.Configuration;
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
        // Issue #18: Enforce minimum SecretKey length at startup to prevent empty-key vulnerability.
        // An empty key would allow forging tokens with a known secret (effectively no signing).
        if (string.IsNullOrEmpty(opts.SecretKey) || opts.SecretKey.Length < 32)
            throw new InvalidOperationException(
                "JWT SecretKey must be at least 32 characters. " +
                "Set the 'Jwt:SecretKey' configuration key or 'HNVUE_JWT_SECRET' environment variable.");
        services.AddSingleton(opts);
        services.AddSingleton<JwtTokenService>();

        // SWR-CS-077: Register in-memory token denylist for session revocation
        var tokenDenylist = new InMemoryTokenDenylist(TimeSpan.FromMinutes(opts.ExpiryMinutes));
        services.AddSingleton<ITokenDenylist>(tokenDenylist);

        services.AddScoped<ISecurityService, SecurityService>();

        var audit = auditOptions ?? new AuditOptions();
        services.AddSingleton(Options.Create(audit));
        services.AddScoped<IAuditService, AuditService>();
        return services;
    }

    /// <summary>
    /// Registers <see cref="IPhiEncryptionService"/> for column-level PHI encryption (SWR-CS-080).
    /// Requires <see cref="HnVueOptions.PhiEncryptionKey"/> to be configured.
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddPhiEncryption(this IServiceCollection services)
    {
        services.AddSingleton<IPhiEncryptionService>(sp =>
        {
            var hnvueOptions = sp.GetRequiredService<IOptions<HnVueOptions>>();
            var keyBase64 = hnvueOptions.Value.PhiEncryptionKey;
            if (string.IsNullOrEmpty(keyBase64))
                throw new InvalidOperationException(
                    "HnVue:PhiEncryptionKey must be configured for column-level PHI encryption (SWR-CS-080). " +
                    "Set the 'HnVue:PhiEncryptionKey' configuration key or 'HNVUE_PHI_ENCRYPTION_KEY' environment variable.");
            var key = Convert.FromBase64String(keyBase64);
            return new PhiEncryptionService(key);
        });
        return services;
    }
}
