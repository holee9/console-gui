using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Incident;
using HnVue.Incident.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace HnVue.Incident.Tests;

/// <summary>
/// Integration-style unit tests for <see cref="IncidentService"/>.
/// Uses a real <see cref="IncidentRepository"/> (in-memory) and a mocked <see cref="IAuditService"/>.
/// </summary>
public sealed class IncidentServiceTests
{
    private readonly IAuditService _auditService;
    private readonly IncidentService _sut;

    public IncidentServiceTests()
    {
        _auditService = Substitute.For<IAuditService>();
        _auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        _auditService.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
                     .Returns(Result.Success(true));

        var repo = new IncidentRepository();
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        _sut = new IncidentService(repo, _auditService, notificationService, NullLogger<IncidentService>.Instance);
    }

    // ── ReportAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ReportAsync_ValidInput_ReturnsSuccessWithRecord()
    {
        var result = await _sut.ReportAsync("user-01", IncidentSeverity.Medium, "SOFTWARE_ERROR", "Test error");

        result.IsSuccess.Should().BeTrue();
        result.Value.IncidentId.Should().NotBeNullOrEmpty();
        result.Value.ReportedByUserId.Should().Be("user-01");
        result.Value.Severity.Should().Be(IncidentSeverity.Medium);
        result.Value.Category.Should().Be("SOFTWARE_ERROR");
        result.Value.Description.Should().Be("Test error");
        result.Value.IsResolved.Should().BeFalse();
        result.Value.OccurredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ReportAsync_WritesAuditEntry()
    {
        await _sut.ReportAsync("user-01", IncidentSeverity.Low, "HARDWARE_FAULT", "Sensor offline");

        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e => e.Action == "INCIDENT_REPORTED" && e.UserId == "user-01"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportAsync_CriticalSeverity_WritesAuditWithCriticalFlag()
    {
        await _sut.ReportAsync("user-02", IncidentSeverity.Critical, "DOSE_EXCEEDED", "Dose threshold breached");

        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e =>
                e.Action == "INCIDENT_REPORTED" &&
                e.Details != null &&
                e.Details.Contains("CRITICAL_INCIDENT")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportAsync_NonCriticalSeverity_DoesNotIncludeCriticalFlag()
    {
        await _sut.ReportAsync("user-01", IncidentSeverity.High, "HARDWARE_FAULT", "Fan speed low");

        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e =>
                e.Details != null &&
                !e.Details.Contains("CRITICAL_INCIDENT")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReportAsync_AuditServiceFails_StillReturnsSuccess()
    {
        _auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Failure(ErrorCode.IncidentLogFailed, "Audit backend unavailable"));

        // Incident must be recorded even when audit write fails (audit failure is non-fatal).
        var result = await _sut.ReportAsync("user-03", IncidentSeverity.Medium, "SOFTWARE_ERROR", "Edge case");

        result.IsSuccess.Should().BeTrue("incident store must succeed even if audit write fails");
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRecord()
    {
        var reported = (await _sut.ReportAsync("user-01", IncidentSeverity.Low, "LOW_CAT", "desc")).Value;

        var result = await _sut.GetByIdAsync(reported.IncidentId);

        result.IsSuccess.Should().BeTrue();
        result.Value.IncidentId.Should().Be(reported.IncidentId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNotFound()
    {
        var result = await _sut.GetByIdAsync("does-not-exist");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── ListAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task ListAsync_NoFilter_ReturnsAllIncidents()
    {
        await _sut.ReportAsync("u1", IncidentSeverity.Critical, "CAT_A", "d1");
        await _sut.ReportAsync("u1", IncidentSeverity.High, "CAT_B", "d2");
        await _sut.ReportAsync("u1", IncidentSeverity.Low, "CAT_C", "d3");

        var result = await _sut.ListAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ListAsync_BySeverity_FiltersCorrectly()
    {
        // Use a fresh IncidentService instance to isolate state.
        var sut = BuildFreshService();
        await sut.ReportAsync("u1", IncidentSeverity.Critical, "CAT", "d");
        await sut.ReportAsync("u1", IncidentSeverity.Critical, "CAT", "d");
        await sut.ReportAsync("u1", IncidentSeverity.Low, "CAT", "d");

        var result = await sut.ListAsync(severityFilter: IncidentSeverity.Critical);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(r => r.Severity == IncidentSeverity.Critical);
    }

    [Fact]
    public async Task ListAsync_ByDateRange_FiltersCorrectly()
    {
        var sut = BuildFreshService();
        var cutoff = DateTimeOffset.UtcNow;

        // We cannot control OccurredAt directly because it is set internally, so we
        // verify that reported records are included when the range covers UtcNow.
        await sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "d1");
        await sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "d2");

        var result = await sut.ListAsync(
            from: cutoff.AddMinutes(-1),
            toDate: cutoff.AddMinutes(1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListAsync_FromInFuture_ReturnsEmpty()
    {
        var sut = BuildFreshService();
        await sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "old");

        var result = await sut.ListAsync(from: DateTimeOffset.UtcNow.AddDays(1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── ResolveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveAsync_OpenIncident_ReturnsResolvedRecord()
    {
        var reported = (await _sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "desc")).Value;

        var result = await _sut.ResolveAsync(reported.IncidentId, "admin", "Rebooted system");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsResolved.Should().BeTrue();
        result.Value.Resolution.Should().Be("Rebooted system");
        result.Value.ResolvedByUserId.Should().Be("admin");
        result.Value.ResolvedAt.Should().HaveValue();
    }

    [Fact]
    public async Task ResolveAsync_OpenIncident_WritesResolvedAuditEntry()
    {
        var reported = (await _sut.ReportAsync("u1", IncidentSeverity.High, "CAT", "desc")).Value;
        _auditService.ClearReceivedCalls();

        await _sut.ResolveAsync(reported.IncidentId, "admin", "Fixed hardware");

        await _auditService.Received(1).WriteAuditAsync(
            Arg.Is<AuditEntry>(e =>
                e.Action == "INCIDENT_RESOLVED" &&
                e.UserId == "admin"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ResolveAsync_AlreadyResolved_ReturnsValidationFailed()
    {
        var reported = (await _sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "desc")).Value;
        await _sut.ResolveAsync(reported.IncidentId, "admin", "First resolution");

        var result = await _sut.ResolveAsync(reported.IncidentId, "admin2", "Second resolution");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task ResolveAsync_NonExistingId_ReturnsNotFound()
    {
        var result = await _sut.ResolveAsync("non-existing", "admin", "resolution");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── VerifyAuditIntegrityAsync ─────────────────────────────────────────────

    [Fact]
    public async Task VerifyAuditIntegrityAsync_DelegatesToAuditService()
    {
        _auditService.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
                     .Returns(Result.Success(true));

        var result = await _sut.VerifyAuditIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        await _auditService.Received(1).VerifyChainIntegrityAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyAuditIntegrityAsync_AuditServiceReturnsFalse_ReturnsFalse()
    {
        _auditService.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
                     .Returns(Result.Success(false));

        var result = await _sut.VerifyAuditIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAuditIntegrityAsync_AuditServiceFails_PropagatesFailure()
    {
        _auditService.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
                     .Returns(Result.Failure<bool>(ErrorCode.IncidentLogFailed, "DB unavailable"));

        var result = await _sut.VerifyAuditIntegrityAsync();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    [Fact]
    public async Task ResolveAsync_AuditServiceFails_StillReturnsSuccess()
    {
        var reported = (await _sut.ReportAsync("u1", IncidentSeverity.High, "CAT", "desc")).Value;
        _auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Failure(ErrorCode.IncidentLogFailed, "Audit backend down"));

        var result = await _sut.ResolveAsync(reported.IncidentId, "admin", "Fixed");

        result.IsSuccess.Should().BeTrue("resolve must succeed even when audit write fails");
        result.Value.IsResolved.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_CancelledToken_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _sut.GetByIdAsync("any-id", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── Concurrency ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ConcurrentReports_AllSucceed()
    {
        var sut = BuildFreshService();
        const int count = 50;

        var tasks = Enumerable.Range(0, count)
            .Select(i => sut.ReportAsync($"user-{i}", IncidentSeverity.Low, "CAT", $"desc-{i}"))
            .ToList();

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r.IsSuccess);
        var list = await sut.ListAsync();
        list.Value.Should().HaveCount(count);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IIncidentService BuildFreshService()
    {
        var audit = Substitute.For<IAuditService>();
        audit.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
             .Returns(Result.Success());
        audit.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
             .Returns(Result.Success(true));

        var repo = new IncidentRepository();
        var notify = new NotificationService(NullLogger<NotificationService>.Instance);
        return new IncidentService(repo, audit, notify, NullLogger<IncidentService>.Instance);
    }
}
