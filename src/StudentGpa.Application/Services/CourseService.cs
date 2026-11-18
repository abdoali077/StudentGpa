using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;
using StudentGpa.Application.Interfaces;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Interfaces;
using StudentGpa.Domain.Services;

namespace StudentGpa.Application.Services;

public sealed class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAcademicPolicy _academicPolicy;
    private readonly IValidator<SaveCourseRequest> _courseValidator;
    private readonly IValidator<ListQuery> _listQueryValidator;

    public CourseService(
        IUnitOfWork unitOfWork,
        IAcademicPolicy academicPolicy,
        IValidator<SaveCourseRequest> courseValidator,
        IValidator<ListQuery> listQueryValidator)
    {
        _unitOfWork = unitOfWork;
        _academicPolicy = academicPolicy;
        _courseValidator = courseValidator;
        _listQueryValidator = listQueryValidator;
    }

    public async Task<Result<PagedList<CourseDto>>> GetPagedAsync(
        ListQuery query,
        CancellationToken ct = default)
    {
        var validation = await _listQueryValidator.ValidateToResultAsync(query, ct);

        if (validation.IsFailure)
            return validation.ToFailure<PagedList<CourseDto>>();

        var search = string.IsNullOrWhiteSpace(query.Search)
            ? null
            : query.Search.Trim();

        var courses = await _unitOfWork.Courses.GetPagedAsync(
            search,
            query.Page,
            query.PageSize,
            ct);

        return Result.Success(courses.Map(MapToDto));
    }

    public async Task<Result<CourseDto>> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation("Courses.InvalidId", "Course ID must be greater than zero.");

        var course = await _unitOfWork.Courses.GetByIdAsync(id, ct);

        if (course is null)
            return DomainErrors.Courses.NotFound;

        return Result.Success(MapToDto(course));
    }

    public async Task<Result<CourseDto>> CreateAsync(
        SaveCourseRequest request,
        CancellationToken ct = default)
    {
        var validation = await _courseValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<CourseDto>();

        var creditHoursResult =
            _academicPolicy.ValidateCourseCreditHours(request.CreditHours);

        if (creditHoursResult.IsFailure)
            return creditHoursResult.ToFailure<CourseDto>();

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Courses.CodeExistsAsync(
            code,
            excludeId: null,
            ct);

        if (codeExists)
            return DomainErrors.Courses.CodeAlreadyExists(code);

        var course = new Course
        {
            Code = code,
            Name = name,
            CreditHours = request.CreditHours,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Courses.AddAsync(course, ct);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<CourseDto>();

        return Result.Success(MapToDto(course));
    }

    public async Task<Result<CourseDto>> UpdateAsync(
        int id,
        SaveCourseRequest request,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Error.Validation("Courses.InvalidId", "Course ID must be greater than zero.");

        var validation = await _courseValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<CourseDto>();

        var creditHoursResult =
            _academicPolicy.ValidateCourseCreditHours(request.CreditHours);

        if (creditHoursResult.IsFailure)
            return creditHoursResult.ToFailure<CourseDto>();

        var course = await _unitOfWork.Courses.GetByIdAsync(id, ct);

        if (course is null)
            return DomainErrors.Courses.NotFound;

        var code = TextNormalizer.Code(request.Code);
        var name = TextNormalizer.Name(request.Name);

        var codeExists = await _unitOfWork.Courses.CodeExistsAsync(
            code,
            excludeId: id,
            ct);

        if (codeExists)
            return DomainErrors.Courses.CodeAlreadyExists(code);

        if (course.CreditHours != request.CreditHours)
        {
            var hasEnrollments =
                await _unitOfWork.Enrollments.AnyForCourseAsync(id, ct);

            if (hasEnrollments)
                return DomainErrors.Courses.CreditHoursLocked;
        }

        course.Code = code;
        course.Name = name;
        course.CreditHours = request.CreditHours;
        course.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Courses.Update(course);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<CourseDto>();

        return Result.Success(MapToDto(course));
    }

    public async Task<Result> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        if (id <= 0)
            return Result.Failure(
                Error.Validation("Courses.InvalidId", "Course ID must be greater than zero."));

        var course = await _unitOfWork.Courses.GetByIdAsync(id, ct);

        if (course is null)
            return Result.Failure(DomainErrors.Courses.NotFound);

        var hasEnrollments =
            await _unitOfWork.Enrollments.AnyForCourseAsync(id, ct);

        if (hasEnrollments)
            return Result.Failure(DomainErrors.Courses.HasEnrollments);

        _unitOfWork.Courses.Remove(course);

        return await _unitOfWork.SaveChangesAsync(ct);
    }

    private static CourseDto MapToDto(Course course)
    {
        return new CourseDto(
            course.Id,
            course.Code,
            course.Name,
            course.CreditHours);
    }
}
