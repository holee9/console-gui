using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.Dicom;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-PM-025")]
public sealed class WorklistRepositoryTests
{
    private readonly IDicomService _dicomService;
    private readonly IDicomNetworkConfig _config;
    private readonly WorklistRepository _sut;

    private static WorklistItem MakeWorklistItem(
        string accession = "ACC001",
        string patientId = "P001",
        string patientName = "Doe^John",
        DateOnly? studyDate = null,
        string? bodyPart = "CHEST",
        string? procedure = "Chest PA") =>
        new(accession, patientId, patientName, studyDate ?? DateOnly.FromDateTime(DateTime.Today),
            bodyPart, procedure);

    public WorklistRepositoryTests()
    {
        _dicomService = Substitute.For<IDicomService>();
        _config = Substitute.For<IDicomNetworkConfig>();
        _config.PacsAeTitle.Returns("PACS_AET");

        _sut = new WorklistRepository(_dicomService, _config);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullDicomService_ThrowsArgumentNullException()
    {
        var act = () => new WorklistRepository(null!, _config);

        act.Should().Throw<ArgumentNullException>().WithParameterName("dicomService");
    }

    [Fact]
    public void Constructor_NullConfig_ThrowsArgumentNullException()
    {
        var act = () => new WorklistRepository(_dicomService, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("config");
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        var sut = new WorklistRepository(_dicomService, _config);

        sut.Should().NotBeNull();
    }

    // ── QueryTodayAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task QueryTodayAsync_SuccessfulQuery_ReturnsWorklistItems()
    {
        var items = (IReadOnlyList<WorklistItem>)new[] { MakeWorklistItem() };
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task QueryTodayAsync_DicomServiceReturnsFailure_ReturnsEmptyList()
    {
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed, "MWL SCP unreachable"));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_EmptyWorklistResponse_ReturnsEmptyList()
    {
        var empty = (IReadOnlyList<WorklistItem>)Array.Empty<WorklistItem>();
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(empty));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_CallsServiceWithTodayDateRange()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));

        await _sut.QueryTodayAsync();

        await _dicomService.Received(1).QueryWorklistAsync(
            Arg.Is<WorklistQuery>(q =>
                q.DateFrom == today &&
                q.DateTo == today &&
                q.PatientId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTodayAsync_UsesConfiguredAeTitle()
    {
        _config.PacsAeTitle.Returns("CUSTOM_AET");
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));

        await _sut.QueryTodayAsync();

        await _dicomService.Received(1).QueryWorklistAsync(
            Arg.Is<WorklistQuery>(q => q.AeTitle == "CUSTOM_AET"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTodayAsync_PassesCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), token)
            .Returns(Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));

        await _sut.QueryTodayAsync(token);

        await _dicomService.Received(1).QueryWorklistAsync(
            Arg.Any<WorklistQuery>(), token);
    }

    [Fact]
    public async Task QueryTodayAsync_MultipleItems_AllReturned()
    {
        var items = (IReadOnlyList<WorklistItem>)new[]
        {
            MakeWorklistItem("ACC001", "P001", "Kim^Minho"),
            MakeWorklistItem("ACC002", "P002", "Lee^Sujin"),
            MakeWorklistItem("ACC003", "P003", "Park^Jiwoo"),
        };
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].PatientName.Should().Be("Kim^Minho");
        result.Value[1].PatientName.Should().Be("Lee^Sujin");
        result.Value[2].PatientName.Should().Be("Park^Jiwoo");
    }

    // ── Edge Cases ────────────────────────────────────────────────────────────

    [Fact]
    public async Task QueryTodayAsync_SpecialCharactersInPatientNames_NoCrash()
    {
        var items = (IReadOnlyList<WorklistItem>)new[]
        {
            MakeWorklistItem(patientName: "O'Brien^Sean"),
            MakeWorklistItem(patientName: "Mueller-Hein^Anna-Katharina"),
            MakeWorklistItem(patientName: "\u00C9tude^Ren\u00E9"), // Unicode: Étude^René
        };
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value[0].PatientName.Should().Contain("'");
        result.Value[1].PatientName.Should().Contain("-");
    }

    [Fact]
    public async Task QueryTodayAsync_LargeResultSet_ProcessedCorrectly()
    {
        var largeList = Enumerable.Range(1, 200)
            .Select(i => MakeWorklistItem($"ACC{i:D4}", $"P{i:D4}"))
            .ToArray();
        var items = (IReadOnlyList<WorklistItem>)largeList;
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(200);
        result.Value[0].AccessionNumber.Should().Be("ACC0001");
        result.Value[199].AccessionNumber.Should().Be("ACC0200");
    }

    [Fact]
    public async Task QueryTodayAsync_NetworkFailureReturnsSuccess_WithEmptyList()
    {
        // The key behavior: network/DICOM failures are swallowed and return empty list
        // because the worklist is advisory-only and must not block the acquisition workflow.
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed, "Connection refused"));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryTodayAsync_DoesNotThrowWhenServiceFails()
    {
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<WorklistItem>>(
                ErrorCode.DicomQueryFailed, "Timeout"));

        var act = async () => await _sut.QueryTodayAsync();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task QueryTodayAsync_NullPatientIdInQuery_MatchesServiceCall()
    {
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<WorklistItem>>(Array.Empty<WorklistItem>()));

        await _sut.QueryTodayAsync();

        await _dicomService.Received(1).QueryWorklistAsync(
            Arg.Is<WorklistQuery>(q => q.PatientId == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task QueryTodayAsync_ReturnedItemsPreserveAllFields()
    {
        var item = new WorklistItem(
            AccessionNumber: "ACC999",
            PatientId: "P999",
            PatientName: "Test^Patient",
            StudyDate: new DateOnly(2026, 4, 9),
            BodyPart: "ABDOMEN",
            RequestedProcedure: "Abdomen CT");
        var items = (IReadOnlyList<WorklistItem>)new[] { item };
        _dicomService.QueryWorklistAsync(Arg.Any<WorklistQuery>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(items));

        var result = await _sut.QueryTodayAsync();

        result.IsSuccess.Should().BeTrue();
        var returned = result.Value[0];
        returned.AccessionNumber.Should().Be("ACC999");
        returned.PatientId.Should().Be("P999");
        returned.PatientName.Should().Be("Test^Patient");
        returned.StudyDate.Should().Be(new DateOnly(2026, 4, 9));
        returned.BodyPart.Should().Be("ABDOMEN");
        returned.RequestedProcedure.Should().Be("Abdomen CT");
    }
}
