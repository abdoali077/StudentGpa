using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Interfaces;
using StudentGpa.Domain.Models;
using StudentGpa.Domain.Services;

namespace StudentGpa.Application.Services;

public sealed class GpaService : IGpaService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGpaCalculator _gpaCalculator;

    public GpaService(
        IUnitOfWork unitOfWork,
        IGpaCalculator gpaCalculator)
    {
        _unitOfWork = unitOfWork;
        _gpaCalculator = gpaCalculator;
    }

    public async Task<Result<TranscriptDto>> GetTranscriptAsync(
        int studentId,
        CancellationToken ct = default)
    {
        if (studentId <= 0)
        {
            return Error.Validation(
                "Students.InvalidId",
                "Student ID must be greater than zero.");
        }

        // 1. Get student.
        var student = await _unitOfWork.Students.GetByIdAsync(
            studentId,
            ct);

        if (student is null)
            return DomainErrors.Students.NotFound;

        // 2. Get all registrations with Course and Semester loaded.
        var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(
            studentId,
            ct);

        // 3. Calculate GPA using Domain logic.
        var report = _gpaCalculator.Calculate(student, enrollments);

        // 4. Map Domain report to Application DTOs.
        var transcript = MapToDto(report);

        return Result.Success(transcript);
    }

    private static TranscriptDto MapToDto(StudentGpaReport report)
    {
        var semesters = report.Semesters
            .Select(semester => new SemesterTranscriptDto(
                semester.SemesterId,
                semester.SemesterCode,
                semester.SemesterName,
                semester.StartDate,
                semester.Courses
                    .Select(course => new TranscriptCourseDto(
                        course.EnrollmentId,
                        course.CourseId,
                        course.CourseCode,
                        course.CourseName,
                        course.CreditHours,
                        course.Score,
                        course.DisplayGrade,
                        course.GradePoints,
                        course.QualityPoints,
                        course.IsGpaEligible))
                    .ToList(),
                semester.RegisteredCreditHours,
                semester.EligibleCreditHours,
                semester.TotalQualityPoints,
                semester.Gpa,
                semester.GpaDisplay))
            .ToList();

        return new TranscriptDto(
            report.StudentId,
            report.StudentCode,
            report.StudentName,
            semesters,
            report.TotalRegisteredCreditHours,
            report.TotalEligibleCreditHours,
            report.TotalQualityPoints,
            report.CumulativeGpa,
            report.CumulativeGpaDisplay);
    }
}
