using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Enums;
using StudentGpa.Domain.Errors;

namespace StudentGpa.Domain.Services;

public sealed class AcademicPolicy : IAcademicPolicy
{
    public Result ValidateCourseCreditHours(int creditHours) =>
        creditHours is >= AcademicRules.MinCourseCreditHours and <= AcademicRules.MaxCourseCreditHours
            ? Result.Success()
            : Result.Failure(DomainErrors.Courses.InvalidCreditHours);

    public Result ValidateSemesterCreditLimit(int alreadyRegisteredHours, int newCourseHours) =>
        alreadyRegisteredHours + newCourseHours <= AcademicRules.MaxSemesterCreditHours
            ? Result.Success()
            : Result.Failure(DomainErrors.Enrollments.CreditLimitExceeded(alreadyRegisteredHours, newCourseHours));

    public Result ValidateGradeInput(decimal? score, AcademicStatus? status)
    {
        if (score is null && status is null)
            return Result.Failure(DomainErrors.Grades.GradeRequired);

        if (score is not null && status is not null)
            return Result.Failure(DomainErrors.Grades.ScoreAndStatusConflict);

        if (status is not null)
            return Enum.IsDefined(status.Value)
                ? Result.Success()
                : Result.Failure(DomainErrors.Grades.InvalidStatus);

        var value = score!.Value;

        if (value < AcademicRules.MinScore || value > AcademicRules.MaxScore)
            return Result.Failure(DomainErrors.Grades.ScoreOutOfRange);

        // 95.999 would be stored as 96.00 in decimal(5,2) and silently change the grade, so reject it.
        if (decimal.Round(value, AcademicRules.ScoreDecimalPlaces) != value)
            return Result.Failure(DomainErrors.Grades.ScorePrecision);

        return Result.Success();
    }
}
