using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Enums;

namespace StudentGpa.Domain.Entities;

public class Enrollment : BaseEntity
{
    public int StudentId { get; set; }
    public int SemesterId { get; set; }
    public int CourseId { get; set; }

    /// <summary>Snapshot copied from the course at registration time.</summary>
    public int CreditHours { get; set; }

    /// <summary>Numeric score (0..100). Null when a special status is used.</summary>
    public decimal? Score { get; set; }

    /// <summary>Special status. Null when a numeric score is used.</summary>
    public AcademicStatus? SpecialStatus { get; set; }

    /// <summary>Letter grade (A+..F) or ABS / W / I.</summary>
    public string LetterGrade { get; set; } = string.Empty;

    /// <summary>Points from the grading scale. 0 for ABS; null for W and I.</summary>
    public decimal? GradePoints { get; set; }

    public Student? Student { get; set; }
    public Semester? Semester { get; set; }
    public Course? Course { get; set; }

    // ---- Derived (not mapped) ----

    /// <summary>Normal score and ABS count toward GPA; W and I do not.</summary>
    public bool IsGpaEligible => Score.HasValue || SpecialStatus == AcademicStatus.Absent;

    public decimal? QualityPoints => IsGpaEligible && GradePoints.HasValue ? GradePoints.Value * CreditHours : null;

    public string DisplayGrade => SpecialStatus switch
    {
        AcademicStatus.Absent => AcademicRules.AbsentGrade,
        AcademicStatus.Withdrawn => AcademicRules.WithdrawnGrade,
        AcademicStatus.Incomplete => AcademicRules.IncompleteGrade,
        _ => LetterGrade
    };
}
