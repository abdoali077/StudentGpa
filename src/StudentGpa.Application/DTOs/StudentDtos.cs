namespace StudentGpa.Application.DTOs;

public sealed record SaveStudentRequest(string Code, string Name);

public sealed record StudentDto(int Id, string Code, string Name);
