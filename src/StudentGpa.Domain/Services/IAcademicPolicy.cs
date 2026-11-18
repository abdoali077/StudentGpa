using StudentGpa.Domain.Common;
using StudentGpa.Domain.Enums;

namespace StudentGpa.Domain.Services;

/// <summary>Pure business-rule checks (no I/O). Repositories supply the numbers.</summary>
public interface IAcademicPolicy
{
    /// <summary>Course credit hours must be 1..3.</summary>
    Result ValidateCourseCreditHours(int creditHours);

    /// <summary>Total registered hours (including W and I) must stay within 18 per semester.</summary>
    Result ValidateSemesterCreditLimit(int alreadyRegisteredHours, int newCourseHours);

    /// <summary>Exactly one of score / special status; score in [0,100] with at most 2 decimals.</summary>
    Result ValidateGradeInput(decimal? score, AcademicStatus? status);
}
