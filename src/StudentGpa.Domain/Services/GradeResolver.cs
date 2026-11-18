using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Models;

namespace StudentGpa.Domain.Services;

public sealed class GradeResolver : IGradeResolver
{
    public Result<GradeResult> Resolve(decimal score, IReadOnlyCollection<GradeScale> scale)
    {
        if (score < AcademicRules.MinScore || score > AcademicRules.MaxScore)
            return DomainErrors.Grades.ScoreOutOfRange;

        if (scale.Count == 0)
            return DomainErrors.Grades.ScaleNotConfigured;

        // Inclusive lower bound, exclusive upper bound: 96 -> A+, 95.99 -> A, 100 -> A+ (ToGrade = 101).
        var band = scale.FirstOrDefault(b => score >= b.FromGrade && score < b.ToGrade);
        if (band is null)
            return DomainErrors.Grades.NoMatchingBand(score);

        return new GradeResult(band.Grade, band.Points);
    }
}
