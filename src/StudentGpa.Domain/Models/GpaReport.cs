namespace StudentGpa.Domain.Models;

public sealed record CourseResultLine(
    int EnrollmentId,
    int CourseId,
    string CourseCode,
    string CourseName,
    int CreditHours,
    decimal? Score,
    string DisplayGrade,
    decimal? GradePoints,
    decimal? QualityPoints,
    bool IsGpaEligible);

public sealed record SemesterGpaReport(
    int SemesterId,
    string SemesterCode,
    string SemesterName,
    DateOnly StartDate,
    IReadOnlyList<CourseResultLine> Courses,
    int RegisteredCreditHours,
    int EligibleCreditHours,
    decimal TotalQualityPoints,
    decimal? Gpa)
{
    public string GpaDisplay => GpaFormatter.Format(Gpa);
}

public sealed record StudentGpaReport(
    int StudentId,
    string StudentCode,
    string StudentName,
    IReadOnlyList<SemesterGpaReport> Semesters,
    int TotalRegisteredCreditHours,
    int TotalEligibleCreditHours,
    decimal TotalQualityPoints,
    decimal? CumulativeGpa)
{
    public string CumulativeGpaDisplay => GpaFormatter.Format(CumulativeGpa);
}
