using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;

namespace StudentGpa.Domain.Interfaces;

public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedList<Student>> GetPagedAsync(string? search, int page, int pageSize, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, int? excludeId, CancellationToken ct = default);
    Task AddAsync(Student student, CancellationToken ct = default);
    void Update(Student student);
    void Remove(Student student);
}
