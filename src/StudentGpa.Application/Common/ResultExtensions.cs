using FluentValidation;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Common;

internal static class ResultExtensions
{
    public static Result<T> ToFailure<T>(this Result result) =>
        Result.Failure<T>(result.Errors);

    public static async Task<Result> ValidateToResultAsync<T>(
        this IValidator<T> validator,
        T instance,
        CancellationToken ct = default)
    {
        var outcome = await validator.ValidateAsync(instance, ct);

        if (outcome.IsValid)
            return Result.Success();

        var errors = outcome.Errors
            .Select(f => Error.Validation(
                !string.IsNullOrEmpty(f.ErrorCode) && f.ErrorCode.Contains('.')
                    ? f.ErrorCode
                    : $"Validation.{f.PropertyName}",
                f.ErrorMessage))
            .Distinct()
            .ToList();

        return Result.Failure(errors);
    }
}
