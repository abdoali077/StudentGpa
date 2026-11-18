using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface ICourseService
{
    Task<Result<PagedList<CourseDto>>> GetPagedAsync(
        ListQuery query,
        CancellationToken ct = default);

    Task<Result<CourseDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default);

    Task<Result<CourseDto>> CreateAsync(
        SaveCourseRequest request,
        CancellationToken ct = default);

    Task<Result<CourseDto>> UpdateAsync(
        int id,
        SaveCourseRequest request,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default);
}
