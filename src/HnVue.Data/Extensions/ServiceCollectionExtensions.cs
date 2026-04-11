using System.Security.Cryptography;
using HnVue.Common.Abstractions;
using HnVue.Data.Repositories;
using HnVue.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Data.Extensions;

// @MX:ANCHOR: [AUTO] ServiceCollectionExtensions - DI composition root for Data layer, registers 4 repositories + DbContext + PHI encryption
// @MX:REASON: All Data module dependencies resolved here
// @MX:WARN: [AUTO] SQLCipher Password parameter must come from secure config (never hardcoded) - SWR-CS-080
/// <summary>
/// Extension methods for registering HnVue.Data services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the EF Core DbContext and all repository implementations with the DI container.
    /// Uses HKDF to derive the PHI encryption key from the SQLCipher connection password (REQ-PHI-002).
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="connectionString">
    /// SQLite connection string. For encrypted databases use SQLCipher format:
    /// <c>Data Source=hnvue.db;Password=&lt;encryption-key&gt;</c>.
    /// </param>
    /// <param name="phiEncryptionKey">
    /// Optional 32-byte AES-256-GCM key for column-level PHI encryption (REQ-PHI-001).
    /// When null, a key is derived from the SQLCipher password in <paramref name="connectionString"/> via HKDF.
    /// If the connection string has no password, a random key is generated (development only).
    /// </param>
    /// <returns>The same <paramref name="services"/> for call chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="phiEncryptionKey"/> is provided but not 32 bytes.</exception>
    public static IServiceCollection AddHnVueData(
        this IServiceCollection services,
        string connectionString,
        byte[]? phiEncryptionKey = null)
    {
        // Resolve PHI encryption key (REQ-PHI-001, REQ-PHI-002)
        IPhiEncryptionService phiService;
        if (phiEncryptionKey is not null)
        {
            if (phiEncryptionKey.Length != 32)
                throw new ArgumentException("PHI encryption key must be 32 bytes for AES-256-GCM (REQ-PHI-001).", nameof(phiEncryptionKey));
            phiService = new AesGcmPhiEncryptionService(phiEncryptionKey);
        }
        else
        {
            // Derive PHI encryption key from SQLCipher password using HKDF (REQ-PHI-002)
            var sqlCipherPassword = ExtractPasswordFromConnectionString(connectionString);
            if (!string.IsNullOrWhiteSpace(sqlCipherPassword))
            {
                phiService = AesGcmPhiEncryptionService.FromSqlCipherKey(sqlCipherPassword);
            }
            else
            {
                // Development fallback: random key (data cannot be decrypted across restarts)
                phiService = new AesGcmPhiEncryptionService(RandomNumberGenerator.GetBytes(32));
            }
        }

        services.AddSingleton(phiService);
        services.AddSingleton<IPhiEncryptionService>(phiService);

        // Register DbContext options (required by EF Core infrastructure)
        var dbContextOptions = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connectionString)
            .Options;
        services.AddSingleton(dbContextOptions);

        // Register DbContext as Scoped with PHI encryption service (REQ-PHI-003)
        // This factory pattern ensures PHI value converters are configured in OnModelCreating.
        services.AddScoped<HnVueDbContext>(sp =>
        {
            var opts = sp.GetRequiredService<DbContextOptions<HnVueDbContext>>();
            var encryption = sp.GetRequiredService<IPhiEncryptionService>();
            return new HnVueDbContext(opts, encryption);
        });

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }

    /// <summary>
    /// Extracts the Password parameter from a SQLite/SQLCipher connection string.
    /// </summary>
    private static string? ExtractPasswordFromConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return null;

        foreach (var part in connectionString.Split(';'))
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                return trimmed["Password=".Length..].Trim();
        }

        return null;
    }
}
