using HnVue.Common.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Dicom.Extensions;

/// <summary>
/// Extension methods for registering HnVue.Dicom services with the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers DICOM services: <see cref="IDicomService"/>, <see cref="DicomOutbox"/>,
    /// and binds <see cref="DicomOptions"/> from the "Dicom" configuration section.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">Application configuration used to bind <see cref="DicomOptions"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddDicom(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DicomOptions>(configuration.GetSection("Dicom"));
        services.AddSingleton<IDicomService, DicomService>();
        services.AddSingleton<DicomOutbox>();
        return services;
    }
}
