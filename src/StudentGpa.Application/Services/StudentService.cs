using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Application.Services;

public sealed class StudentService : IStudentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<SaveStudentRequest> _studentValidator;
    private readonly IValidator<ListQuery> _listQueryValidator;

    public StudentService(
        IUnitOfWork unitOfWork,
        IValidator<SaveStudentRequest> studentValidator,
        IValidator<ListQuery> listQueryValidator)
    {
        _unitOfWork = unitOfWork;
        _studentValidator = studentValidator;
        _listQueryValidator = listQueryValidator;
    }

    public async Task<Result<PagedList<StudentDto>>> GetPagedAsync(
        ListQuery query,
        CancellationToken ct = default)
    {
        var validation = await _listQueryValidator.ValidateToResultAsync(query, ct);

        if (validation.IsFailure)
            return validation.ToFailure<PagedList<StudentDto>>();

        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        var students = await _unitOfWork.Students.GetPagedAsync(
            search,
            query.Page,
            query.PageSize,
            ct);

        var result = students.Map(MapToDto);

        return Result.Success(result);
    }

    public async Task<Result<StudentDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation("Students.InvalidId", "Student ID must be greater than zero.");

        var student = await _unitOfWork.Students.GetByIdAsync(id, ct);

        if (student is null)
            return DomainErrors.Students.NotFound;

        return Result.Success(MapToDto(student));
    }

    public async Task<Result<StudentDto>> CreateAsync(
        SaveStudentRequest request,
        CancellationToken ct = default)
    {
        var validation = await _studentValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<StudentDto>();

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Students.CodeExistsAsync(
            code,
            excludeId: null,
            ct);

        if (codeExists)
            return DomainErrors.Students.CodeAlreadyExists(code);

        var student = new Student
        {
            Code = code,
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Students.AddAsync(student, ct);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<StudentDto>();

        return Result.Success(MapToDto(student));
    }

    public async Task<Result<StudentDto>> UpdateAsync(
        int id,
        SaveStudentRequest request,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation("Students.InvalidId", "Student ID must be greater than zero.");

        var validation = await _studentValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<StudentDto>();

        var student = await _unitOfWork.Students.GetByIdAsync(id, ct);

        if (student is null)
            return DomainErrors.Students.NotFound;

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Students.CodeExistsAsync(
            code,
            excludeId: id,
            ct);

        if (codeExists)
            return DomainErrors.Students.CodeAlreadyExists(code);

        student.Code = code;
        student.Name = name;
        student.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Students.Update(student);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<StudentDto>();

        return Result.Success(MapToDto(student));
    }

    public async Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Result.Failure(
                Error.Validation("Students.InvalidId", "Student ID must be greater than zero."));

        var student = await _unitOfWork.Students.GetByIdAsync(id, ct);

        if (student is null)
            return Result.Failure(DomainErrors.Students.NotFound);

        var hasEnrollments = await _unitOfWork.Enrollments.AnyForStudentAsync(id, ct);

        if (hasEnrollments)
            return Result.Failure(DomainErrors.Students.HasEnrollments);

        _unitOfWork.Students.Remove(student);

        return await _unitOfWork.SaveChangesAsync(ct);
    }

    private static StudentDto MapToDto(Student student)
    {
        return new StudentDto(
            student.Id,
            student.Code,
            student.Name);
    }
}
