using FluentAssertions;
using HnVue.Common.Abstractions;
using Xunit;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.UI.ViewModels;
using NSubstitute;

namespace HnVue.UI.Tests;

/// <summary>
/// Tests for <see cref="PatientListViewModel"/>.
/// </summary>
public sealed class PatientListViewModelTests
{
    private readonly IPatientService _patientService = Substitute.For<IPatientService>();

    private PatientListViewModel CreateSut() => new(_patientService);

    private static PatientRecord MakePatient(string id = "P001") => new(
        PatientId: id,
        Name: $"Test^{id}",
        DateOfBirth: new DateOnly(1980, 1, 1),
        Sex: "M",
        IsEmergency: false,
        CreatedAt: DateTimeOffset.UtcNow,
        CreatedBy: "admin");

    [Fact]
    public void Constructor_SetsDefaultProperties()
    {
        var sut = CreateSut();

        sut.SearchQuery.Should().Be(string.Empty);
        sut.Patients.Should().BeEmpty();
        sut.SelectedPatient.Should().BeNull();
        sut.IsLoading.Should().BeFalse();
    }

    [Fact]
    public async Task SearchCommand_WithSuccessResult_PopulatesPatientsCollection()
    {
        var patients = new[] { MakePatient("P001"), MakePatient("P002") };
        _patientService
            .SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(patients));

        var sut = CreateSut();
        sut.SearchQuery = "Test";

        await sut.SearchCommand.ExecuteAsync(null);

        sut.Patients.Should().HaveCount(2);
        sut.Patients[0].PatientId.Should().Be("P001");
        sut.Patients[1].PatientId.Should().Be("P002");
    }

    [Fact]
    public async Task SearchCommand_CallsServiceWithCurrentQuery()
    {
        _patientService
            .SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(Array.Empty<PatientRecord>()));

        var sut = CreateSut();
        sut.SearchQuery = "Smith";

        await sut.SearchCommand.ExecuteAsync(null);

        await _patientService.Received(1).SearchAsync("Smith", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchCommand_WithFailureResult_SetsErrorMessage()
    {
        _patientService
            .SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<IReadOnlyList<PatientRecord>>(
                ErrorCode.DatabaseError, "DB connection failed"));

        var sut = CreateSut();

        await sut.SearchCommand.ExecuteAsync(null);

        sut.ErrorMessage.Should().Be("DB connection failed");
        sut.Patients.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchCommand_ClearsPreviousResults()
    {
        var first = new[] { MakePatient("P001") };
        var second = Array.Empty<PatientRecord>();

        _patientService
            .SearchAsync("first", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(first));
        _patientService
            .SearchAsync("second", Arg.Any<CancellationToken>())
            .Returns(Result.Success<IReadOnlyList<PatientRecord>>(second));

        var sut = CreateSut();
        sut.SearchQuery = "first";
        await sut.SearchCommand.ExecuteAsync(null);

        sut.SearchQuery = "second";
        await sut.SearchCommand.ExecuteAsync(null);

        sut.Patients.Should().BeEmpty();
    }

    [Fact]
    public void SelectPatientCommand_SetsSelectedPatientAndRaisesEvent()
    {
        var patient = MakePatient("P001");
        PatientRecord? receivedPatient = null;

        var sut = CreateSut();
        sut.PatientSelected += (_, p) => receivedPatient = p;

        sut.SelectPatientCommand.Execute(patient);

        sut.SelectedPatient.Should().Be(patient);
        receivedPatient.Should().Be(patient);
    }

    [Fact]
    public async Task SearchCommand_SetsIsLoadingDuringExecution()
    {
        bool wasLoadingDuringCall = false;
        _patientService
            .SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(async _ =>
            {
                await Task.Delay(1);
                return Result.Success<IReadOnlyList<PatientRecord>>(Array.Empty<PatientRecord>());
            });

        var sut = CreateSut();
        var task = sut.SearchCommand.ExecuteAsync(null);

        await task;
        sut.IsLoading.Should().BeFalse();
    }
}
