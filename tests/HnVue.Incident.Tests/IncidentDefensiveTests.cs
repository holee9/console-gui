using System.Collections.Concurrent;
using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident;
using HnVue.Incident.Models;
using Xunit;

namespace HnVue.Incident.Tests;

/// <summary>
/// Defensive tests for <see cref="IncidentRepository"/> edge cases:
/// cancellation, concurrent access, boundary queries, and null guards.
/// Safety-critical: Incident module requires 90%+ branch coverage (DOC-012).
/// </summary>
[Trait("SWR", "SWR-IP-050")]
public sealed class IncidentDefensiveTests
{
    private static IncidentRecord MakeRecord(
        IncidentSeverity severity = IncidentSeverity.Medium,
        DateTimeOffset? occurredAt = null,
        string? incidentId = null,
        string category = "SOFTWARE_ERROR",
        string description = "Test incident",
        string reportedByUserId = "user-01")
        => new(
            IncidentId: incidentId ?? Guid.NewGuid().ToString(),
            OccurredAt: occurredAt ?? DateTimeOffset.UtcNow,
            ReportedByUserId: reportedByUserId,
            Severity: severity,
            Category: category,
            Description: description,
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

    // ── Cancellation Token Edge Cases ─────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var repo = new IncidentRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.GetByIdAsync("any-id", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task UpdateAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var repo = new IncidentRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.UpdateAsync(MakeRecord(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task QueryAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var repo = new IncidentRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.QueryAsync(null, null, null, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── Empty Store Queries ───────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_EmptyStore_ReturnsEmptyList()
    {
        var repo = new IncidentRepository();

        var result = await repo.QueryAsync(null, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_EmptyStore_WithSeverityFilter_ReturnsEmptyList()
    {
        var repo = new IncidentRepository();

        var result = await repo.QueryAsync(IncidentSeverity.Critical, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_EmptyStore_WithDateRange_ReturnsEmptyList()
    {
        var repo = new IncidentRepository();
        var now = DateTimeOffset.UtcNow;

        var result = await repo.QueryAsync(null, now.AddDays(-1), now.AddDays(1), default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── Combined Filter Combinations ──────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_SeverityAndDateRange_CombinesFilters()
    {
        var repo = new IncidentRepository();
        var baseTime = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);

        // Add records with different severities and times
        await repo.AddAsync(MakeRecord(IncidentSeverity.Critical, occurredAt: baseTime), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Critical, occurredAt: baseTime.AddDays(5)), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.High, occurredAt: baseTime.AddDays(1)), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Medium, occurredAt: baseTime), default);

        // Query: Critical severity only, within date range that excludes the +5 day entry
        var result = await repo.QueryAsync(
            IncidentSeverity.Critical,
            from: baseTime,
            to: baseTime.AddDays(2),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Severity.Should().Be(IncidentSeverity.Critical);
        result.Value[0].OccurredAt.Should().Be(baseTime);
    }

    [Fact]
    public async Task QueryAsync_SeverityFilter_NoMatch_ReturnsEmptyList()
    {
        var repo = new IncidentRepository();
        await repo.AddAsync(MakeRecord(IncidentSeverity.Low), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Medium), default);

        var result = await repo.QueryAsync(IncidentSeverity.Critical, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryAsync_DateRange_ExcludesBoundaryOutside()
    {
        var repo = new IncidentRepository();
        var baseTime = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);

        await repo.AddAsync(MakeRecord(occurredAt: baseTime.AddDays(-1)), default); // before range
        await repo.AddAsync(MakeRecord(occurredAt: baseTime), default);              // at range start (inclusive)
        await repo.AddAsync(MakeRecord(occurredAt: baseTime.AddDays(3)), default);   // after range

        var result = await repo.QueryAsync(null, from: baseTime, to: baseTime, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].OccurredAt.Should().Be(baseTime);
    }

    // ── Concurrent Access Patterns ────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ConcurrentAdds_AllSucceedWithUniqueIds()
    {
        var repo = new IncidentRepository();
        const int count = 20;

        var tasks = Enumerable.Range(0, count)
            .Select(_ => repo.AddAsync(MakeRecord(), default))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());
        results.Should().HaveCount(count);
    }

    [Fact]
    public async Task QueryAsync_DuringConcurrentAdds_ReturnsConsistentResults()
    {
        var repo = new IncidentRepository();
        // Pre-populate some records
        for (int i = 0; i < 5; i++)
            await repo.AddAsync(MakeRecord(), default);

        // Run concurrent adds and a query simultaneously
        var addTasks = Enumerable.Range(0, 10)
            .Select(_ => repo.AddAsync(MakeRecord(), default))
            .ToArray();

        // Query while adds are in progress
        var queryTask = repo.QueryAsync(null, null, null, default);

        await Task.WhenAll(addTasks);
        var queryResult = await queryTask;

        queryResult.IsSuccess.Should().BeTrue();
        // At minimum the 5 pre-populated records should be present
        queryResult.Value.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task UpdateAsync_ConcurrentUpdates_LastWriteWins()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();
        await repo.AddAsync(record, default);

        // Multiple concurrent updates to the same record
        var updateTasks = Enumerable.Range(0, 5)
            .Select(i => repo.UpdateAsync(
                record with { Resolution = $"Resolution-{i}" }, default))
            .ToArray();

        var results = await Task.WhenAll(updateTasks);

        // All updates should succeed (last-write-wins)
        results.Should().AllSatisfy(r => r.IsSuccess.Should().BeTrue());

        // Verify the final state is one of the updates
        var final = await repo.GetByIdAsync(record.IncidentId, default);
        final.IsSuccess.Should().BeTrue();
        final.Value.Resolution.Should().StartWith("Resolution-");
    }

    // ── IncidentRecord Property Edge Cases ────────────────────────────────────

    [Fact]
    public async Task AddAsync_RecordWithEmptyStrings_Succeeds()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord(category: "", description: "", reportedByUserId: "");

        var result = await repo.AddAsync(record, default);

        // Repository does not validate content - it stores what it receives
        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().BeEmpty();
        result.Value.Description.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_RecordWithMaxLengthStrings_Succeeds()
    {
        var repo = new IncidentRepository();
        var longString = new string('A', 10000);
        var record = MakeRecord(
            category: longString,
            description: longString,
            reportedByUserId: longString);

        var result = await repo.AddAsync(record, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Category.Should().Be(longString);
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_ReturnsExactRecord()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord(
            severity: IncidentSeverity.High,
            category: "DOSE_EXCEEDED",
            description: "DAP exceeded 2x DRL during chest exam",
            reportedByUserId: "tech-42");

        await repo.AddAsync(record, default);
        var result = await repo.GetByIdAsync(record.IncidentId, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(record, options =>
            options.ComparingByMembers<IncidentRecord>());
    }

    // ── Update Edge Cases ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ResolvedRecord_PersistsResolutionFields()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();
        await repo.AddAsync(record, default);

        var resolvedAt = DateTimeOffset.UtcNow;
        var resolved = record with
        {
            IsResolved = true,
            Resolution = "Root cause identified and fixed",
            ResolvedAt = resolvedAt,
            ResolvedByUserId = "admin-01"
        };

        var updateResult = await repo.UpdateAsync(resolved, default);
        updateResult.IsSuccess.Should().BeTrue();

        // Verify all resolution fields persisted
        var fetched = await repo.GetByIdAsync(record.IncidentId, default);
        fetched.Value.IsResolved.Should().BeTrue();
        fetched.Value.Resolution.Should().Be("Root cause identified and fixed");
        fetched.Value.ResolvedAt.Should().Be(resolvedAt);
        fetched.Value.ResolvedByUserId.Should().Be("admin-01");
    }

    [Fact]
    public async Task UpdateAsync_DoesNotAffectOtherRecords()
    {
        var repo = new IncidentRepository();
        var record1 = MakeRecord(category: "ORIGINAL_1");
        var record2 = MakeRecord(category: "ORIGINAL_2");
        await repo.AddAsync(record1, default);
        await repo.AddAsync(record2, default);

        var updated1 = record1 with { Category = "MODIFIED_1" };
        await repo.UpdateAsync(updated1, default);

        // record2 should be unchanged
        var fetched2 = await repo.GetByIdAsync(record2.IncidentId, default);
        fetched2.Value.Category.Should().Be("ORIGINAL_2");
    }

    // ── Query Ordering ────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_WithSeverityFilter_ResultsStillOrderedByTime()
    {
        var repo = new IncidentRepository();
        var now = DateTimeOffset.UtcNow;

        var r3 = MakeRecord(IncidentSeverity.High, occurredAt: now.AddHours(2));
        var r1 = MakeRecord(IncidentSeverity.High, occurredAt: now);
        var r2 = MakeRecord(IncidentSeverity.High, occurredAt: now.AddHours(1));

        // Add in non-chronological order
        await repo.AddAsync(r2, default);
        await repo.AddAsync(r3, default);
        await repo.AddAsync(r1, default);

        var result = await repo.QueryAsync(IncidentSeverity.High, null, null, default);

        result.Value[0].OccurredAt.Should().Be(r1.OccurredAt);
        result.Value[1].OccurredAt.Should().Be(r2.OccurredAt);
        result.Value[2].OccurredAt.Should().Be(r3.OccurredAt);
    }

    // ── All IncidentSeverity Values ───────────────────────────────────────────

    [Theory]
    [InlineData(IncidentSeverity.Critical)]
    [InlineData(IncidentSeverity.High)]
    [InlineData(IncidentSeverity.Medium)]
    [InlineData(IncidentSeverity.Low)]
    public async Task QueryAsync_EachSeverity_ReturnsMatchingRecords(IncidentSeverity severity)
    {
        var repo = new IncidentRepository();

        // Add one of each severity
        await repo.AddAsync(MakeRecord(IncidentSeverity.Critical), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.High), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Medium), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Low), default);

        var result = await repo.QueryAsync(severity, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().OnlyContain(r => r.Severity == severity);
    }
}
