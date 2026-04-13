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
/// Coverage boost tests for Incident module to reach 90%+ branch coverage.
/// Targets uncovered branches in IncidentService, IncidentResponseService,
/// IncidentRepository, and NotificationService.
/// </summary>
[Trait("SWR", "SWR-IN-001")]
public sealed class IncidentCoverageBoostTests
{
    // ── IncidentResponseService: RecordAsync null/whitespace validation ──────

    [Fact]
    public async Task RecordAsync_NullCategory_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        repo.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
             .Returns(Result.Success());
        var sut = new IncidentResponseService(repo);

        var act = async () => await sut.RecordAsync(
            IncidentSeverity.High, null!, "desc", "user1");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("category");
    }

    [Fact]
    public async Task RecordAsync_NullDescription_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var act = async () => await sut.RecordAsync(
            IncidentSeverity.High, "CAT", null!, "user1");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("description");
    }

    [Fact]
    public async Task RecordAsync_NullReportedByUserId_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var act = async () => await sut.RecordAsync(
            IncidentSeverity.High, "CAT", "desc", null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("reportedByUserId");
    }

    [Fact]
    public async Task RecordAsync_WhitespaceCategory_ReturnsValidationFailure()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var result = await sut.RecordAsync(
            IncidentSeverity.High, "   ", "desc", "user1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task RecordAsync_WhitespaceDescription_ReturnsValidationFailure()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var result = await sut.RecordAsync(
            IncidentSeverity.High, "CAT", "   ", "user1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task RecordAsync_RepositorySaveFails_ReturnsFailure()
    {
        var repo = Substitute.For<IIncidentRepository>();
        repo.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
             .Returns(Result.Failure(ErrorCode.DatabaseError, "DB error"));
        var sut = new IncidentResponseService(repo);

        var result = await sut.RecordAsync(
            IncidentSeverity.Medium, "CAT", "desc", "user1");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── IncidentResponseService: Critical escalation ──────────────────────────

    [Fact]
    public async Task RecordAsync_CriticalSeverity_TriggersCallback()
    {
        var repo = Substitute.For<IIncidentRepository>();
        repo.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
             .Returns(Result.Success());
        var sut = new IncidentResponseService(repo);

        IncidentRecord? callbackRecord = null;
        sut.OnCritical(r => { callbackRecord = r; return Task.CompletedTask; });

        var result = await sut.RecordAsync(
            IncidentSeverity.Critical, "CAT", "desc", "user1");

        result.IsSuccess.Should().BeTrue();
        callbackRecord.Should().NotBeNull();
        callbackRecord!.Severity.Should().Be(IncidentSeverity.Critical);
    }

    [Fact]
    public async Task RecordAsync_CriticalCallbackThrows_DoesNotFailRecord()
    {
        var repo = Substitute.For<IIncidentRepository>();
        repo.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
             .Returns(Result.Success());
        var sut = new IncidentResponseService(repo);

        sut.OnCritical(_ => throw new InvalidOperationException("Callback error"));

        var result = await sut.RecordAsync(
            IncidentSeverity.Critical, "CAT", "desc", "user1");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task RecordAsync_NonCritical_DoesNotTriggerCallback()
    {
        var repo = Substitute.For<IIncidentRepository>();
        repo.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
             .Returns(Result.Success());
        var sut = new IncidentResponseService(repo);

        var callbackInvoked = false;
        sut.OnCritical(_ => { callbackInvoked = true; return Task.CompletedTask; });

        await sut.RecordAsync(IncidentSeverity.Low, "CAT", "desc", "user1");

        callbackInvoked.Should().BeFalse();
    }

    [Fact]
    public void OnCritical_NullCallback_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var act = () => sut.OnCritical(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── IncidentResponseService: ResolveAsync null validation ─────────────────

    [Fact]
    public async Task ResolveAsync_NullIncidentId_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var act = async () => await sut.ResolveAsync(null!, "resolution");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("incidentId");
    }

    [Fact]
    public async Task ResolveAsync_NullResolution_ThrowsArgumentNullException()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var sut = new IncidentResponseService(repo);

        var act = async () => await sut.ResolveAsync("id1", null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("resolution");
    }

    // ── IncidentResponseService: GetBySeverityAsync ───────────────────────────

    [Fact]
    public async Task GetBySeverityAsync_ReturnsRepositoryResult()
    {
        var repo = Substitute.For<IIncidentRepository>();
        var incidents = new List<IncidentRecord>
        {
            new("id1", DateTimeOffset.UtcNow, "user1", IncidentSeverity.High, "CAT", "desc", null, false, null, null),
        }.AsReadOnly();
        repo.GetBySeverityAsync(IncidentSeverity.High, Arg.Any<CancellationToken>())
             .Returns(Result.Success<IReadOnlyList<IncidentRecord>>(incidents));
        var sut = new IncidentResponseService(repo);

        var result = await sut.GetBySeverityAsync(IncidentSeverity.High);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // ── IncidentRepository: AddAsync duplicate ────────────────────────────────

    [Fact]
    public async Task Repository_AddAsync_DuplicateId_ReturnsAlreadyExists()
    {
        var repo = new IncidentRepository();
        var record = new IncidentRecord(
            "dup-id", DateTimeOffset.UtcNow, "user1", IncidentSeverity.Medium,
            "CAT", "desc", null, false, null, null);

        var result1 = await repo.AddAsync(record, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();

        var result2 = await repo.AddAsync(record, CancellationToken.None);
        result2.IsFailure.Should().BeTrue();
        result2.Error.Should().Be(ErrorCode.AlreadyExists);
    }

    // ── IncidentRepository: UpdateAsync non-existent ──────────────────────────

    [Fact]
    public async Task Repository_UpdateAsync_NonExistentId_ReturnsNotFound()
    {
        var repo = new IncidentRepository();
        var record = new IncidentRecord(
            "nonexistent", DateTimeOffset.UtcNow, "user1", IncidentSeverity.Medium,
            "CAT", "desc", "resolved", true, DateTimeOffset.UtcNow, "admin");

        var result = await repo.UpdateAsync(record, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── IncidentRepository: QueryAsync filter combinations ────────────────────

    [Fact]
    public async Task Repository_QueryAsync_SeverityFilter_OnlyMatchingResults()
    {
        var repo = new IncidentRepository();
        await repo.AddAsync(new("id1", DateTimeOffset.UtcNow, "u1", IncidentSeverity.High, "C", "d", null, false, null, null), CancellationToken.None);
        await repo.AddAsync(new("id2", DateTimeOffset.UtcNow, "u1", IncidentSeverity.Low, "C", "d", null, false, null, null), CancellationToken.None);

        var result = await repo.QueryAsync(IncidentSeverity.High, null, null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Severity.Should().Be(IncidentSeverity.High);
    }

    [Fact]
    public async Task Repository_QueryAsync_DateFromFilter_OnlyAfterDate()
    {
        var repo = new IncidentRepository();
        var oldDate = DateTimeOffset.UtcNow.AddDays(-5);
        var newDate = DateTimeOffset.UtcNow;
        await repo.AddAsync(new("id1", oldDate, "u1", IncidentSeverity.Medium, "C", "d", null, false, null, null), CancellationToken.None);
        await repo.AddAsync(new("id2", newDate, "u1", IncidentSeverity.Medium, "C", "d", null, false, null, null), CancellationToken.None);

        var result = await repo.QueryAsync(null, DateTimeOffset.UtcNow.AddDays(-1), null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].IncidentId.Should().Be("id2");
    }

    [Fact]
    public async Task Repository_QueryAsync_DateToFilter_OnlyBeforeDate()
    {
        var repo = new IncidentRepository();
        var oldDate = DateTimeOffset.UtcNow.AddDays(-5);
        var newDate = DateTimeOffset.UtcNow;
        await repo.AddAsync(new("id1", oldDate, "u1", IncidentSeverity.Medium, "C", "d", null, false, null, null), CancellationToken.None);
        await repo.AddAsync(new("id2", newDate, "u1", IncidentSeverity.Medium, "C", "d", null, false, null, null), CancellationToken.None);

        var result = await repo.QueryAsync(null, null, DateTimeOffset.UtcNow.AddDays(-1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].IncidentId.Should().Be("id1");
    }

    [Fact]
    public async Task Repository_QueryAsync_AllFilters_ReturnsEmptyWhenNoMatch()
    {
        var repo = new IncidentRepository();
        await repo.AddAsync(new("id1", DateTimeOffset.UtcNow, "u1", IncidentSeverity.Medium, "C", "d", null, false, null, null), CancellationToken.None);

        var result = await repo.QueryAsync(IncidentSeverity.Critical, null, null, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    // ── IncidentRepository: GetByIdAsync ───────────────────────────────────────

    [Fact]
    public async Task Repository_GetByIdAsync_NotFound_ReturnsFailure()
    {
        var repo = new IncidentRepository();

        var result = await repo.GetByIdAsync("nonexistent", CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    // ── IncidentService (internal): ReportAsync audit failure path ────────────

    [Fact]
    public async Task Service_ReportAsync_AuditFailure_StillSucceeds()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Failure(ErrorCode.Unknown, "Audit write failed"));
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var result = await sut.ReportAsync("user1", IncidentSeverity.High, "CAT", "desc");

        result.IsSuccess.Should().BeTrue();
        result.Value.Severity.Should().Be(IncidentSeverity.High);
    }

    [Fact]
    public async Task Service_ReportAsync_Critical_HasCriticalAuditMarker()
    {
        AuditEntry? capturedAudit = null;
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Do<AuditEntry>(e => capturedAudit = e), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        await sut.ReportAsync("user1", IncidentSeverity.Critical, "CAT", "desc");

        capturedAudit.Should().NotBeNull();
        capturedAudit!.Details.Should().Contain("CRITICAL_INCIDENT");
    }

    [Fact]
    public async Task Service_ReportAsync_NonCritical_NoCriticalAuditMarker()
    {
        AuditEntry? capturedAudit = null;
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Do<AuditEntry>(e => capturedAudit = e), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        await sut.ReportAsync("user1", IncidentSeverity.Medium, "CAT", "desc");

        capturedAudit.Should().NotBeNull();
        capturedAudit!.Details.Should().NotContain("CRITICAL_INCIDENT");
    }

    // ── IncidentService: ResolveAsync paths ───────────────────────────────────

    [Fact]
    public async Task Service_ResolveAsync_AlreadyResolved_ReturnsValidationFailure()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var repo = new IncidentRepository();
        var sut = new IncidentService(repo, auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var report = await sut.ReportAsync("user1", IncidentSeverity.Medium, "CAT", "desc");
        // Resolve first time
        await sut.ResolveAsync(report.Value.IncidentId, "admin", "Fixed");

        // Try to resolve again
        var result = await sut.ResolveAsync(report.Value.IncidentId, "admin", "Fixed again");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Service_ResolveAsync_NotFound_ReturnsNotFound()
    {
        var auditService = Substitute.For<IAuditService>();
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var result = await sut.ResolveAsync("nonexistent-id", "admin", "resolution");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task Service_ResolveAsync_AuditFailureOnResolve_StillSucceeds()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Failure(ErrorCode.Unknown, "Audit fail"));
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var repo = new IncidentRepository();
        var sut = new IncidentService(repo, auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var report = await sut.ReportAsync("user1", IncidentSeverity.Medium, "CAT", "desc");
        var result = await sut.ResolveAsync(report.Value.IncidentId, "admin", "Fixed");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsResolved.Should().BeTrue();
    }

    // ── NotificationService: all severity levels ──────────────────────────────

    [Theory]
    [InlineData(IncidentSeverity.Critical)]
    [InlineData(IncidentSeverity.High)]
    [InlineData(IncidentSeverity.Medium)]
    [InlineData(IncidentSeverity.Low)]
    public void NotificationService_Notify_AllSeverityLevels_DoesNotThrow(IncidentSeverity severity)
    {
        var sut = new NotificationService(NullLogger<NotificationService>.Instance);
        var record = new IncidentRecord(
            "id", DateTimeOffset.UtcNow, "user", severity, "CAT", "desc", null, false, null, null);

        var act = () => sut.Notify(record);

        act.Should().NotThrow();
    }

    // ── IncidentService: ListAsync filter combinations ────────────────────────

    [Fact]
    public async Task Service_ListAsync_WithAllFilters_ReturnsFilteredResults()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var repo = new IncidentRepository();
        var sut = new IncidentService(repo, auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        await sut.ReportAsync("u1", IncidentSeverity.High, "CAT1", "desc1");
        await sut.ReportAsync("u1", IncidentSeverity.Low, "CAT2", "desc2");

        var result = await sut.ListAsync(
            IncidentSeverity.High,
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Severity.Should().Be(IncidentSeverity.High);
    }

    // ── IncidentService: VerifyAuditIntegrityAsync ────────────────────────────

    [Fact]
    public async Task Service_VerifyAuditIntegrityAsync_ReturnsResult()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.VerifyChainIntegrityAsync(Arg.Any<CancellationToken>())
                     .Returns(Result.Success(true));
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var result = await sut.VerifyAuditIntegrityAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    // ── IncidentService: ReportAsync multiple reports ─────────────────────────

    [Fact]
    public async Task Service_ReportAsync_MultipleReports_AllSucceed()
    {
        var auditService = Substitute.For<IAuditService>();
        auditService.WriteAuditAsync(Arg.Any<AuditEntry>(), Arg.Any<CancellationToken>())
                     .Returns(Result.Success());
        var notificationService = new NotificationService(NullLogger<NotificationService>.Instance);
        var sut = new IncidentService(
            new IncidentRepository(), auditService, notificationService,
            NullLogger<IncidentService>.Instance);

        var report1 = await sut.ReportAsync("u1", IncidentSeverity.High, "CAT", "desc");
        var report2 = await sut.ReportAsync("u1", IncidentSeverity.Medium, "CAT", "desc2");

        report1.IsSuccess.Should().BeTrue();
        report2.IsSuccess.Should().BeTrue();
        report1.Value.IncidentId.Should().NotBe(report2.Value.IncidentId);
    }

    // ── IncidentRecord: with expression ───────────────────────────────────────

    [Fact]
    public void IncidentRecord_With_UpdatesResolvedFields()
    {
        var original = new IncidentRecord(
            "id1", DateTimeOffset.UtcNow, "u1", IncidentSeverity.High,
            "CAT", "desc", null, false, null, null);

        var resolved = original with
        {
            Resolution = "Fixed",
            IsResolved = true,
            ResolvedAt = DateTimeOffset.UtcNow,
            ResolvedByUserId = "admin"
        };

        resolved.IsResolved.Should().BeTrue();
        resolved.Resolution.Should().Be("Fixed");
        resolved.ResolvedByUserId.Should().Be("admin");
        resolved.IncidentId.Should().Be("id1");
    }
}
