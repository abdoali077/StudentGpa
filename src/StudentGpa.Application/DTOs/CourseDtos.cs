namespace StudentGpa.Application.DTOs;

public sealed record SaveCourseRequest(string Code, string Name, int CreditHours);

public sealed record CourseDto(int Id, string Code, string Name, int CreditHours);
