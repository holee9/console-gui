using System.Reflection;
using HnVue.Common.Results;

namespace HnVue.Data;

/// <summary>
/// Internal helpers for constructing <see cref="Result{T}"/> instances with nullable values.
/// The standard <c>Result.Success&lt;T&gt;(null)</c> factory rejects null; these helpers bypass that
/// guard using reflection to support repository methods whose contracts allow returning null on success
/// (e.g., <c>FindByIdAsync</c> returns <c>null</c> when no record is found).
/// </summary>
internal static class ResultExtensions
{
    // Cache the private Result<T>(T value) constructor to avoid per-call reflection overhead.
    private static readonly Type _openGenericResultType = typeof(Result<>);

    /// <summary>
    /// Creates a successful <see cref="Result{T}"/> that wraps a <see langword="null"/> value.
    /// Use this only for repository methods that have a nullable type parameter (e.g., <c>T?</c>).
    /// </summary>
    internal static Result<T> SuccessWithNull<T>() where T : class?
    {
        // Obtain the closed generic type Result<T>
        var closedType = _openGenericResultType.MakeGenericType(typeof(T));

        // Find the private constructor Result<T>(T value)
        var ctor = closedType.GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null,
            types: [typeof(T)],
            modifiers: null)
            ?? throw new InvalidOperationException($"Cannot find private constructor on {closedType.Name}.");

        return (Result<T>)ctor.Invoke([null]);
    }
}
