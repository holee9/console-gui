using HnVue.Common.Abstractions;
using HnVue.Common.Models;
using HnVue.Common.Results;
using HnVue.PatientManagement;
using NSubstitute;
using Xunit;

namespace HnVue.PatientManagement.Tests;

// @MX:NOTE Emergency patient registration tests - IEC 62304 Class B requirement for trauma care data integrity
/// <summary>
/// Tests for emergency patient quick registration (SWR-PM-030~033).
/// Validates that emergency workflow bypasses normal validation while maintaining data integrity.
/// </summary>
public sealed class PatientServiceEmergencyTests
{
    private readonly IPatientRepository _mockRepository = Substitute.For<IPatientRepository>();
    private readonly ISecurityContext _mockSecurityContext = Substitute.For<ISecurityContext>();

    public PatientServiceEmergencyTests()
    {
        _mockSecurityContext.CurrentUserId.Returns("test-user");
        _mockSecurityContext.CurrentUsername.Returns("TestUser");
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithValidEmergencyId_ReturnsSuccess()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";
        var patientName = "John Doe";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            patientName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(emergencyPatientId, result.Value.PatientId);
        Assert.Equal(patientName, result.Value.Name);
        Assert.True(result.Value.IsEmergency);
        Assert.Null(result.Value.DateOfBirth);
        Assert.Null(result.Value.Sex);

        // Verify repository was called (no duplicate check performed)
        await _mockRepository.Received(1).AddAsync(
            Arg.Is<PatientRecord>(p =>
                p.PatientId == emergencyPatientId &&
                p.Name == patientName &&
                p.IsEmergency),
            default);

        // Verify FindByIdAsync was NOT called (duplicate detection skipped)
        await _mockRepository.DidNotReceive().FindByIdAsync(Arg.Any<string>(), default);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithNullPatientName_CreatesUnknownPatientRecord()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            patientName: null);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UNKNOWN EMERGENCY PATIENT", result.Value.Name);
        Assert.True(result.Value.IsEmergency);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithEmptyPatientName_CreatesUnknownPatientRecord()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            patientName: string.Empty);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UNKNOWN EMERGENCY PATIENT", result.Value.Name);
        Assert.True(result.Value.IsEmergency);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithWhitespacePatientName_CreatesUnknownPatientRecord()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            patientName: "   ");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("UNKNOWN EMERGENCY PATIENT", result.Value.Name);
        Assert.True(result.Value.IsEmergency);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithNonEmergencyPrefix_ReturnsValidationFailed()
    {
        // Arrange
        var invalidPatientId = "PATIENT-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            invalidPatientId,
            "John Doe");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.ValidationFailed, result.Error);
        Assert.Contains("EMERG-", result.ErrorMessage);

        // Verify repository was NOT called
        await _mockRepository.DidNotReceive().AddAsync(Arg.Any<PatientRecord>(), default);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WithNullEmergencyId_ReturnsValidationFailed()
    {
        // Arrange
        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.QuickRegisterEmergencyAsync(null!, "John Doe"));
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_WhenRepositoryFails_PropagatesFailure()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(Result.Failure<PatientRecord>(
                ErrorCode.DatabaseError,
                "Database connection failed"));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            "John Doe");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorCode.DatabaseError, result.Error);
        Assert.Contains("Database connection failed", result.ErrorMessage);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";
        var beforeTime = DateTimeOffset.UtcNow;

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            "John Doe");

        var afterTime = DateTimeOffset.UtcNow;

        // Assert
        Assert.True(result.IsSuccess);
        Assert.InRange(result.Value.CreatedAt, beforeTime, afterTime);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_SetsCreatedByToCurrentUser()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            "John Doe");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("test-user", result.Value.CreatedBy);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_DoesNotCheckForDuplicates()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";

        // Simulate existing patient with same ID
        _mockRepository.FindByIdAsync(emergencyPatientId, default)
            .Returns(Result.Success(new PatientRecord(
                emergencyPatientId,
                "Existing Patient",
                new DateOnly(1980, 1, 1),
                "M",
                IsEmergency: false,
                DateTimeOffset.UtcNow.AddDays(-1),
                "USER1")));

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            "John Doe");

        // Assert
        Assert.True(result.IsSuccess);

        // Verify FindByIdAsync was NOT called (duplicate detection skipped)
        await _mockRepository.DidNotReceive().FindByIdAsync(Arg.Any<string>(), default);

        // Verify AddAsync WAS called (emergency override allows duplicate IDs)
        await _mockRepository.Received(1).AddAsync(
            Arg.Is<PatientRecord>(p => p.PatientId == emergencyPatientId),
            default);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_MultipleEmergenciesWithSameId_AllowsAll()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045"; // Same ID

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result1 = await service.QuickRegisterEmergencyAsync(emergencyPatientId, "Patient 1");
        var result2 = await service.QuickRegisterEmergencyAsync(emergencyPatientId, "Patient 2");
        var result3 = await service.QuickRegisterEmergencyAsync(emergencyPatientId, "Patient 3");

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.True(result3.IsSuccess);

        // All three registrations succeeded (no duplicate check)
        await _mockRepository.Received(3).AddAsync(Arg.Any<PatientRecord>(), default);
    }

    [Theory]
    [InlineData("EMERG-20260106153045")]
    [InlineData("EMERG-99999999999999")]
    [InlineData("EMERG-00000000000000")]
    [InlineData("EMERG-12345")]
    public async Task QuickRegisterEmergencyAsync_AcceptsAnyEmergencyPrefixedId(string emergencyId)
    {
        // Arrange
        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(emergencyId, "Test Patient");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(emergencyId, result.Value.PatientId);
    }

    [Fact]
    public async Task QuickRegisterEmergencyAsync_PreservesOriginalPatientName()
    {
        // Arrange
        var emergencyPatientId = "EMERG-20260106153045";
        var originalName = "Smith^John^^Mr.";

        _mockRepository.AddAsync(Arg.Any<PatientRecord>(), default)
            .Returns(ci => Result.Success(ci.Arg<PatientRecord>()));

        var service = new PatientService(_mockRepository, _mockSecurityContext);

        // Act
        var result = await service.QuickRegisterEmergencyAsync(
            emergencyPatientId,
            originalName);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalName, result.Value.Name);
    }
}
