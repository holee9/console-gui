using HnVue.Common.Results;
using HnVue.Data.Entities;
using HnVue.Data.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Data.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="EfIncidentRepository"/> using an in-memory EF Core database.
/// REQ-COORD-003: SPEC-COORDINATOR-001 EF Core incident persistence.
/// </summary>
[Trait("Category", "Data")]
public sealed class EfIncidentRepositoryTests
{
    private static (HnVueDbContext Context, SqliteConnection Connection) CreateSqliteContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<HnVueDbContext>()
            .UseSqlite(connection)
            .Options;
        var ctx = new HnVueDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, connection);
    }

    private static IncidentEntity CreateSampleIncident(
        string incidentId = "INC-001",
        int severityValue = 3) =>
        new()
        {
            IncidentId = incidentId,
            SeverityValue = severityValue,
            Description = "Test incident",
            OccurredAtTicks = DateTimeOffset.UtcNow.Ticks,
            IsResolved = false
        };

    // ── SaveAsync ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_ValidIncident_ReturnsSuccess()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);
        var incident = CreateSampleIncident();

        // Act
        var result = await repo.SaveAsync(incident);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_NullIncident_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SaveAsync(null!));
    }

    // ── GetBySeverityAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeverityAsync_ExistingSeverity_ReturnsIncidents()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Arrange
        ctx.Incidents.Add(CreateSampleIncident("INC-001", 3));
        ctx.Incidents.Add(CreateSampleIncident("INC-002", 3));
        ctx.Incidents.Add(CreateSampleIncident("INC-003", 1));
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetBySeverityAsync(3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBySeverityAsync_NoMatchingSeverity_ReturnsEmptyList()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Arrange
        ctx.Incidents.Add(CreateSampleIncident("INC-001", 1));
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetBySeverityAsync(5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetBySeverityAsync_ReturnsOrderedByOccurredAtDescending()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Arrange
        var now = DateTimeOffset.UtcNow;
        var incident1 = CreateSampleIncident("INC-001", 3);
        incident1.OccurredAtTicks = now.AddMinutes(-10).Ticks;

        var incident2 = CreateSampleIncident("INC-002", 3);
        incident2.OccurredAtTicks = now.AddMinutes(-5).Ticks;

        var incident3 = CreateSampleIncident("INC-003", 3);
        incident3.OccurredAtTicks = now.Ticks;

        ctx.Incidents.AddRange(incident1, incident2, incident3);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.GetBySeverityAsync(3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value[0].IncidentId.Should().Be("INC-003"); // Most recent
        result.Value[1].IncidentId.Should().Be("INC-002");
        result.Value[2].IncidentId.Should().Be("INC-001");
    }

    // ── ResolveAsync ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_ExistingIncident_ResolvesSuccessfully()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Arrange
        var incident = CreateSampleIncident();
        incident.IsResolved = false;
        ctx.Incidents.Add(incident);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.ResolveAsync("INC-001", "Fixed by user");

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify resolution
        var resolved = await ctx.Incidents.FindAsync("INC-001");
        resolved.Should().NotBeNull();
        resolved!.IsResolved.Should().BeTrue();
        resolved.Resolution.Should().Be("Fixed by user");
        resolved.ResolvedAtTicks.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ResolveAsync_NonExistingIncident_ReturnsNotFound()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Act
        var result = await repo.ResolveAsync("NON-EXISTING", "Fixed");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task ResolveAsync_AlreadyResolved_ReturnsValidationFailed()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Arrange
        var incident = CreateSampleIncident();
        incident.IsResolved = true;
        incident.Resolution = "Already fixed";
        ctx.Incidents.Add(incident);
        await ctx.SaveChangesAsync();

        // Act
        var result = await repo.ResolveAsync("INC-001", "Fixed again");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ResolveAsync_NullIncidentId_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.ResolveAsync(null!, "resolution"));
    }

    [Fact]
    public async Task ResolveAsync_NullResolution_ThrowsArgumentNullException()
    {
        var (ctx, connection) = CreateSqliteContext();
        await using var _ctx = ctx;
        await using var _conn = connection;
        var repo = new EfIncidentRepository(ctx);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.ResolveAsync("INC-001", null!));
    }
}
