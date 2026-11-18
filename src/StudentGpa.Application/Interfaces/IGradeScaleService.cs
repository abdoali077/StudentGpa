using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface IGradeScaleService
{
    Task<Result<IReadOnlyList<GradeScaleDto>>> GetAllAsync(
        CancellationToken ct = default);
}
