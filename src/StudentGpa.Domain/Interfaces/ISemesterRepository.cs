using StudentGpa.Domain.Entities;

namespace StudentGpa.Domain.Interfaces;

public interface ISemesterRepository
{
    Task<Semester?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default);
    Task AddAsync(Semester semester, CancellationToken ct = default);
    void Update(Semester semester);
    void Remove(Semester semester);
}
