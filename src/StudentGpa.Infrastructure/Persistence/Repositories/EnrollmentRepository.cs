using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Infrastructure.Persistence.Repositories;

internal sealed class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _db;
    public EnrollmentRepository(AppDbContext db) => _db = db;

    public Task<Enrollment?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Enrollments
            .Include(e => e.Student)
            .Include(e => e.Semester)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<Enrollment>> GetByStudentAsync(int studentId, CancellationToken ct = default) =>
        await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Semester)
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .OrderBy(e => e.Semester!.StartDate)
            .ThenBy(e => e.Course!.Code)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Enrollment>> GetByStudentAndSemesterAsync(
        int studentId, int semesterId, CancellationToken ct = default) =>
        await _db.Enrollments
            .AsNoTracking()
            .Include(e => e.Semester)
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId && e.SemesterId == semesterId)
            .OrderBy(e => e.Course!.Code)
            .ToListAsync(ct);

    public Task<bool> ExistsAsync(int studentId, int semesterId, int courseId, CancellationToken ct = default) =>
        _db.Enrollments.AnyAsync(
            e => e.StudentId == studentId && e.SemesterId == semesterId && e.CourseId == courseId, ct);

    public async Task<int> GetRegisteredHoursAsync(
        int studentId, int semesterId, int? excludeEnrollmentId = null, CancellationToken ct = default)
    {
        // Includes Withdrawn and Incomplete: every registered course counts toward the 18-hour limit.
        var total = await _db.Enrollments
            .Where(e => e.StudentId == studentId
                        && e.SemesterId == semesterId
                        && (excludeEnrollmentId == null || e.Id != excludeEnrollmentId))
            .SumAsync(e => (int?)e.CreditHours, ct);

        return total ?? 0;
    }

    public Task<bool> AnyForStudentAsync(int studentId, CancellationToken ct = default) =>
        _db.Enrollments.AnyAsync(e => e.StudentId == studentId, ct);

    public Task<bool> AnyForCourseAsync(int courseId, CancellationToken ct = default) =>
        _db.Enrollments.AnyAsync(e => e.CourseId == courseId, ct);

    public Task<bool> AnyForSemesterAsync(int semesterId, CancellationToken ct = default) =>
        _db.Enrollments.AnyAsync(e => e.SemesterId == semesterId, ct);

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct = default) =>
        await _db.Enrollments.AddAsync(enrollment, ct);

    public void Update(Enrollment enrollment) => _db.Enrollments.Update(enrollment);
    public void Remove(Enrollment enrollment) => _db.Enrollments.Remove(enrollment);
}
