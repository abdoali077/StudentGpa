using StudentGpa.Domain.Entities;

namespace StudentGpa.Domain.Interfaces;

public interface IEnrollmentRepository
{
    /// <summary>Tracked, with Student, Semester and Course loaded.</summary>
    Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Read-only, with Course and Semester loaded, ordered by semester start date then course code.</summary>
    Task<IReadOnlyList<Enrollment>> GetByStudentAsync(int studentId, CancellationToken ct = default);

    Task<IReadOnlyList<Enrollment>> GetByStudentAndSemesterAsync(int studentId, int semesterId, CancellationToken ct = default);

    Task<bool> ExistsAsync(int studentId, int semesterId, int courseId, CancellationToken ct = default);

    /// <summary>Sum of credit hours of ALL registrations in the semester, including W and I.</summary>
    Task<int> GetRegisteredHoursAsync(int studentId, int semesterId, int? excludeEnrollmentId = null, CancellationToken ct = default);

    Task<bool> AnyForStudentAsync(int studentId, CancellationToken ct = default);
    Task<bool> AnyForCourseAsync(int courseId, CancellationToken ct = default);
    Task<bool> AnyForSemesterAsync(int semesterId, CancellationToken ct = default);

    Task AddAsync(Enrollment enrollment, CancellationToken ct = default);
    void Update(Enrollment enrollment);
    void Remove(Enrollment enrollment);
}
