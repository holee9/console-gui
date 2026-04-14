using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Data;
using HnVue.Incident.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Incident.Tests;

/// <summary>
/// Tests for EfIncidentRepository — EF Core persistence layer.
/// Uses SQLite in-memory for transaction support.
/// Safety-Critical: Dose interlock and incident traceability depend on this layer.
/// </summary>
[Trait("SWR", "SWR-IN-001")]
public sealed class EfIncidentRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public EfIncidentRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose()
    {
        _connection.Close();
        _connection.Dispose();
    }

    private HnVueDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(_connection)
            .Options;
        var context = new HnVueDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static IncidentRecord CreateRecord(
        string id = "inc-001",
        IncidentSeverity severity = IncidentSeverity.High,
        bool resolved = false)
    {
        return new IncidentRecord(
            IncidentId: id,
            OccurredAt: new DateTimeOffset(2026, 4, 14, 10, 30, 0, TimeSpan.FromHours(9)),
            ReportedByUserId: "user-1",
            Severity: severity,
            Category: "DOSE_EXCEEDED",
            Description: "Test incident description",
            Resolution: resolved ? "Fixed" : null,
            IsResolved: resolved,
            ResolvedAt: resolved ? new DateTimeOffset(2026, 4, 14, 12, 0, 0, TimeSpan.FromHours(9)) : null,
            ResolvedByUserId: resolved ? "admin-1" : null);
    }

    // ── SaveAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidRecord_SucceedsAndPersists()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        var record = CreateRecord();

        var result = await repo.SaveAsync(record, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var entity = await context.Incidents.FindAsync("inc-001");
        entity.Should().NotBeNull();
        entity!.IncidentId.Should().Be("inc-001");
        entity.Category.Should().Be("DOSE_EXCEEDED");
        entity.SeverityValue.Should().Be((int)IncidentSeverity.High);
        entity.ReportedByUserId.Should().Be("user-1");
        entity.Description.Should().Be("Test incident description");
        entity.IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task SaveAsync_NullRecord_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var act = async () => await repo.SaveAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SaveAsync_CriticalSeverity_PersistsCorrectSeverity()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        var record = CreateRecord(severity: IncidentSeverity.Critical);

        var result = await repo.SaveAsync(record, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var entity = await context.Incidents.FindAsync("inc-001");
        entity!.SeverityValue.Should().Be((int)IncidentSeverity.Critical);
    }

    [Fact]
    public async Task SaveAsync_ResolvedRecord_PersistsResolutionFields()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        var record = CreateRecord(resolved: true);

        var result = await repo.SaveAsync(record, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var entity = await context.Incidents.FindAsync("inc-001");
        entity!.IsResolved.Should().BeTrue();
        entity.Resolution.Should().Be("Fixed");
        entity.ResolvedAtTicks.Should().NotBeNull();
        entity.ResolvedAtOffsetMinutes.Should().NotBeNull();
        entity.ResolvedByUserId.Should().Be("admin-1");
    }

    [Fact]
    public async Task SaveAsync_PreservesUtcOffset()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        var offset = TimeSpan.FromHours(5); // UTC+5
        var record = new IncidentRecord(
            "inc-offset", new DateTimeOffset(2026, 1, 1, 0, 0, 0, offset),
            "user1", IncidentSeverity.Low, "CAT", "desc", null, false, null, null);

        await repo.SaveAsync(record, CancellationToken.None);

        var entity = await context.Incidents.FindAsync("inc-offset");
        entity!.OccurredAtOffsetMinutes.Should().Be(300); // 5 * 60
    }

    [Fact]
    public async Task SaveAsync_MultipleRecords_AllPersist()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        await repo.SaveAsync(CreateRecord("inc-1"), CancellationToken.None);
        await repo.SaveAsync(CreateRecord("inc-2", severity: IncidentSeverity.Low), CancellationToken.None);
        await repo.SaveAsync(CreateRecord("inc-3", severity: IncidentSeverity.Critical), CancellationToken.None);

        context.Incidents.Should().HaveCount(3);
    }

    [Fact]
    public async Task SaveAsync_DuplicateId_ReturnsDatabaseError()
    {
        var record = CreateRecord();
        await using (var ctx1 = CreateContext())
        {
            var repo1 = new EfIncidentRepository(ctx1);
            await repo1.SaveAsync(record, CancellationToken.None);
        }

        await using var ctx2 = CreateContext();
        var repo2 = new EfIncidentRepository(ctx2);
        var result = await repo2.SaveAsync(record, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public async Task SaveAsync_AllSeverityLevels_PersistCorrectly()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        foreach (IncidentSeverity severity in Enum.GetValues<IncidentSeverity>())
        {
            var record = CreateRecord(id: $"inc-{severity}", severity: severity);
            var result = await repo.SaveAsync(record, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        context.Incidents.Should().HaveCount(4);
    }

    // ── GetBySeverityAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeverityAsync_ReturnsOnlyMatchingSeverity()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord("inc-1", severity: IncidentSeverity.High), CancellationToken.None);
        await repo.SaveAsync(CreateRecord("inc-2", severity: IncidentSeverity.Low), CancellationToken.None);
        await repo.SaveAsync(CreateRecord("inc-3", severity: IncidentSeverity.High), CancellationToken.None);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.High, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(r => r.Severity == IncidentSeverity.High);
    }

    [Fact]
    public async Task GetBySeverityAsync_NoMatches_ReturnsEmptyList()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord("inc-1", severity: IncidentSeverity.Low), CancellationToken.None);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.Critical, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeverityAsync_EmptyDatabase_ReturnsEmptyList()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.High, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeverityAsync_ReturnsNewestFirst()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var oldRecord = new IncidentRecord(
            "inc-old", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            "u1", IncidentSeverity.High, "CAT", "old", null, false, null, null);
        var newRecord = new IncidentRecord(
            "inc-new", new DateTimeOffset(2026, 4, 14, 0, 0, 0, TimeSpan.Zero),
            "u1", IncidentSeverity.High, "CAT", "new", null, false, null, null);

        await repo.SaveAsync(oldRecord, CancellationToken.None);
        await repo.SaveAsync(newRecord, CancellationToken.None);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.High, CancellationToken.None);

        result.Value[0].IncidentId.Should().Be("inc-new");
        result.Value[1].IncidentId.Should().Be("inc-old");
    }

    [Fact]
    public async Task GetBySeverityAsync_MapsEntityToRecordCorrectly()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        var original = CreateRecord();
        await repo.SaveAsync(original, CancellationToken.None);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.High, CancellationToken.None);

        result.Value.Should().HaveCount(1);
        var mapped = result.Value[0];
        mapped.IncidentId.Should().Be(original.IncidentId);
        mapped.ReportedByUserId.Should().Be(original.ReportedByUserId);
        mapped.Category.Should().Be(original.Category);
        mapped.Description.Should().Be(original.Description);
        mapped.IsResolved.Should().Be(original.IsResolved);
    }

    [Fact]
    public async Task GetBySeverityAsync_CriticalSeverity_ReturnsCriticalRecords()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord("inc-crit", severity: IncidentSeverity.Critical), CancellationToken.None);

        var result = await repo.GetBySeverityAsync(IncidentSeverity.Critical, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Severity.Should().Be(IncidentSeverity.Critical);
    }

    // ── ResolveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_ExistingOpenIncident_Succeeds()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord(), CancellationToken.None);

        var result = await repo.ResolveAsync("inc-001", "Resolved by admin", CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var entity = await context.Incidents.FindAsync("inc-001");
        entity!.IsResolved.Should().BeTrue();
        entity.Resolution.Should().Be("Resolved by admin");
        entity.ResolvedAtTicks.Should().NotBeNull();
    }

    [Fact]
    public async Task ResolveAsync_NonExistentIncident_ReturnsNotFound()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var result = await repo.ResolveAsync("nonexistent", "resolution", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ResolveAsync_AlreadyResolved_ReturnsValidationFailed()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord(), CancellationToken.None);

        await repo.ResolveAsync("inc-001", "First resolution", CancellationToken.None);
        var result = await repo.ResolveAsync("inc-001", "Second resolution", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ResolveAsync_NullIncidentId_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var act = async () => await repo.ResolveAsync(null!, "resolution", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolveAsync_NullResolution_ThrowsArgumentNullException()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var act = async () => await repo.ResolveAsync("inc-001", null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolveAsync_SetsResolvedAtToCurrentTime()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord(), CancellationToken.None);
        var before = DateTimeOffset.UtcNow;

        await repo.ResolveAsync("inc-001", "Fixed", CancellationToken.None);

        var after = DateTimeOffset.UtcNow;
        var entity = await context.Incidents.FindAsync("inc-001");
        var resolvedAt = new DateTimeOffset(entity!.ResolvedAtTicks!.Value, TimeSpan.FromMinutes(entity.ResolvedAtOffsetMinutes!.Value));
        resolvedAt.Should().BeOnOrAfter(before);
        resolvedAt.Should().BeOnOrBefore(after);
    }

    // ── End-to-end scenarios ──────────────────────────────────────────────────

    [Fact]
    public async Task FullLifecycle_SaveGetBySeverityResolve_Succeeds()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        // Save
        var record = CreateRecord(id: "lifecycle-1", severity: IncidentSeverity.Critical);
        var saveResult = await repo.SaveAsync(record, CancellationToken.None);
        saveResult.IsSuccess.Should().BeTrue();

        // Get by severity
        var getResult = await repo.GetBySeverityAsync(IncidentSeverity.Critical, CancellationToken.None);
        getResult.IsSuccess.Should().BeTrue();
        getResult.Value.Should().HaveCount(1);
        getResult.Value[0].IsResolved.Should().BeFalse();

        // Resolve
        var resolveResult = await repo.ResolveAsync("lifecycle-1", "Root cause fixed", CancellationToken.None);
        resolveResult.IsSuccess.Should().BeTrue();

        // Verify resolved
        var afterResolve = await repo.GetBySeverityAsync(IncidentSeverity.Critical, CancellationToken.None);
        afterResolve.Value[0].IsResolved.Should().BeTrue();
        afterResolve.Value[0].Resolution.Should().Be("Root cause fixed");
    }

    [Fact]
    public async Task MultipleSeverities_GetBySeverityReturnsCorrectSubset()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var severities = new[]
        {
            IncidentSeverity.Critical, IncidentSeverity.High,
            IncidentSeverity.Medium, IncidentSeverity.Low,
            IncidentSeverity.High, IncidentSeverity.Critical
        };

        for (int i = 0; i < severities.Length; i++)
        {
            await repo.SaveAsync(
                CreateRecord(id: $"inc-{i}", severity: severities[i]),
                CancellationToken.None);
        }

        var critical = await repo.GetBySeverityAsync(IncidentSeverity.Critical, CancellationToken.None);
        var high = await repo.GetBySeverityAsync(IncidentSeverity.High, CancellationToken.None);
        var medium = await repo.GetBySeverityAsync(IncidentSeverity.Medium, CancellationToken.None);
        var low = await repo.GetBySeverityAsync(IncidentSeverity.Low, CancellationToken.None);

        critical.Value.Should().HaveCount(2);
        high.Value.Should().HaveCount(2);
        medium.Value.Should().HaveCount(1);
        low.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task ResolveAsync_DoesNotAffectOtherIncidents()
    {
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);
        await repo.SaveAsync(CreateRecord(id: "inc-a"), CancellationToken.None);
        await repo.SaveAsync(CreateRecord(id: "inc-b"), CancellationToken.None);

        await repo.ResolveAsync("inc-a", "Fixed A", CancellationToken.None);

        var entityA = await context.Incidents.FindAsync("inc-a");
        var entityB = await context.Incidents.FindAsync("inc-b");
        entityA!.IsResolved.Should().BeTrue();
        entityB!.IsResolved.Should().BeFalse();
    }
}
