using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Dicom;
using HnVue.Dicom.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace HnVue.Dicom.Tests;

/// <summary>
/// Unit tests for <see cref="ServiceCollectionExtensions.AddDicom"/>,
/// verifying DI registration and options binding.
/// </summary>
public sealed class ServiceCollectionExtensionsTests
{
    private static IServiceProvider BuildProvider(Dictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDicom(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddDicom_RegistersDicomService()
    {
        var provider = BuildProvider(new Dictionary<string, string?>());

        var service = provider.GetService<IDicomService>();

        service.Should().NotBeNull();
        service.Should().BeOfType<DicomService>();
    }

    [Fact]
    public void AddDicom_RegistersDicomOutbox()
    {
        var provider = BuildProvider(new Dictionary<string, string?>());

        var outbox = provider.GetService<DicomOutbox>();

        outbox.Should().NotBeNull();
    }

    [Fact]
    public void AddDicom_BindsOptionsFromConfiguration()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Dicom:LocalAeTitle"] = "MY_SCU",
            ["Dicom:PacsAeTitle"] = "MY_PACS",
            ["Dicom:PacsHost"] = "10.0.0.1",
            ["Dicom:PacsPort"] = "11112",
            ["Dicom:TlsEnabled"] = "true"
        };

        var provider = BuildProvider(settings);
        var opts = provider.GetRequiredService<IOptions<DicomOptions>>().Value;

        opts.LocalAeTitle.Should().Be("MY_SCU");
        opts.PacsAeTitle.Should().Be("MY_PACS");
        opts.PacsHost.Should().Be("10.0.0.1");
        opts.PacsPort.Should().Be(11112);
        opts.TlsEnabled.Should().BeTrue();
    }

    [Fact]
    public void AddDicom_EmptyConfiguration_UsesDefaults()
    {
        var provider = BuildProvider(new Dictionary<string, string?>());
        var opts = provider.GetRequiredService<IOptions<DicomOptions>>().Value;

        opts.LocalAeTitle.Should().Be("HNVUE");
        opts.PacsPort.Should().Be(104);
        opts.MwlPort.Should().Be(104);
        opts.PrinterPort.Should().Be(104);
    }

    [Fact]
    public void AddDicom_IDicomServiceIsSingleton()
    {
        var provider = BuildProvider(new Dictionary<string, string?>());

        var first = provider.GetRequiredService<IDicomService>();
        var second = provider.GetRequiredService<IDicomService>();

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void AddDicom_DicomOutboxIsSingleton()
    {
        var provider = BuildProvider(new Dictionary<string, string?>());

        var first = provider.GetRequiredService<DicomOutbox>();
        var second = provider.GetRequiredService<DicomOutbox>();

        first.Should().BeSameAs(second);
    }
}
