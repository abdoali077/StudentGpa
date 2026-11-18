using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Application.Services;

public sealed class GradeScaleService : IGradeScaleService
{
    private readonly IUnitOfWork _unitOfWork;

    public GradeScaleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<GradeScaleDto>>> GetAllAsync(
        CancellationToken ct = default)
    {
        var gradeScales = await _unitOfWork.GradeScales.GetAllAsync(ct);

        var result = gradeScales
            .Select(x => new GradeScaleDto(
                x.FromGrade,
                x.ToGrade,
                x.Points,
                x.Grade))
            .ToList();

        return Result.Success<IReadOnlyList<GradeScaleDto>>(result);
    }
}
