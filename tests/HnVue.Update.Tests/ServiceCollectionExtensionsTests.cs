using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Update.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.Update.Tests;

/// <summary>
/// Tests for <see cref="ServiceCollectionExtensions.AddSWUpdate"/> DI registration.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddSWUpdate_RegistersISWUpdateService()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData = new Dictionary<string, string?>
        {
            ["SWUpdate:UpdateServerUrl"] = "https://update.example.com/api/v1",
            ["SWUpdate:CurrentVersion"] = "1.0.0"
        };
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        // Act
        services.AddSWUpdate(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var service = provider.GetService<ISWUpdateService>();
        service.Should().NotBeNull("ISWUpdateService must be resolvable after AddSWUpdate");
    }

    [Fact]
    public void AddSWUpdate_BindsUpdateOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var configData2 = new Dictionary<string, string?>
        {
            ["SWUpdate:UpdateServerUrl"] = "https://my-server.com/api/v1",
            ["SWUpdate:CurrentVersion"] = "2.3.1",
            ["SWUpdate:RequireAuthenticodeSignature"] = "false"
        };
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData2)
            .Build();

        // Act
        services.AddSWUpdate(config);
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<UpdateOptions>>();
        options.Should().NotBeNull();
        options!.Value.UpdateServerUrl.Should().Be("https://my-server.com/api/v1");
        options.Value.CurrentVersion.Should().Be("2.3.1");
        options.Value.RequireAuthenticodeSignature.Should().BeFalse();
    }

    [Fact]
    public void AddSWUpdate_NullServices_ThrowsArgumentNullException()
    {
        // Arrange
        var config = new ConfigurationBuilder().Build();

        // Act
        Action act = () => ServiceCollectionExtensions.AddSWUpdate(null!, config);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddSWUpdate_NullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        Action act = () => services.AddSWUpdate(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
