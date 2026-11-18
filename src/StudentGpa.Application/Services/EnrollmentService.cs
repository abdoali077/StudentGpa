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

public sealed class EnrollmentService : IEnrollmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEnrollmentGradingService _gradingService;
    private readonly IAcademicPolicy _academicPolicy;
    private readonly IValidator<CreateEnrollmentRequest> _createValidator;
    private readonly IValidator<UpdateEnrollmentGradeRequest> _gradeValidator;

    public EnrollmentService(
        IUnitOfWork unitOfWork,
        IEnrollmentGradingService gradingService,
        IAcademicPolicy academicPolicy,
        IValidator<CreateEnrollmentRequest> createValidator,
        IValidator<UpdateEnrollmentGradeRequest> gradeValidator)
    {
        _unitOfWork = unitOfWork;
        _gradingService = gradingService;
        _academicPolicy = academicPolicy;
        _createValidator = createValidator;
        _gradeValidator = gradeValidator;
    }

    public async Task<Result<IReadOnlyList<EnrollmentDto>>> GetByStudentAsync(
        int studentId,
        CancellationToken ct = default)
    {
        if (studentId <= 0)
            return Error.Validation(
                "Students.InvalidId",
                "Student ID must be greater than zero.");

        var student = await _unitOfWork.Students.GetByIdAsync(studentId, ct);

        if (student is null)
            return DomainErrors.Students.NotFound;

        var enrollments = await _unitOfWork.Enrollments.GetByStudentAsync(
            studentId,
            ct);

        var result = enrollments
            .Select(e => MapToDto(e, student))
            .ToList();

        return Result.Success<IReadOnlyList<EnrollmentDto>>(result);
    }

    public async Task<Result<EnrollmentDto>> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<EnrollmentDto>();

        return await _unitOfWork.ExecuteInTransactionWithResultAsync(
            async token =>
            {
                // 1. Verify that the related entities exist.
                var student = await _unitOfWork.Students.GetByIdAsync(
                    request.StudentId, token);

                if (student is null)
                    return Result.Failure<EnrollmentDto>(DomainErrors.Students.NotFound);

                var course = await _unitOfWork.Courses.GetByIdAsync(
                    request.CourseId, token);

                if (course is null)
                    return Result.Failure<EnrollmentDto>(DomainErrors.Courses.NotFound);

                var semester = await _unitOfWork.Semesters.GetByIdAsync(
                    request.SemesterId, token);

                if (semester is null)
                    return Result.Failure<EnrollmentDto>(DomainErrors.Semesters.NotFound);

                // 2. Validate the course credit hours.
                var creditValidation =
                    _academicPolicy.ValidateCourseCreditHours(course.CreditHours);

                if (creditValidation.IsFailure)
                    return creditValidation.ToFailure<EnrollmentDto>();

                // 3. Prevent duplicate registration.
                var alreadyExists = await _unitOfWork.Enrollments.ExistsAsync(
                    request.StudentId,
                    request.SemesterId,
                    request.CourseId,
                    token);

                if (alreadyExists)
                    return Result.Failure<EnrollmentDto>(DomainErrors.Enrollments.Duplicate);

                // 4. Check the semester credit-hour limit.
                var registeredHours =
                    await _unitOfWork.Enrollments.GetRegisteredHoursAsync(
                        request.StudentId,
                        request.SemesterId,
                        ct: token);

                var limitValidation =
                    _academicPolicy.ValidateSemesterCreditLimit(
                        registeredHours,
                        course.CreditHours);

                if (limitValidation.IsFailure)
                    return limitValidation.ToFailure<EnrollmentDto>();

                // 5. Load the grading scale only when a numeric score is provided.
                IReadOnlyList<GradeScale> scale = request.Score.HasValue
                    ? await _unitOfWork.GradeScales.GetAllAsync(token)
                    : Array.Empty<GradeScale>();

                // 6. Create the enrollment and preserve the credit-hour snapshot.
                var enrollment = new Enrollment
                {
                    StudentId = student.Id,
                    SemesterId = semester.Id,
                    CourseId = course.Id,
                    CreditHours = course.CreditHours,
                    Student = student,
                    Semester = semester,
                    Course = course,
                    CreatedAtUtc = DateTime.UtcNow
                };

                // 7. Apply grade/status business rules.
                var gradingResult = _gradingService.Apply(
                    enrollment,
                    request.Score,
                    request.SpecialStatus,
                    scale);

                if (gradingResult.IsFailure)
                    return gradingResult.ToFailure<EnrollmentDto>();

                // 8. Persist inside the same transaction.
                await _unitOfWork.Enrollments.AddAsync(enrollment, token);

                var saveResult = await _unitOfWork.SaveChangesAsync(token);

                if (saveResult.IsFailure)
                    return saveResult.ToFailure<EnrollmentDto>();

                return Result.Success(MapToDto(enrollment, student));
            },
            ct);
    }

    public async Task<Result<EnrollmentDto>> UpdateGradeAsync(
        int enrollmentId,
        UpdateEnrollmentGradeRequest request,
        CancellationToken ct = default)
    {
        if (enrollmentId <= 0)
            return Error.Validation(
                "Enrollments.InvalidId",
                "Enrollment ID must be greater than zero.");

        var validation = await _gradeValidator.ValidateToResultAsync(request, ct);

        if (validation.IsFailure)
            return validation.ToFailure<EnrollmentDto>();

        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(
            enrollmentId,
            ct);

        if (enrollment is null)
            return DomainErrors.Enrollments.NotFound;

        IReadOnlyList<GradeScale> scale = request.Score.HasValue
            ? await _unitOfWork.GradeScales.GetAllAsync(ct)
            : Array.Empty<GradeScale>();

        var gradingResult = _gradingService.Apply(
            enrollment,
            request.Score,
            request.SpecialStatus,
            scale);

        if (gradingResult.IsFailure)
            return gradingResult.ToFailure<EnrollmentDto>();

        enrollment.UpdatedAtUtc = DateTime.UtcNow;

        _unitOfWork.Enrollments.Update(enrollment);

        var saveResult = await _unitOfWork.SaveChangesAsync(ct);

        if (saveResult.IsFailure)
            return saveResult.ToFailure<EnrollmentDto>();

        return Result.Success(MapToDto(enrollment));
    }

    public async Task<Result> DeleteAsync(
        int enrollmentId,
        CancellationToken ct = default)
    {
        if (enrollmentId <= 0)
            return Result.Failure(
                Error.Validation(
                    "Enrollments.InvalidId",
                    "Enrollment ID must be greater than zero."));

        var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(
            enrollmentId,
            ct);

        if (enrollment is null)
            return Result.Failure(DomainErrors.Enrollments.NotFound);

        _unitOfWork.Enrollments.Remove(enrollment);

        return await _unitOfWork.SaveChangesAsync(ct);
    }

    private static EnrollmentDto MapToDto(
        Enrollment enrollment,
        Student? student = null)
    {
        var actualStudent = student ?? enrollment.Student
            ?? throw new InvalidOperationException(
                "Student navigation property must be loaded.");

        var semester = enrollment.Semester
            ?? throw new InvalidOperationException(
                "Semester navigation property must be loaded.");

        var course = enrollment.Course
            ?? throw new InvalidOperationException(
                "Course navigation property must be loaded.");

        return new EnrollmentDto(
            enrollment.Id,
            enrollment.StudentId,
            actualStudent.Code,
            actualStudent.Name,
            enrollment.SemesterId,
            semester.Code,
            semester.Name,
            enrollment.CourseId,
            course.Code,
            course.Name,
            enrollment.CreditHours,
            enrollment.Score,
            enrollment.SpecialStatus,
            enrollment.DisplayGrade,
            enrollment.GradePoints,
            enrollment.QualityPoints,
            enrollment.IsGpaEligible);
    }
}
