using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-PM-010")]
public sealed class PatientServiceTests
{
    private readonly IPatientRepository _repository;
    private readonly PatientService _sut;

    private static PatientRecord MakePatient(string? id = "P001", string? name = "Doe^John") =>
        new(id ?? "P001", name ?? "Doe^John", new DateOnly(1980, 1, 1), "M",
            IsEmergency: false, DateTimeOffset.UtcNow, "op1");

    public PatientServiceTests()
    {
        _repository = Substitute.For<IPatientRepository>();
        var securityContext = Substitute.For<ISecurityContext>();
        securityContext.CurrentUserId.Returns("test-user");
        securityContext.CurrentUsername.Returns("TestUser");
        _sut = new PatientService(_repository, securityContext);
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        var securityContext = Substitute.For<ISecurityContext>();
        var act = () => new PatientService(null!, securityContext);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── RegisterAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_NewPatient_ReturnsSuccess()
    {
        var patient = MakePatient();
        _repository.FindByIdAsync(patient.PatientId, Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(null));
        _repository.AddAsync(patient, Arg.Any<CancellationToken>())
            .Returns(Result.Success(patient));

        var result = await _sut.RegisterAsync(patient);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(patient);
    }

    [Fact]
    public async Task Register_DuplicateId_ReturnsAlreadyExists()
    {
        var patient = MakePatient();
        _repository.FindByIdAsync(patient.PatientId, Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(patient)); // Already exists

        var result = await _sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.AlreadyExists);
    }

    [Fact]
    public async Task Register_NullPatient_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.RegisterAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Register_EmptyPatientId_ReturnsValidationFailure()
    {
        var patient = MakePatient(id: "");

        var result = await _sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Register_EmptyName_ReturnsValidationFailure()
    {
        var patient = MakePatient(name: "");

        var result = await _sut.RegisterAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── SearchAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Search_ValidQuery_ReturnsResults()
    {
        var patients = (IReadOnlyList<PatientRecord>)new[] { MakePatient() };
        _repository.SearchAsync("Doe", Arg.Any<CancellationToken>())
            .Returns(Result.Success(patients));

        var result = await _sut.SearchAsync("Doe");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task Search_EmptyQuery_ReturnsValidationFailure()
    {
        var result = await _sut.SearchAsync("   ");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task Search_NullQuery_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.SearchAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingPatient_ReturnsSuccess()
    {
        var patient = MakePatient();
        _repository.UpdateAsync(patient, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UpdateAsync(patient);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Update_NullPatient_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.UpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Update_EmptyPatientId_ReturnsValidationFailure()
    {
        var patient = MakePatient(id: "");

        var result = await _sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ExistingId_ReturnsPatient()
    {
        var patient = MakePatient();
        _repository.FindByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(patient));

        var result = await _sut.GetByIdAsync("P001");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(patient);
    }

    [Fact]
    public async Task GetById_NullId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.GetByIdAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingPatient_ReturnsSuccess()
    {
        var patient = MakePatient();
        _repository.FindByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(patient));
        _repository.DeleteAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.DeleteAsync("P001");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Delete_NonExistentPatient_ReturnsNotFound()
    {
        _repository.FindByIdAsync("UNKNOWN", Arg.Any<CancellationToken>())
            .Returns(Result.SuccessNullable<PatientRecord?>(null));

        var result = await _sut.DeleteAsync("UNKNOWN");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task Delete_NullId_ThrowsArgumentNullException()
    {
        var act = async () => await _sut.DeleteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
