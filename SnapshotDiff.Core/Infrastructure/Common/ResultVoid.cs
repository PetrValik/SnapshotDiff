using System.Diagnostics.CodeAnalysis;

namespace SnapshotDiff.Infrastructure.Common;

/// <summary>
/// Represents the result of an operation that can succeed or fail without returning a value.
/// Use <see cref="Success"/> or <see cref="Failure"/> factory methods to construct instances.
/// </summary>
internal readonly record struct Result
{
    private readonly string? _error;

    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the error message. Throws <see cref="InvalidOperationException"/> if this is a success result.
    /// </summary>
    public string Error => !IsSuccess 
        ? _error 
        : throw new InvalidOperationException("Cannot access Error of a successful result.");

    private Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true);

    /// <summary>
    /// Creates a failed result with the given <paramref name="error"/> message.
    /// </summary>
    public static Result Failure(string error) => new(false, error);

    /// <summary>
    /// Dispatches to <paramref name="onSuccess"/> or <paramref name="onFailure"/> depending on the result state.
    /// </summary>
    public TResult Match<TResult>(
        Func<TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        
        return IsSuccess ? onSuccess() : onFailure(_error!);
    }
}
