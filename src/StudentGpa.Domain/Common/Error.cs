namespace StudentGpa.Domain.Common;

/// <summary>Maps 1:1 to an HTTP status in the API layer (Validation=400, NotFound=404, Conflict=409, BusinessRule=422, Failure=500).</summary>
public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    BusinessRule = 4
}

public sealed record Error(string Code, string Description, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error BusinessRule(string code, string description) => new(code, description, ErrorType.BusinessRule);
}
