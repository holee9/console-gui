using System.Security.Cryptography;
using HnVue.Common.Abstractions;
using HnVue.Data.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace HnVue.Data.Tests.Extensions;

/// <summary>
/// Verifies that <see cref="ServiceCollectionExtensions.AddHnVueData"/> registers all required services.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddHnVueData_RegistersAllRepositories()
    {
        var services = new ServiceCollection();
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);

        services.AddHnVueData("Data Source=:memory:", key);

        var provider = services.BuildServiceProvider();

        provider.GetService<IPatientRepository>().Should().NotBeNull();
        provider.GetService<IStudyRepository>().Should().NotBeNull();
        provider.GetService<IUserRepository>().Should().NotBeNull();
        provider.GetService<IAuditRepository>().Should().NotBeNull();
    }

    [Fact]
    public void AddHnVueData_RegistersDbContext()
    {
        var services = new ServiceCollection();
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);

        services.AddHnVueData("Data Source=:memory:", key);

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var ctx = scope.ServiceProvider.GetService<HnVueDbContext>();
        ctx.Should().NotBeNull();
    }
}

