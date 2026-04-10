using System.Security.Cryptography;
using HnVue.Common.Abstractions;
using HnVue.Data.Repositories;
using HnVue.Data.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Data.Extensions;

// @MX:ANCHOR: [AUTO] ServiceCollectionExtensions - DI composition root for Data layer, registers 4 repositories + DbContext
// @MX:REASON: All Data module dependencies resolved here
// @MX:WARN: [AUTO] SQLCipher Password parameter must come from secure config (never hardcoded) - SWR-CS-080
/// <summary>
/// Extension methods for registering HnVue.Data services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EF Core DbContext and all repository implementations with the DI container.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="connectionString">
    /// SQLite connection string. For encrypted databases use SQLCipher format:
    /// <c>Data Source=hnvue.db;Password=&lt;encryption-key&gt;</c>.
    /// </param>
    /// <param name="phiEncryptionKey">
    /// 32-byte AES-256-GCM key for column-level PHI encryption (REQ-DATA-001).
    /// Generate using: <c>RandomNumberGenerator.GetBytes(32);</c>
    /// </param>
    /// <returns>The same <paramref name="services"/> for call chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="phiEncryptionKey"/> is provided but not 32 bytes.</exception>
    public static IServiceCollection AddHnVueData(
        this IServiceCollection services,
        string connectionString,
        byte[]? phiEncryptionKey = null)
    {
        services.AddDbContext<HnVueDbContext>(opts =>
            opts.UseSqlite(connectionString));

        // Register PHI encryption service (REQ-DATA-001)
        if (phiEncryptionKey is not null)
        {
            if (phiEncryptionKey.Length != 32)
                throw new ArgumentException("PHI encryption key must be 32 bytes for AES-256-GCM (REQ-DATA-001).", nameof(phiEncryptionKey));
            services.AddSingleton<IPhiEncryptionService>(new PhiEncryptionService(phiEncryptionKey));
        }
        else
        {
            // Fallback: no encryption (for development/testing without configured key)
            services.AddSingleton<IPhiEncryptionService>(new PhiEncryptionService(RandomNumberGenerator.GetBytes(32)));
        }

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }
}
