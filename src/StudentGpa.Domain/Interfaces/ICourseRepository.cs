using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Domain.Interfaces;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedList<Course>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default);
    Task AddAsync(Course course, CancellationToken ct = default);
    void Update(Course course);
    void Remove(Course course);
}
