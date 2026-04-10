using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident;
using HnVue.Incident.Models;
using Xunit;

namespace HnVue.Incident.Tests;

public sealed class IncidentRepositoryTests
{
    private static IncidentRecord MakeRecord(
        IncidentSeverity severity = IncidentSeverity.Medium,
        DateTimeOffset? occurredAt = null)
        => new(
            IncidentId: Guid.NewGuid().ToString(),
            OccurredAt: occurredAt ?? DateTimeOffset.UtcNow,
            ReportedByUserId: "user-01",
            Severity: severity,
            Category: "SOFTWARE_ERROR",
            Description: "Test incident",
            Resolution: null,
            IsResolved: false,
            ResolvedAt: null,
            ResolvedByUserId: null);

    // ── AddAsync / GetByIdAsync ───────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NewRecord_ReturnsSuccessWithRecord()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();

        var result = await repo.AddAsync(record, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(record);
    }

    [Fact]
    public async Task AddAsync_DuplicateId_ReturnsAlreadyExists()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();
        await repo.AddAsync(record, default);

        var result = await repo.AddAsync(record, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AlreadyExists);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRecord()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();
        await repo.AddAsync(record, default);

        var result = await repo.GetByIdAsync(record.IncidentId, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IncidentId.Should().Be(record.IncidentId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNotFound()
    {
        var repo = new IncidentRepository();

        var result = await repo.GetByIdAsync("non-existing-id", default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── QueryAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_NoFilter_ReturnsAllRecords()
    {
        var repo = new IncidentRepository();
        var r1 = MakeRecord(IncidentSeverity.Critical);
        var r2 = MakeRecord(IncidentSeverity.Low);
        var r3 = MakeRecord(IncidentSeverity.Medium);
        await repo.AddAsync(r1, default);
        await repo.AddAsync(r2, default);
        await repo.AddAsync(r3, default);

        var result = await repo.QueryAsync(null, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
    }

    [Fact]
    public async Task QueryAsync_BySeverity_FiltersCorrectly()
    {
        var repo = new IncidentRepository();
        await repo.AddAsync(MakeRecord(IncidentSeverity.Critical), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Critical), default);
        await repo.AddAsync(MakeRecord(IncidentSeverity.Low), default);

        var result = await repo.QueryAsync(IncidentSeverity.Critical, null, null, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(r => r.Severity == IncidentSeverity.Critical);
    }

    [Fact]
    public async Task QueryAsync_ByDateRange_FiltersCorrectly()
    {
        var repo = new IncidentRepository();
        var baseTime = new DateTimeOffset(2026, 1, 10, 0, 0, 0, TimeSpan.Zero);
        await repo.AddAsync(MakeRecord(occurredAt: baseTime.AddDays(-1)), default);  // out
        await repo.AddAsync(MakeRecord(occurredAt: baseTime), default);               // in
        await repo.AddAsync(MakeRecord(occurredAt: baseTime.AddDays(1)), default);   // in
        await repo.AddAsync(MakeRecord(occurredAt: baseTime.AddDays(5)), default);   // out

        var result = await repo.QueryAsync(
            null,
            from: baseTime,
            to: baseTime.AddDays(2),
            default);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryAsync_ResultsOrderedByOccurredAt()
    {
        var repo = new IncidentRepository();
        var now = DateTimeOffset.UtcNow;
        var r1 = MakeRecord(occurredAt: now.AddHours(-2));
        var r2 = MakeRecord(occurredAt: now.AddHours(-1));
        var r3 = MakeRecord(occurredAt: now);
        // Add in reverse order
        await repo.AddAsync(r3, default);
        await repo.AddAsync(r1, default);
        await repo.AddAsync(r2, default);

        var result = await repo.QueryAsync(null, null, null, default);

        result.Value.Select(r => r.IncidentId).Should().ContainInOrder(r1.IncidentId, r2.IncidentId, r3.IncidentId);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingRecord_ReturnsUpdated()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();
        await repo.AddAsync(record, default);

        var updated = record with { IsResolved = true, Resolution = "Fixed" };
        var result = await repo.UpdateAsync(updated, default);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsResolved.Should().BeTrue();
        result.Value.Resolution.Should().Be("Fixed");

        // Verify persistence
        var getResult = await repo.GetByIdAsync(record.IncidentId, default);
        getResult.Value.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsNotFound()
    {
        var repo = new IncidentRepository();
        var record = MakeRecord();

        var result = await repo.UpdateAsync(record, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── Cancellation ──────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        var repo = new IncidentRepository();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await repo.AddAsync(MakeRecord(), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── Null Guard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_NullRecord_ThrowsArgumentNullException()
    {
        var repo = new IncidentRepository();

        var act = async () => await repo.AddAsync(null!, default);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_NullRecord_ThrowsArgumentNullException()
    {
        var repo = new IncidentRepository();

        var act = async () => await repo.UpdateAsync(null!, default);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
