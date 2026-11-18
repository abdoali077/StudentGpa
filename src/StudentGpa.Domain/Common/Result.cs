namespace StudentGpa.Domain.Common;

public class Result
{
    private static readonly IReadOnlyList<Error> NoErrors = Array.Empty<Error>();

    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        if (isSuccess && errors.Count > 0)
            throw new InvalidOperationException("A successful result cannot contain errors.");
        if (!isSuccess && errors.Count == 0)
            throw new InvalidOperationException("A failed result must contain at least one error.");

        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public IReadOnlyList<Error> Errors { get; }
    public Error Error => Errors.Count > 0 ? Errors[0] : Error.None;

    public static Result Success() => new(true, NoErrors);
    public static Result Failure(Error error) => new(false, new[] { error });
    public static Result Failure(IEnumerable<Error> errors) => new(false, errors.ToArray());

    public static Result<T> Success<T>(T value) => new(value, true, NoErrors);
    public static Result<T> Failure<T>(Error error) => new(default, false, new[] { error });
    public static Result<T> Failure<T>(IEnumerable<Error> errors) => new(default, false, errors.ToArray());

    /// <summary>Merges several results; fails with all collected errors if any failed.</summary>
    public static Result Combine(params Result[] results)
    {
        var errors = results.Where(r => r.IsFailure).SelectMany(r => r.Errors).ToArray();
        return errors.Length == 0 ? Success() : Failure(errors);
    }

    public TOut Match<TOut>(Func<TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure)
        => IsSuccess ? onSuccess() : onFailure(Errors);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, IReadOnlyList<Error> errors) : base(isSuccess, errors)
        => _value = value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    public static implicit operator Result<T>(T value) => Result.Success(value);
    public static implicit operator Result<T>(Error error) => Result.Failure<T>(error);

    public Result<TOut> Map<TOut>(Func<T, TOut> map)
        => IsSuccess ? Result.Success(map(_value!)) : Result.Failure<TOut>(Errors);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> bind)
        => IsSuccess ? bind(_value!) : Result.Failure<TOut>(Errors);

    public TOut Match<TOut>(Func<T, TOut> onSuccess, Func<IReadOnlyList<Error>, TOut> onFailure)
        => IsSuccess ? onSuccess(_value!) : onFailure(Errors);
}
