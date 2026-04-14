using System.Diagnostics.CodeAnalysis;

namespace SnapshotDiff.Infrastructure.Common;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error message.
/// Use <see cref="Success"/> or <see cref="Failure"/> factory methods to construct instances.
/// </summary>
/// <typeparam name="TValue">The type of the value returned on success.</typeparam>
public readonly record struct Result<TValue>
{
    private readonly TValue? _value;
    private readonly string? _error;

    [MemberNotNullWhen(true, nameof(_value))]
    [MemberNotNullWhen(false, nameof(_error))]
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the success value. Throws <see cref="InvalidOperationException"/> if this is a failure result.
    /// </summary>
    public TValue Value => IsSuccess 
        ? _value 
        : throw new InvalidOperationException($"Cannot access Value of a failed result. Error: {_error}");

    /// <summary>
    /// Gets the error message. Throws <see cref="InvalidOperationException"/> if this is a success result.
    /// </summary>
    public string Error => !IsSuccess 
        ? _error 
        : throw new InvalidOperationException("Cannot access Error of a successful result.");

    private Result(TValue value)
    {
        IsSuccess = true;
        _value = value;
        _error = null;
    }

    private Result(string error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    /// <summary>
    /// Creates a successful result wrapping <paramref name="value"/>.
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failed result with the given <paramref name="error"/> message.
    /// </summary>
    public static Result<TValue> Failure(string error) => new(error);

    /// <summary>
    /// Dispatches to <paramref name="onSuccess"/> or <paramref name="onFailure"/> depending on the result state.
    /// </summary>
    public TResult Match<TResult>(
        Func<TValue, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        
        return IsSuccess ? onSuccess(_value) : onFailure(_error);
    }

    /// <summary>
    /// Async variant of <see cref="Match{TResult}"/>.
    /// </summary>
    public async Task<TResult> MatchAsync<TResult>(
        Func<TValue, Task<TResult>> onSuccess,
        Func<string, Task<TResult>> onFailure)
    {
        ArgumentNullException.ThrowIfNull(onSuccess);
        ArgumentNullException.ThrowIfNull(onFailure);
        
        return IsSuccess ? await onSuccess(_value) : await onFailure(_error);
    }

    /// <summary>
    /// Transforms the success value using <paramref name="mapper"/>. Propagates failure unchanged.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        return IsSuccess ? Result<TNew>.Success(mapper(_value)) : Result<TNew>.Failure(_error);
    }

    /// <summary>
    /// Chains another result-returning operation. Propagates failure without calling <paramref name="binder"/>.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder)
    {
        ArgumentNullException.ThrowIfNull(binder);
        return IsSuccess ? binder(_value) : Result<TNew>.Failure(_error);
    }

    /// <summary>
    /// Implicitly wraps a value in a successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue value) => Success(value);
}