using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Enums;
using StudentGpa.Domain.Services;

namespace StudentGpa.Infrastructure.Persistence.Seed;

public static class DatabaseInitializer
{
    /// <summary>Applies migrations (which also seed the grading scale) and inserts sample data once.</summary>
    public static async Task InitialiseDatabaseAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var grading = scope.ServiceProvider.GetRequiredService<IEnrollmentGradingService>();

        await db.Database.MigrateAsync(ct);
        await SampleDataSeeder.SeedAsync(db, grading, ct);
    }
}

internal static class SampleDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, IEnrollmentGradingService grading, CancellationToken ct)
    {
        if (await db.Students.AnyAsync(ct))
            return;

        var scale = await db.GradeScales.AsNoTracking().OrderByDescending(g => g.FromGrade).ToListAsync(ct);

        var students = new[]
        {
            new Student { Code = "STU-001", Name = "Ahmed Hassan" },
            new Student { Code = "STU-002", Name = "Sara Ali" }
        };

        var courses = new Dictionary<string, Course>
        {
            ["CS101"]   = new() { Code = "CS101",   Name = "Introduction to Programming", CreditHours = 3 },
            ["MATH101"] = new() { Code = "MATH101", Name = "Calculus I",                  CreditHours = 2 },
            ["ENG101"]  = new() { Code = "ENG101",  Name = "English Composition",         CreditHours = 3 },
            ["PHY101"]  = new() { Code = "PHY101",  Name = "General Physics",             CreditHours = 2 },
            ["IT101"]   = new() { Code = "IT101",   Name = "IT Fundamentals",             CreditHours = 3 },
            ["CS102"]   = new() { Code = "CS102",   Name = "Data Structures",             CreditHours = 3 },
            ["MATH102"] = new() { Code = "MATH102", Name = "Calculus II",                 CreditHours = 2 }
        };

        var sem1 = new Semester { Code = "2025-FALL",   Name = "Fall 2025",   StartDate = new DateOnly(2025, 9, 1), EndDate = new DateOnly(2026, 1, 15) };
        var sem2 = new Semester { Code = "2026-SPRING", Name = "Spring 2026", StartDate = new DateOnly(2026, 2, 1), EndDate = new DateOnly(2026, 6, 15) };

        db.Students.AddRange(students);
        db.Courses.AddRange(courses.Values);
        db.Semesters.AddRange(sem1, sem2);

        void Enroll(Student student, Semester semester, string courseCode, decimal? score, AcademicStatus? status)
        {
            var course = courses[courseCode];
            var enrollment = new Enrollment
            {
                Student = student,
                Semester = semester,
                Course = course,
                CreditHours = course.CreditHours
            };

            var result = grading.Apply(enrollment, score, status, scale);
            if (result.IsFailure)
                throw new InvalidOperationException($"Seed failed for {student.Code}/{courseCode}: {result.Error.Description}");

            db.Enrollments.Add(enrollment);
        }

        // Student 1 = the verification example from the task: Sem1 GPA 2.08, Sem2 GPA 3.36, Cumulative 2.57
        var s1 = students[0];
        Enroll(s1, sem1, "CS101",   90,   null);
        Enroll(s1, sem1, "MATH101", 85,   null);
        Enroll(s1, sem1, "ENG101",  null, AcademicStatus.Absent);
        Enroll(s1, sem1, "PHY101",  null, AcademicStatus.Withdrawn);
        Enroll(s1, sem1, "IT101",   null, AcademicStatus.Incomplete);
        Enroll(s1, sem2, "CS102",   96,   null);
        Enroll(s1, sem2, "MATH102", 70,   null);

        // Student 2 = boundary values
        var s2 = students[1];
        Enroll(s2, sem1, "CS101",   95.99m, null);   // A  (upper bound is exclusive)
        Enroll(s2, sem1, "MATH101", 55,     null);   // D  (lower bound is inclusive)
        Enroll(s2, sem1, "ENG101",  100,    null);   // A+
        Enroll(s2, sem2, "CS102",   49.99m, null);   // F
        Enroll(s2, sem2, "MATH102", null,   AcademicStatus.Incomplete);

        await db.SaveChangesAsync(ct);
    }
}
