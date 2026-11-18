using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Infrastructure.Persistence.Repositories;

internal sealed class SemesterRepository : ISemesterRepository
{
    private readonly AppDbContext _db;
    public SemesterRepository(AppDbContext db) => _db = db;

    public Task<Semester?> GetByIdAsync(int id, CancellationToken ct = default) =>
        _db.Semesters.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default) =>
        await _db.Semesters.AsNoTracking().OrderByDescending(s => s.StartDate).ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default) =>
        _db.Semesters.AnyAsync(s => s.Code == code && (excludeId == null || s.Id != excludeId), ct);

    public async Task AddAsync(Semester semester, CancellationToken ct = default) =>
        await _db.Semesters.AddAsync(semester, ct);

    public void Update(Semester semester) => _db.Semesters.Update(semester);
    public void Remove(Semester semester) => _db.Semesters.Remove(semester);
}
