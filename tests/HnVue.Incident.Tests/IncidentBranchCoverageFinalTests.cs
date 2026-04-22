// <copyright file="IncidentBranchCoverageFinalTests.cs" company="ABYZ">
// Copyright (c) ABYZ. All rights reserved.
// </copyright>

using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Data;
using HnVue.Incident;
using HnVue.Incident.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HnVue.Incident.Tests;

/// <summary>
/// Targeted tests to reach 90%+ branch coverage for the Incident module.
/// Covers the catch(DbUpdateException) branch in EfIncidentRepository.ResolveAsync.
/// Safety-Critical: Incident module requires 90%+ branch coverage (DOC-012).
/// </summary>
[Trait("SWR", "SWR-IN-001")]
public sealed class IncidentBranchCoverageFinalTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public IncidentBranchCoverageFinalTests()
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

    // ── EfIncidentRepository: ResolveAsync catch(DbUpdateException) path (line 111-113) ──
    // Strategy: Load entity, set a NOT NULL field to null, then call ResolveAsync.
    // When SaveChangesAsync runs, SQLite rejects the null value in the NOT NULL column,
    // EF Core wraps it as DbUpdateException, and the catch block at line 111-113 is hit.

    [Fact]
    public async Task EfRepo_ResolveAsync_NullRequiredField_CoversDbUpdateExceptionCatch()
    {
        // Save a record
        const string id = "inc-resolve-nullfield";
        await using (var ctxSetup = CreateContext())
        {
            var repoSetup = new EfIncidentRepository(ctxSetup);
            await repoSetup.SaveAsync(
                new IncidentRecord(id, DateTimeOffset.UtcNow, "u1",
                    IncidentSeverity.Medium, "CAT", "desc", null, false, null, null),
                CancellationToken.None);
        }

        // Use the context directly to load the entity and set a required field to null
        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        // Load the entity to get it tracked
        var entity = await context.Incidents.FindAsync(id);
        entity.Should().NotBeNull("entity must exist after save");

        // Set ReportedByUserId to null - violates the [Required] / NOT NULL constraint
        entity!.ReportedByUserId = null!;

        // Mark as modified so EF Core includes it in SaveChanges
        context.Entry(entity).State = EntityState.Modified;

        // Now call ResolveAsync - it will attempt SaveChangesAsync which fails
        // because ReportedByUserId is NULL on a NOT NULL column.
        // This triggers catch(DbUpdateException) at line 111-113.
        var result = await repo.ResolveAsync(id, "Fixed", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    // ── Additional variant: Description field NOT NULL violation ──

    [Fact]
    public async Task EfRepo_ResolveAsync_NullDescription_CoversDbUpdateExceptionCatch()
    {
        // Save a record
        const string id = "inc-resolve-nulldesc";
        await using (var ctxSetup = CreateContext())
        {
            var repoSetup = new EfIncidentRepository(ctxSetup);
            await repoSetup.SaveAsync(
                new IncidentRecord(id, DateTimeOffset.UtcNow, "u1",
                    IncidentSeverity.High, "CAT", "desc", null, false, null, null),
                CancellationToken.None);
        }

        await using var context = CreateContext();
        var repo = new EfIncidentRepository(context);

        var entity = await context.Incidents.FindAsync(id);
        entity.Should().NotBeNull();

        // Set Description to null - also violates [Required] constraint
        entity!.Description = null!;
        context.Entry(entity).State = EntityState.Modified;

        var result = await repo.ResolveAsync(id, "Resolution text", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}
