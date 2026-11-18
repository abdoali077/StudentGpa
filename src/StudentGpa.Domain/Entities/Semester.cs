using StudentGpa.Domain.Common;

namespace StudentGpa.Domain.Entities;

public class Semester : BaseEntity
{
    public string Code { get; set; } = string.Empty;   // e.g. 2025-FALL (unique)
    public string Name { get; set; } = string.Empty;   // e.g. Fall 2025
    public DateOnly StartDate { get; set; }            // used for chronological ordering
    public DateOnly EndDate { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
