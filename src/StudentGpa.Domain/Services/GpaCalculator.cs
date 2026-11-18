using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Models;

namespace StudentGpa.Domain.Services;

public sealed class GpaCalculator : IGpaCalculator
{
    public StudentGpaReport Calculate(Student student, IEnumerable<Enrollment> enrollments)
    {
        var semesterGroups = enrollments
            .GroupBy(e => e.SemesterId)
            .Select(g => new
            {
                Semester = g.First().Semester
                           ?? throw new InvalidOperationException("Enrollment.Semester must be loaded."),
                Items = g.OrderBy(e => e.Course?.Code).ToList()
            })
            .OrderBy(x => x.Semester.StartDate)
            .ThenBy(x => x.Semester.Id)
            .ToList();

        var semesterReports = new List<SemesterGpaReport>();
        int totalRegistered = 0, totalEligibleHours = 0;
        decimal totalQualityPoints = 0m;

        foreach (var group in semesterGroups)
        {
            var lines = group.Items.Select(ToLine).ToList();

            // W and I are excluded from both numerator and denominator; ABS stays in with 0 points.
            var eligible = group.Items.Where(e => e.IsGpaEligible).ToList();

            var registeredHours = group.Items.Sum(e => e.CreditHours);          // includes W and I
            var eligibleHours = eligible.Sum(e => e.CreditHours);
            var qualityPoints = eligible.Sum(e => e.QualityPoints ?? 0m);

            decimal? gpa = eligibleHours > 0 ? qualityPoints / eligibleHours : null;   // null => "N/A"

            semesterReports.Add(new SemesterGpaReport(
                group.Semester.Id, group.Semester.Code, group.Semester.Name, group.Semester.StartDate,
                lines, registeredHours, eligibleHours, qualityPoints, gpa));

            totalRegistered += registeredHours;
            totalEligibleHours += eligibleHours;
            totalQualityPoints += qualityPoints;
        }

        // Weighted by credit hours: total quality points / total eligible hours (NOT an average of semester GPAs).
        decimal? cumulative = totalEligibleHours > 0 ? totalQualityPoints / totalEligibleHours : null;

        return new StudentGpaReport(
            student.Id, student.Code, student.Name, semesterReports,
            totalRegistered, totalEligibleHours, totalQualityPoints, cumulative);
    }

    private static CourseResultLine ToLine(Enrollment e) => new(
        e.Id,
        e.CourseId,
        e.Course?.Code ?? string.Empty,
        e.Course?.Name ?? string.Empty,
        e.CreditHours,
        e.Score,
        e.DisplayGrade,
        e.GradePoints,
        e.QualityPoints,
        e.IsGpaEligible);
}
