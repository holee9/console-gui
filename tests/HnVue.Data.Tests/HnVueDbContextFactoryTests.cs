using HnVue.Data;
using Microsoft.EntityFrameworkCore;

namespace HnVue.Data.Tests;

/// <summary>
/// Tests for <see cref="HnVueDbContextFactory"/> design-time context creation.
/// </summary>
[Trait("Category", "Data")]
public sealed class HnVueDbContextFactoryTests
{
    [Fact]
    public void CreateDbContext_ReturnsNonNullContext()
    {
        var factory = new HnVueDbContextFactory();

        var ctx = factory.CreateDbContext(Array.Empty<string>());

        ctx.Should().NotBeNull();
        ctx.Should().BeOfType<HnVueDbContext>();
    }

    [Fact]
    public void CreateDbContext_ReturnsContextWithValidOptions()
    {
        var factory = new HnVueDbContextFactory();

        var ctx = factory.CreateDbContext(Array.Empty<string>());

        ctx.Database.Should().NotBeNull();
        ctx.Database.ProviderName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CreateDbContext_WithArgs_ReturnsNonNullContext()
    {
        var factory = new HnVueDbContextFactory();

        var ctx = factory.CreateDbContext(new[] { "some", "args" });

        ctx.Should().NotBeNull();
    }

    [Fact]
    public void CreateDbContext_AllDbSetsAreAccessible()
    {
        var factory = new HnVueDbContextFactory();
        var ctx = factory.CreateDbContext(Array.Empty<string>());

        // Verify all DbSets are accessible (not null)
        ctx.Patients.Should().NotBeNull();
        ctx.Studies.Should().NotBeNull();
        ctx.Images.Should().NotBeNull();
        ctx.DoseRecords.Should().NotBeNull();
        ctx.Users.Should().NotBeNull();
        ctx.AuditLogs.Should().NotBeNull();
        ctx.UpdateHistories.Should().NotBeNull();
        ctx.Incidents.Should().NotBeNull();
        ctx.SystemSettings.Should().NotBeNull();
    }
}
