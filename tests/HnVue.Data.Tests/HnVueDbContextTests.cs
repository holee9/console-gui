using HnVue.Data.Entities;

namespace HnVue.Data.Tests;

/// <summary>
/// Basic smoke tests for <see cref="HnVueDbContext"/> configuration.
/// </summary>
public sealed class HnVueDbContextTests
{
    [Fact]
    public void Create_InMemoryContext_HasAllDbSets()
    {
        using var ctx = TestDbContextFactory.Create();

        ctx.Patients.Should().NotBeNull();
        ctx.Studies.Should().NotBeNull();
        ctx.Images.Should().NotBeNull();
        ctx.DoseRecords.Should().NotBeNull();
        ctx.Users.Should().NotBeNull();
        ctx.AuditLogs.Should().NotBeNull();
    }

    [Fact]
    public void Create_MultipleContexts_AreIsolated()
    {
        using var ctx1 = TestDbContextFactory.Create();
        using var ctx2 = TestDbContextFactory.Create();

        ctx1.Patients.Add(new Entities.PatientEntity
        {
            PatientId = "P999",
            Name = "Test^Patient",
            IsEmergency = false,
            CreatedAtTicks = DateTimeOffset.UtcNow.UtcTicks,
            CreatedAtOffsetMinutes = 0,
            CreatedBy = "test",
        });
        ctx1.SaveChanges();

        // ctx2 should not see changes from ctx1 (different in-memory databases)
        ctx2.Patients.Any(p => p.PatientId == "P999").Should().BeFalse();
    }
}
