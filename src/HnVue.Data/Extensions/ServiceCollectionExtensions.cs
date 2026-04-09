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
    /// <returns>The same <paramref name="services"/> for call chaining.</returns>
    public static IServiceCollection AddHnVueData(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<HnVueDbContext>(opts =>
            opts.UseSqlite(connectionString));

        // Register null PHI encryption service by default (SWR-CS-080 optional)
        // Will be replaced by AddPhiEncryption() if column-level encryption is configured
        if (services.All(s => s.ServiceType != typeof(IPhiEncryptionService)))
        {
            services.AddSingleton<IPhiEncryptionService, NullPhiEncryptionService>();
        }

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IStudyRepository, StudyRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();

        return services;
    }
}
