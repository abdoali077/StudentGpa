using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Enums;

namespace StudentGpa.Domain.Services;

public sealed class EnrollmentGradingService : IEnrollmentGradingService
{
    private readonly IAcademicPolicy _policy;
    private readonly IGradeResolver _resolver;

    public EnrollmentGradingService(IAcademicPolicy policy, IGradeResolver resolver)
    {
        _policy = policy;
        _resolver = resolver;
    }

    public Result Apply(Enrollment enrollment, decimal? score, AcademicStatus? status, IReadOnlyCollection<GradeScale> scale)
    {
        var validation = _policy.ValidateGradeInput(score, status);
        if (validation.IsFailure)
            return validation;

        if (score.HasValue)
        {
            var resolved = _resolver.Resolve(score.Value, scale);
            if (resolved.IsFailure)
                return Result.Failure(resolved.Errors);

            enrollment.Score = score;
            enrollment.SpecialStatus = null;
            enrollment.LetterGrade = resolved.Value.LetterGrade;
            enrollment.GradePoints = resolved.Value.Points;
            return Result.Success();
        }

        enrollment.Score = null;
        enrollment.SpecialStatus = status;

        switch (status)
        {
            case AcademicStatus.Absent:       // counts in GPA with 0.0 points
                enrollment.LetterGrade = AcademicRules.AbsentGrade;
                enrollment.GradePoints = 0m;
                break;
            case AcademicStatus.Withdrawn:    // excluded from GPA
                enrollment.LetterGrade = AcademicRules.WithdrawnGrade;
                enrollment.GradePoints = null;
                break;
            case AcademicStatus.Incomplete:   // excluded from GPA
                enrollment.LetterGrade = AcademicRules.IncompleteGrade;
                enrollment.GradePoints = null;
                break;
        }

        return Result.Success();
    }
}
