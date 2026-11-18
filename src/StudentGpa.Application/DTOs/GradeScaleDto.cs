namespace StudentGpa.Application.DTOs;

public sealed record GradeScaleDto(
    decimal FromGrade,
    decimal ToGrade,
    decimal Points,
    string Grade);
