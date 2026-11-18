namespace StudentGpa.Application.DTOs;

public sealed record TranscriptCourseDto(
    int EnrollmentId,
    int CourseId,
    string CourseCode,
    string CourseName,
    int CreditHours,
    decimal? Score,
    string Grade,
    decimal? GradePoints,
    decimal? QualityPoints,
    bool IsGpaEligible);

public sealed record SemesterTranscriptDto(
    int SemesterId,
    string SemesterCode,
    string SemesterName,
    DateOnly StartDate,
    IReadOnlyList<TranscriptCourseDto> Courses,
    int RegisteredCreditHours,
    int EligibleCreditHours,
    decimal TotalQualityPoints,
    decimal? Gpa,
    string GpaDisplay);

public sealed record TranscriptDto(
    int StudentId,
    string StudentCode,
    string StudentName,
    IReadOnlyList<SemesterTranscriptDto> Semesters,
    int TotalRegisteredCreditHours,
    int TotalEligibleCreditHours,
    decimal TotalQualityPoints,
    decimal? CumulativeGpa,
    string CumulativeGpaDisplay);
