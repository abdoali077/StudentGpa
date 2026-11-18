using StudentGpa.Domain.Enums;

namespace StudentGpa.Application.DTOs;

public sealed record CreateEnrollmentRequest(
    int StudentId,
    int SemesterId,
    int CourseId,
    decimal? Score,
    AcademicStatus? SpecialStatus);

public sealed record UpdateEnrollmentGradeRequest(
    decimal? Score,
    AcademicStatus? SpecialStatus);

public sealed record EnrollmentDto(
    int Id,
    int StudentId,
    string StudentCode,
    string StudentName,
    int SemesterId,
    string SemesterCode,
    string SemesterName,
    int CourseId,
    string CourseCode,
    string CourseName,
    int CreditHours,
    decimal? Score,
    AcademicStatus? SpecialStatus,
    string Grade,
    decimal? GradePoints,
    decimal? QualityPoints,
    bool IsGpaEligible);
