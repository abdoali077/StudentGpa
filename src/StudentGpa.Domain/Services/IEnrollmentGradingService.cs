using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Enums;

namespace StudentGpa.Domain.Services;

public interface IEnrollmentGradingService
{
    /// <summary>
    /// Validates the input and sets Score / SpecialStatus / LetterGrade / GradePoints on the enrollment.
    /// Used for both creating and editing a registration, so recalculation always sees fresh values.
    /// </summary>
    Result Apply(Enrollment enrollment, decimal? score, AcademicStatus? status, IReadOnlyCollection<GradeScale> scale);
}
