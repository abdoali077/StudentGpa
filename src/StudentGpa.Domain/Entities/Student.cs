using StudentGpa.Domain.Common;

namespace StudentGpa.Domain.Entities;

public class Student : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
