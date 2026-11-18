using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Models;

namespace StudentGpa.Domain.Services;

public interface IGpaCalculator
{
    /// <summary>
    /// Builds the full transcript with semester and cumulative GPA.
    /// Enrollments must be loaded with Course and Semester.
    /// </summary>
    StudentGpaReport Calculate(Student student, IEnumerable<Enrollment> enrollments);
}
