using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Incident;
using HnVue.Incident.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HnVue.Incident.Tests;

public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddIncident_RegistersIIncidentService_AsSingleton()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAuditService>());
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

        services.AddIncident();

        using var provider = services.BuildServiceProvider();
        var svc1 = provider.GetService<IIncidentService>();
        var svc2 = provider.GetService<IIncidentService>();

        svc1.Should().NotBeNull();
        svc1.Should().BeSameAs(svc2, "IIncidentService must be registered as singleton");
    }

    [Fact]
    public void AddIncident_ReturnsServiceCollection_ForChaining()
    {
        var services = new ServiceCollection();

        var returned = services.AddIncident();

        returned.Should().BeSameAs(services, "AddIncident must return the same IServiceCollection for chaining");
    }

    [Fact]
    public void AddIncident_CanResolveIncidentService_WhenDependenciesPresent()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IAuditService>());
        services.AddSingleton<ILoggerFactory, NullLoggerFactory>();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddIncident();

        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IIncidentService>();
        act.Should().NotThrow("all required dependencies are registered");
    }
}
