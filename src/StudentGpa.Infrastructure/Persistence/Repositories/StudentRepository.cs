using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Interfaces;
using StudentGpa.Infrastructure.Persistence.Extensions;

namespace StudentGpa.Infrastructure.Persistence.Repositories;

internal sealed class StudentRepository : IStudentRepository
{
    private readonly AppDbContext _db;
    public StudentRepository(AppDbContext db) => _db = db;

    public Task<Student?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Students.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<PagedList<Student>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.Students.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Code.Contains(term) || s.Name.Contains(term));
        }

        return query.OrderBy(s => s.Code).ToPagedListAsync(page, pageSize, ct);
    }

    public Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default) =>
        _db.Students.AnyAsync(s => s.Code == code && (excludeId == null || s.Id != excludeId), ct);

    public async Task AddAsync(Student student, CancellationToken ct = default) =>
        await _db.Students.AddAsync(student, ct);

    public void Update(Student student) => _db.Students.Update(student);
    public void Remove(Student student) => _db.Students.Remove(student);
}
