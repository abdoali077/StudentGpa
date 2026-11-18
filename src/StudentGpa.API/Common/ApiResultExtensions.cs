using Microsoft.AspNetCore.Mvc;
using StudentGpa.Domain.Common;

namespace StudentGpa.API.Common;

public static class ApiResultExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return CreateProblem(result.Errors);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return CreateProblem(result.Errors);
    }

    private static IActionResult CreateProblem(
        IReadOnlyList<Error> errors)
    {
        var statusCode = GetStatusCode(errors);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(statusCode),
            Detail = "One or more errors occurred."
        };

        problem.Extensions["errors"] = errors.Select(error => new
        {
            error.Code,
            error.Description,
            Type = error.Type.ToString()
        });

        return new ObjectResult(problem)
        {
            StatusCode = statusCode
        };
    }

    private static int GetStatusCode(IReadOnlyList<Error> errors)
    {
        if (errors.Any(e => e.Type == ErrorType.Failure))
            return StatusCodes.Status500InternalServerError;

        if (errors.Any(e => e.Type == ErrorType.Conflict))
            return StatusCodes.Status409Conflict;

        if (errors.Any(e => e.Type == ErrorType.BusinessRule))
            return StatusCodes.Status422UnprocessableEntity;

        if (errors.Any(e => e.Type == ErrorType.NotFound))
            return StatusCodes.Status404NotFound;

        return StatusCodes.Status400BadRequest;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Validation failed",
        StatusCodes.Status404NotFound => "Resource not found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Business rule violation",
        _ => "An unexpected error occurred"
    };
}
