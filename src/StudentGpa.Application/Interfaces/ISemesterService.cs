using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface ISemesterService
{
    Task<Result<IReadOnlyList<SemesterDto>>> GetAllAsync(
        CancellationToken ct = default);

    Task<Result<SemesterDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<Result<SemesterDto>> CreateAsync(
        SaveSemesterRequest request,
        CancellationToken ct = default);

    Task<Result<SemesterDto>> UpdateAsync(
        int id,
        SaveSemesterRequest request,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default);
}
