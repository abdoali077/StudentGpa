using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Interfaces;
using StudentGpa.Infrastructure.Persistence.Extensions;

namespace StudentGpa.Infrastructure.Persistence.Repositories;

internal sealed class CourseRepository : ICourseRepository
{
    private readonly AppDbContext _db;
    public CourseRepository(AppDbContext db) => _db = db;

    public Task<Course?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<PagedList<Course>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Courses.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c => c.Code.Contains(term) || c.Name.Contains(term));
        }

        return query.OrderBy(c => c.Code).ToPagedListAsync(page, pageSize, ct);
    }

    public Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default) =>
        _db.Courses.AnyAsync(c => c.Code == code && (excludeId == null || c.Id != excludeId), ct);

    public async Task AddAsync(Course course, CancellationToken ct = default) =>
        await _db.Courses.AddAsync(course, ct);

    public void Update(Course course) => _db.Courses.Update(course);
    public void Remove(Course course) => _db.Courses.Remove(course);
}
