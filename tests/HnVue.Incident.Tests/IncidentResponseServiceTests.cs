using FluentAssertions;
using HnVue.Common.Enums;
using HnVue.Common.Results;
using HnVue.Incident;
using HnVue.Incident.Models;
using NSubstitute;
using Xunit;

namespace HnVue.Incident.Tests;

[Trait("SWR", "SWR-INC-010")]
public sealed class IncidentResponseServiceTests
{
    private readonly IIncidentRepository _repository;
    private readonly IncidentResponseService _sut;

    public IncidentResponseServiceTests()
    {
        _repository = Substitute.For<IIncidentRepository>();
        _sut = new IncidentResponseService(_repository);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var act = () => new IncidentResponseService(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── RecordAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Record_ValidIncident_SavesAndReturnsRecord()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.RecordAsync(
            IncidentSeverity.Medium,
            "NETWORK",
            "PACS connection dropped",
            "DicomService");

        result.IsSuccess.Should().BeTrue();
        result.Value.Severity.Should().Be(IncidentSeverity.Medium);
        result.Value.Category.Should().Be("NETWORK");
        result.Value.IsResolved.Should().BeFalse();
    }

    [Fact]
    public async Task Record_EmptyCategory_ReturnsValidationFailure()
    {
        var result = await _sut.RecordAsync(
            IncidentSeverity.Low,
            "   ",
            "Description",
            "Source");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Record_EmptyDescription_ReturnsValidationFailure()
    {
        var result = await _sut.RecordAsync(
            IncidentSeverity.Low,
            "CATEGORY",
            "",
            "Source");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Record_NullCategory_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RecordAsync(IncidentSeverity.Low, null!, "desc", "src");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Record_CriticalIncident_TriggersEscalationCallback()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        IncidentRecord? escalated = null;
        _sut.OnCritical(r =>
        {
            escalated = r;
            return Task.CompletedTask;
        });

        var result = await _sut.RecordAsync(
            IncidentSeverity.Critical,
            "DOSE",
            "Emergency dose exceeded",
            "DoseService");

        result.IsSuccess.Should().BeTrue();
        escalated.Should().NotBeNull();
        escalated!.Severity.Should().Be(IncidentSeverity.Critical);
    }

    [Fact]
    public async Task Record_NonCriticalIncident_DoesNotTriggerEscalation()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var escalated = false;
        _sut.OnCritical(_ =>
        {
            escalated = true;
            return Task.CompletedTask;
        });

        await _sut.RecordAsync(IncidentSeverity.High, "HW", "Fan fault", "System");

        escalated.Should().BeFalse();
    }

    [Fact]
    public async Task Record_EscalationCallbackThrows_StillReturnsSuccess()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _sut.OnCritical(_ => throw new InvalidOperationException("Callback failure"));

        var result = await _sut.RecordAsync(
            IncidentSeverity.Critical, "TEST", "desc", "src");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Record_RepositoryFailure_ReturnsFailure()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.IncidentLogFailed, "Write failed"));

        var result = await _sut.RecordAsync(IncidentSeverity.Low, "NET", "desc", "src");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.IncidentLogFailed);
    }

    // ── GetBySeverityAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetBySeverity_ReturnsDelegatedResult()
    {
        var records = (IReadOnlyList<IncidentRecord>)new[]
        {
            new IncidentRecord(
                IncidentId: "I1",
                OccurredAt: DateTimeOffset.UtcNow,
                ReportedByUserId: "src",
                Severity: IncidentSeverity.High,
                Category: "NET",
                Description: "desc",
                Resolution: null,
                IsResolved: false,
                ResolvedAt: null,
                ResolvedByUserId: null),
        };
        _repository.GetBySeverityAsync(IncidentSeverity.High, Arg.Any<CancellationToken>())
            .Returns(Result.Success(records));

        var result = await _sut.GetBySeverityAsync(IncidentSeverity.High);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    // ── ResolveAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Resolve_ValidId_DelegatesToRepository()
    {
        _repository.ResolveAsync("I1", "Fixed by upgrade", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.ResolveAsync("I1", "Fixed by upgrade");

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).ResolveAsync("I1", "Fixed by upgrade", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Resolve_NullId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.ResolveAsync(null!, "resolution");

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── OnCritical ────────────────────────────────────────────────────────────

    [Fact]
    public async Task OnCritical_MultipleCallbacks_AllInvoked()
    {
        _repository.SaveAsync(Arg.Any<IncidentRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var count = 0;
        _sut.OnCritical(_ => { count++; return Task.CompletedTask; });
        _sut.OnCritical(_ => { count++; return Task.CompletedTask; });

        await _sut.RecordAsync(IncidentSeverity.Critical, "CAT", "desc", "src");

        count.Should().Be(2);
    }
}
