using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface IStudentService
{
    Task<Result<PagedList<StudentDto>>> GetPagedAsync(
        ListQuery query,
        CancellationToken ct = default);

    Task<Result<StudentDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<Result<StudentDto>> CreateAsync(
        SaveStudentRequest request,
        CancellationToken ct = default);

    Task<Result<StudentDto>> UpdateAsync(
        int id,
        SaveStudentRequest request,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default);
}
