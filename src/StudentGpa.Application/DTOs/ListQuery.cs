namespace StudentGpa.Application.DTOs;

public sealed record ListQuery(string? Search = null, int Page = 1, int PageSize = 20);
