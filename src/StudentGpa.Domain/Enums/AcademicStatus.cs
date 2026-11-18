namespace StudentGpa.Domain.Enums;

/// <summary>Special academic statuses used instead of a numeric score.</summary>
public enum AcademicStatus
{
    /// <summary>ABS - counted in GPA with 0.0 points.</summary>
    Absent = 1,
    /// <summary>W - excluded from GPA.</summary>
    Withdrawn = 2,
    /// <summary>I - excluded from GPA.</summary>
    Incomplete = 3
}
