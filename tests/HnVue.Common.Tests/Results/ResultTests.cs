using FluentAssertions;
using HnVue.Common.Results;
using Xunit;

namespace HnVue.Common.Tests.Results;

public sealed class ResultTests
{
    // ── Result<T> Success ─────────────────────────────────────────────────────

    [Fact]
    public void Success_WithValue_CreatesSuccessfulResult()
    {
        var result = Result.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Success_WithStringValue_CreatesSuccessfulResult()
    {
        var result = Result.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    // ── Result<T> Failure ─────────────────────────────────────────────────────

    [Fact]
    public void Failure_CreatesFailedResult()
    {
        var result = Result.Failure<int>(ErrorCode.NotFound, "Item not found");

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(ErrorCode.NotFound);
        result.ErrorMessage.Should().Be("Item not found");
    }

    [Fact]
    public void Failure_AccessingValue_ThrowsInvalidOperationException()
    {
        var result = Result.Failure<int>(ErrorCode.NotFound, "missing");

        var act = () => { _ = result.Value; };

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Value*");
    }

    // ── Result<T> Map ─────────────────────────────────────────────────────────

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        var result = Result.Success(5).Map(x => x * 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        var result = Result.Failure<int>(ErrorCode.ValidationFailed, "bad input")
            .Map(x => x * 2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Be("bad input");
    }

    // ── Result<T> Bind ────────────────────────────────────────────────────────

    [Fact]
    public void Bind_OnSuccess_ChainsOperation()
    {
        var result = Result.Success(3)
            .Bind(x => Result.Success($"value={x}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("value=3");
    }

    [Fact]
    public void Bind_OnSuccess_WhenBinderReturnsFailure_PropagatesBinderError()
    {
        var result = Result.Success(3)
            .Bind(x => Result.Failure<string>(ErrorCode.DatabaseError, "db error"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.DatabaseError);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesOriginalError_WithoutCallingBinder()
    {
        var binderCalled = false;

        var result = Result.Failure<int>(ErrorCode.NotFound, "not found")
            .Bind(x => { binderCalled = true; return Result.Success("ok"); });

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.NotFound);
        binderCalled.Should().BeFalse();
    }

    // ── Implicit conversion ───────────────────────────────────────────────────

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccessResult()
    {
        Result<int> result = 99;

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(99);
    }

    // ── ToString ──────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_OnSuccess_ContainsSuccessIndicator()
    {
        var result = Result.Success(1);

        result.ToString().Should().Contain("Success");
    }

    [Fact]
    public void ToString_OnFailure_ContainsErrorCode()
    {
        var result = Result.Failure<int>(ErrorCode.AuthenticationFailed, "bad creds");

        var text = result.ToString();

        text.Should().Contain("Failure");
        text.Should().Contain(nameof(ErrorCode.AuthenticationFailed));
    }

    // ── Non-generic Result ────────────────────────────────────────────────────

    [Fact]
    public void NonGenericSuccess_IsSuccessful()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void NonGenericFailure_IsFailedWithCode()
    {
        var result = Result.Failure(ErrorCode.EncryptionFailed, "key error");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ErrorCode.EncryptionFailed);
        result.ErrorMessage.Should().Be("key error");
    }

    [Fact]
    public void NonGenericSuccess_IsSingletonInstance()
    {
        var a = Result.Success();
        var b = Result.Success();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void NonGenericToString_OnSuccess_ReturnsSuccessString()
    {
        Result.Success().ToString().Should().Be("Success");
    }

    [Fact]
    public void NonGenericToString_OnFailure_ContainsErrorCode()
    {
        var text = Result.Failure(ErrorCode.BurnFailed, "disc full").ToString();

        text.Should().Contain("Failure");
        text.Should().Contain(nameof(ErrorCode.BurnFailed));
    }
}
