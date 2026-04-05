using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Incident.Extensions;

/// <summary>
/// Extension methods for registering HnVue.Incident services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all HnVue.Incident services including <see cref="IIncidentService"/>.
    /// Callers must also register <see cref="HnVue.Common.Abstractions.IAuditService"/>
    /// (provided by <c>HnVue.Security.Extensions.ServiceCollectionExtensions.AddHnVueSecurity</c>).
    /// </summary>
    /// <param name="services">The service collection to add registrations to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> to allow chaining.</returns>
    public static IServiceCollection AddIncident(this IServiceCollection services)
    {
        services.AddSingleton<IncidentRepository>();
        services.AddSingleton<NotificationService>();
        services.AddSingleton<IIncidentService, IncidentService>();
        return services;
    }
}
