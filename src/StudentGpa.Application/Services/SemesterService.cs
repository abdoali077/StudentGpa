using FluentValidation;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;
using StudentGpa.Application.Common;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Application.Services;

public sealed class SemesterService : ISemesterService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SaveSemesterRequest> _semesterValidator;

    public SemesterService(
        IUnitOfWork unitOfWork,
        IValidator<SaveSemesterRequest> semesterValidator)
    {
        _unitOfWork = unitOfWork;
        _semesterValidator = semesterValidator;
    }

    public async Task<Result<IReadOnlyList<SemesterDto>>> GetAllAsync(
        CancellationToken ct = default)
    {
        var semesters = await _unitOfWork.Semesters.GetAllAsync(ct);

        var result = semesters
            .Select(MapToDto)
            .ToList();

        return Result.Success<IReadOnlyList<SemesterDto>>(result);
    }

    public async Task<Result<SemesterDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation(
                "Semesters.InvalidId",
                "Semester ID must be greater than zero.");

        var semester = await _unitOfWork.Semesters.GetByIdAsync(id, ct);

        if (semester is null)
            return DomainErrors.Semesters.NotFound;

        return Result.Success(MapToDto(semester));
    }

    public async Task<Result<SemesterDto>> CreateAsync(
        SaveSemesterRequest request,
        CancellationToken ct = default)
    {
        var validation = await _semesterValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<SemesterDto>();

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Semesters.CodeExistsAsync(
            code,
            excludeId: null,
            ct);

        if (codeExists)
            return DomainErrors.Semesters.CodeAlreadyExists(code);

        var semester = new Semester
        {
            Code = code,
            Name = name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Semesters.AddAsync(semester, ct);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<SemesterDto>();

        return Result.Success(MapToDto(semester));
    }

    public async Task<Result<SemesterDto>> UpdateAsync(
        int id,
        SaveSemesterRequest request,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation(
                "Semesters.InvalidId",
                "Semester ID must be greater than zero.");

        var validation = await _semesterValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<SemesterDto>();

        var semester = await _unitOfWork.Semesters.GetByIdAsync(id, ct);

        if (semester is null)
            return DomainErrors.Semesters.NotFound;

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Semesters.CodeExistsAsync(
            code,
            excludeId: id,
            ct);

        if (codeExists)
            return DomainErrors.Semesters.CodeAlreadyExists(code);

        semester.Code = code;
        semester.Name = name;
        semester.StartDate = request.StartDate;
        semester.EndDate = request.EndDate;
        semester.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Semesters.Update(semester);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<SemesterDto>();

        return Result.Success(MapToDto(semester));
    }

    public async Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Result.Failure(
                Error.Validation(
                    "Semesters.InvalidId",
                    "Semester ID must be greater than zero."));

        var semester = await _unitOfWork.Semesters.GetByIdAsync(id, ct);

        if (semester is null)
            return Result.Failure(DomainErrors.Semesters.NotFound);

        var hasEnrollments =
            await _unitOfWork.Enrollments.AnyForSemesterAsync(id, ct);

        if (hasEnrollments)
            return Result.Failure(DomainErrors.Semesters.HasEnrollments);

        _unitOfWork.Semesters.Remove(semester);

        return await _unitOfWork.SaveChangesAsync(ct);
    }

    private static SemesterDto MapToDto(Semester semester)
    {
        return new SemesterDto(
            semester.Id,
            semester.Code,
            semester.Name,
            semester.StartDate,
            semester.EndDate);
    }
}
