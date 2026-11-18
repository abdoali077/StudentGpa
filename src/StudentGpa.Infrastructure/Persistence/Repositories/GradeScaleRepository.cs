using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Infrastructure.Persistence.Repositories;

internal sealed class GradeScaleRepository : IGradeScaleRepository
{
    private readonly AppDbContext _db;
    public GradeScaleRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<GradeScale>> GetAllAsync(CancellationToken ct = default) =>
        await _db.GradeScales.AsNoTracking().OrderByDescending(g => g.FromGrade).ToListAsync(ct);
}
