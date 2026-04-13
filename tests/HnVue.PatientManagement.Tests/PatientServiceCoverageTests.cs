using FluentAssertions;
using HnVue.Common.Abstractions;
using HnVue.Common.Enums;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

[Trait("SWR", "SWR-PM-030")]
public sealed class PatientServiceCoverageTests
{
    private readonly IPatientRepository _repository;
    private readonly ISecurityContext _securityContext;
    private readonly PatientService _sut;

    public PatientServiceCoverageTests()
    {
        _repository = Substitute.For<IPatientRepository>();
        _securityContext = Substitute.For<ISecurityContext>();
        _sut = new PatientService(_repository, _securityContext);
    }

    // ── UpdateAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_NullPatient_ThrowsArgumentNullException()
    {
        var act = () => _sut.UpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patient");
    }

    [Fact]
    public async Task UpdateAsync_EmptyPatientId_ReturnsValidationFailure()
    {
        var patient = CreatePatient(string.Empty);

        var result = await _sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task UpdateAsync_WhitespacePatientId_ReturnsValidationFailure()
    {
        var patient = CreatePatient("   ");

        var result = await _sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task UpdateAsync_ValidPatient_DelegatesToRepository()
    {
        var patient = CreatePatient("P001");
        _repository.UpdateAsync(patient, Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.UpdateAsync(patient);

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(patient, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_RepositoryFails_ReturnsFailure()
    {
        var patient = CreatePatient("P001");
        _repository.UpdateAsync(patient, Arg.Any<CancellationToken>())
            .Returns(Result.Failure(ErrorCode.DatabaseError, "Update failed"));

        var result = await _sut.UpdateAsync(patient);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── DeleteAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_NullPatientId_ThrowsArgumentNullException()
    {
        var act = () => _sut.DeleteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("patientId");
    }

    [Fact]
    public async Task DeleteAsync_PatientNotFound_ReturnsNotFound()
    {
        // Use NotFound failure instead of Result.Success(null)
        _repository.FindByIdAsync("P999", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord?>(ErrorCode.NotFound, "Patient not found"));

        var result = await _sut.DeleteAsync("P999");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_RepositoryFindFails_ReturnsNotFound()
    {
        // When FindById fails, DeleteAsync returns NotFound (maps IsFailure to NotFound)
        _repository.FindByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord?>(ErrorCode.DatabaseError, "DB error"));

        var result = await _sut.DeleteAsync("P001");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
    }

    [Fact]
    public async Task DeleteAsync_PatientExists_DelegatesToDelete()
    {
        var patient = CreatePatient("P001");
        _repository.FindByIdAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.Success<PatientRecord?>(patient));
        _repository.DeleteAsync("P001", Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var result = await _sut.DeleteAsync("P001");

        result.IsSuccess.Should().BeTrue();
        await _repository.Received(1).DeleteAsync("P001", Arg.Any<CancellationToken>());
    }

    // ── QuickRegisterEmergencyAsync ────────────────────────────────────────────

    [Fact]
    public async Task QuickRegisterEmergency_NullId_ThrowsArgumentNullException()
    {
        var act = () => _sut.QuickRegisterEmergencyAsync(null!, "John");

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("emergencyPatientId");
    }

    [Fact]
    public async Task QuickRegisterEmergency_InvalidPrefix_ReturnsValidationFailure()
    {
        var result = await _sut.QuickRegisterEmergencyAsync("PAT-001", "John");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task QuickRegisterEmergency_ValidId_WithPatientName_RegistersSuccessfully()
    {
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-001", "Doe^John");

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be("EMERG-001");
        result.Value.Name.Should().Be("Doe^John");
        result.Value.IsEmergency.Should().BeTrue();
    }

    [Fact]
    public async Task QuickRegisterEmergency_ValidId_NullName_UsesUnknownDefault()
    {
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-002", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UNKNOWN EMERGENCY PATIENT");
    }

    [Fact]
    public async Task QuickRegisterEmergency_ValidId_EmptyName_UsesUnknownDefault()
    {
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-003", "   ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("UNKNOWN EMERGENCY PATIENT");
    }

    [Fact]
    public async Task QuickRegisterEmergency_SetsCreatedBy_UserId()
    {
        _securityContext.CurrentUserId.Returns("user-42");
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-004", "Test");

        result.Value.CreatedBy.Should().Be("user-42");
    }

    [Fact]
    public async Task QuickRegisterEmergency_SetsCreatedBy_UsernameFallback()
    {
        _securityContext.CurrentUserId.Returns((string?)null);
        _securityContext.CurrentUsername.Returns("admin");
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-005", "Test");

        result.Value.CreatedBy.Should().Be("admin");
    }

    [Fact]
    public async Task QuickRegisterEmergency_SetsCreatedBy_SystemFallback()
    {
        _securityContext.CurrentUserId.Returns((string?)null);
        _securityContext.CurrentUsername.Returns((string?)null);
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-006", "Test");

        result.Value.CreatedBy.Should().Be("SYSTEM");
    }

    [Fact]
    public async Task QuickRegisterEmergency_RepositoryFails_ReturnsFailure()
    {
        _repository.AddAsync(Arg.Any<PatientRecord>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<PatientRecord>(ErrorCode.DatabaseError, "DB error"));

        var result = await _sut.QuickRegisterEmergencyAsync("EMERG-007", "Test");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    // ── SearchAsync additional ─────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_NullQuery_ThrowsArgumentNullException()
    {
        var act = () => _sut.SearchAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("query");
    }

    [Fact]
    public async Task SearchAsync_EmptyQuery_ReturnsValidationFailure()
    {
        var result = await _sut.SearchAsync("");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    private static PatientRecord CreatePatient(string patientId, string name = "Test^Patient") => new(
        PatientId: patientId,
        Name: name,
        DateOfBirth: null,
        Sex: null,
        IsEmergency: false,
        CreatedAt: DateTimeOffset.UtcNow,
        CreatedBy: "test-user");
}
