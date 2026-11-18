namespace StudentGpa.Application.DTOs;

public sealed record SaveSemesterRequest(
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);

public sealed record SemesterDto(
    int Id,
    string Code,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate);
