using HnVue.Common.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Update.Extensions;

/// <summary>
/// Extension methods for registering HnVue.Update services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the software update services and binds configuration from the "SWUpdate" section.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">Application configuration containing the "SWUpdate" section.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddSWUpdate(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<UpdateOptions>(configuration.GetSection("SWUpdate"));

        // Named HttpClient for UpdateChecker; base address and timeouts can be configured via IHttpClientBuilder.
        services.AddHttpClient(nameof(UpdateChecker));

        // BackupManager is not registered in DI; it is constructed internally by SWUpdateService.
        // Register the primary service.
        services.AddSingleton<ISWUpdateService, SWUpdateService>();

        return services;
    }
}
