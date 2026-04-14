using FluentAssertions;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.Common.Tests.Results;

/// <summary>
/// Additional edge-case tests for Result monad to reach 85%+ coverage on HnVue.Common.
/// </summary>
public sealed class ResultEdgeCaseTests
{
    // ── Success<T> with null reference type ───────────────────────────────────

    [Fact]
    public void Success_Generic_WithNullReference_Throws()
    {
        var act = () => Result.Success<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── SuccessNullable ───────────────────────────────────────────────────────

    [Fact]
    public void SuccessNullable_WithNull_CreatesSuccessfulResult()
    {
        var result = Result.SuccessNullable<string>(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public void SuccessNullable_WithValue_CreatesSuccessfulResult()
    {
        var result = Result.SuccessNullable<string?>("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    // ── Map with null mapper ──────────────────────────────────────────────────

    [Fact]
    public void Map_NullMapper_Throws()
    {
        var result = Result.Success(1);

        var act = () => result.Map<int>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Bind with null binder ─────────────────────────────────────────────────

    [Fact]
    public void Bind_NullBinder_Throws()
    {
        var result = Result.Success(1);

        var act = () => result.Bind<string>(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    // ── Non-generic Result Failure ────────────────────────────────────────────

    [Fact]
    public void NonGeneric_Failure_WithAllErrorCodes()
    {
        // Test a selection of error codes to ensure enum range works
        var codes = new[]
        {
            ErrorCode.Unknown,
            ErrorCode.ValidationFailed,
            ErrorCode.NotFound,
            ErrorCode.AlreadyExists,
            ErrorCode.OperationCancelled,
            ErrorCode.FileOperationFailed,
            ErrorCode.NetworkTimeout,
            ErrorCode.CommunicationFailure,
            ErrorCode.HardwareNoResponse,
            ErrorCode.ConnectionRefused,
            ErrorCode.SslHandshakeFailed,
            ErrorCode.AuthenticationFailed,
            ErrorCode.AccountLocked,
            ErrorCode.TokenExpired,
            ErrorCode.TokenInvalid,
            ErrorCode.InsufficientPermission,
            ErrorCode.PasswordPolicyViolation,
            ErrorCode.PinNotSet,
            ErrorCode.TokenRevoked,
            ErrorCode.CalibrationDataMissing,
            ErrorCode.DatabaseError,
            ErrorCode.MigrationFailed,
            ErrorCode.EncryptionFailed,
            ErrorCode.InvalidStateTransition,
            ErrorCode.GeneratorNotReady,
            ErrorCode.DetectorNotReady,
            ErrorCode.ExposureAborted,
            ErrorCode.DoseLimitExceeded,
            ErrorCode.DoseInterlock,
            ErrorCode.DicomConnectionFailed,
            ErrorCode.DicomStoreFailed,
            ErrorCode.DicomQueryFailed,
            ErrorCode.DicomPrintFailed,
            ErrorCode.IncidentLogFailed,
            ErrorCode.SignatureVerificationFailed,
            ErrorCode.UpdatePackageCorrupt,
            ErrorCode.RollbackFailed,
            ErrorCode.BurnFailed,
            ErrorCode.DiscVerificationFailed,
            ErrorCode.ImageProcessingFailed,
            ErrorCode.UnsupportedImageFormat,
        };

        foreach (var code in codes)
        {
            var result = Result.Failure(code, $"Error: {code}");
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Be(code);
            result.ErrorMessage.Should().NotBeNull();
        }
    }

    // ── Chained Map/Bind ──────────────────────────────────────────────────────

    [Fact]
    public void Chained_MapBind_Composition()
    {
        var result = Result.Success(10)
            .Map(x => x * 2)
            .Bind(x => x > 15 ? Result.Success($"big: {x}") : Result.Failure<string>(ErrorCode.ValidationFailed, "too small"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("big: 20");
    }

    [Fact]
    public void Chained_MapBind_FailurePropagation()
    {
        var result = Result.Failure<int>(ErrorCode.DatabaseError, "connection lost")
            .Map(x => x * 2)
            .Bind(x => Result.Success($"value: {x}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
        result.ErrorMessage.Should().Be("connection lost");
    }

    // ── Match with complex types ──────────────────────────────────────────────

    [Fact]
    public void Match_WithComplexTransformations()
    {
        var success = Result.Success(new[] { 1, 2, 3 });
        var output = success.Match(
            v => string.Join(",", v),
            (code, msg) => $"Error: {code}");

        output.Should().Be("1,2,3");
    }

    // ── Implicit conversion edge case ─────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_ReferenceType()
    {
        Result<string> result = "test";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test");
    }

    [Fact]
    public void ImplicitConversion_ValueType()
    {
        Result<double> result = 3.14;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(3.14, 0.001);
    }
}
