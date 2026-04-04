namespace HnVue.Common.Results;

/// <summary>
/// Static factory class for creating <see cref="Result{T}"/> instances without having to
/// specify the generic type parameter at the call site.
/// Also serves as the non-generic void-operation result.
/// </summary>
public sealed class Result
{
    private static readonly Result _success = new(isSuccess: true, default, null);

    private readonly string? _errorMessage;

    private Result(bool isSuccess, ErrorCode? code, string? message)
    {
        IsSuccess = isSuccess;
        Error = code;
        _errorMessage = message;
    }

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>Gets the error code, or <see langword="null"/> on success.</summary>
    public ErrorCode? Error { get; }

    /// <summary>Gets the human-readable error message, or <see langword="null"/> on success.</summary>
    public string? ErrorMessage => _errorMessage;

    // ── Non-generic factory methods ───────────────────────────────────────────

    /// <summary>Returns the singleton successful void result.</summary>
    public static Result Success() => _success;

    /// <summary>Creates a failure void result with the given error code and message.</summary>
    /// <param name="code">Domain-specific error code.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Result Failure(ErrorCode code, string message) => new(false, code, message);

    // ── Generic factory methods (avoids CA1000 by placing them on the non-generic class) ──

    /// <summary>Creates a successful result wrapping <paramref name="value"/>.</summary>
    /// <typeparam name="T">Type of the successful value.</typeparam>
    /// <param name="value">The value to wrap.</param>
    public static Result<T> Success<T>(T value) => Result<T>.CreateSuccess(value);

    /// <summary>Creates a failure result with the given error code and message.</summary>
    /// <typeparam name="T">Type of the expected successful value.</typeparam>
    /// <param name="code">Domain-specific error code.</param>
    /// <param name="message">Human-readable error description.</param>
    public static Result<T> Failure<T>(ErrorCode code, string message) => Result<T>.CreateFailure(code, message);

    /// <inheritdoc/>
    public override string ToString() => IsSuccess
        ? "Success"
        : $"Failure({Error}: {_errorMessage})";
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/>.
/// Follows the Result monad pattern for explicit, exception-free error propagation.
/// Thread-safe and immutable after construction.
/// </summary>
/// <typeparam name="T">The type of the successful value.</typeparam>
public sealed class Result<T>
{
    private readonly T? _value;
    private readonly string? _errorMessage;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        Error = default;
        _errorMessage = null;
    }

    private Result(ErrorCode code, string message)
    {
        IsSuccess = false;
        _value = default;
        Error = code;
        _errorMessage = message;
    }

    // Internal factory methods called by the non-generic Result class.
    internal static Result<T> CreateSuccess(T value)
    {
        // Guard against null for reference types; value types are inherently non-null.
        if (value is null)
            throw new ArgumentNullException(nameof(value), "Cannot create a successful Result with a null value.");
        return new(value);
    }

    internal static Result<T> CreateFailure(ErrorCode code, string message) => new(code, message);

    /// <summary>Gets a value indicating whether the operation succeeded.</summary>
    public bool IsSuccess { get; }

    /// <summary>Gets a value indicating whether the operation failed.</summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the successful value.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessed on a failure result.</exception>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Cannot access Value on a failed result. Error: {Error} – {_errorMessage}");

    /// <summary>Gets the error code, or <see langword="null"/> on success.</summary>
    public ErrorCode? Error { get; }

    /// <summary>Gets the human-readable error message, or <see langword="null"/> on success.</summary>
    public string? ErrorMessage => _errorMessage;

    /// <summary>
    /// Transforms the value using <paramref name="mapper"/> if the result is successful;
    /// otherwise propagates the existing failure.
    /// </summary>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="mapper">Projection function applied to the value on success.</param>
    public Result<TOut> Map<TOut>(Func<T, TOut> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess
            ? Result<TOut>.CreateSuccess(mapper(_value!))
            : Result<TOut>.CreateFailure(Error!.Value, _errorMessage!);
    }

    /// <summary>
    /// Chains result-returning operations. If the current result is successful,
    /// applies <paramref name="binder"/> to the value; otherwise propagates the failure.
    /// </summary>
    /// <typeparam name="TOut">The output value type.</typeparam>
    /// <param name="binder">Function that returns a new <see cref="Result{TOut}"/>.</param>
    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSuccess
            ? binder(_value!)
            : Result<TOut>.CreateFailure(Error!.Value, _errorMessage!);
    }

    /// <summary>
    /// Collapses the result into a single value by applying one of two projection functions.
    /// Eliminates the need for <c>if (result.IsSuccess)</c> branches at the call site.
    /// </summary>
    /// <typeparam name="TOut">The type of the returned value.</typeparam>
    /// <param name="onSuccess">Invoked with the value when the result is successful.</param>
    /// <param name="onFailure">Invoked with the error code and message when the result is a failure.</param>
    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<ErrorCode, string, TOut> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        return IsSuccess
            ? onSuccess(_value!)
            : onFailure(Error!.Value, _errorMessage!);
    }

    /// <summary>
    /// Implicitly wraps a value in a successful <see cref="Result{T}"/>.
    /// Allows natural assignment: <c>Result&lt;int&gt; r = 42;</c>
    /// </summary>
    public static implicit operator Result<T>(T value) => CreateSuccess(value);

    /// <inheritdoc/>
    public override string ToString() => IsSuccess
        ? $"Success({typeof(T).Name})"
        : $"Failure({Error}: {_errorMessage})";
}
