using StudentGpa.Application.DTOs;
using StudentGpa.Domain.Common;

namespace StudentGpa.Application.Interfaces;

public interface IEnrollmentService
{
    Task<Result<IReadOnlyList<EnrollmentDto>>> GetByStudentAsync(
        int studentId,
        CancellationToken ct = default);

    Task<Result<EnrollmentDto>> CreateAsync(
        CreateEnrollmentRequest request,
        CancellationToken ct = default);

    Task<Result<EnrollmentDto>> UpdateGradeAsync(
        int enrollmentId,
        UpdateEnrollmentGradeRequest request,
        CancellationToken ct = default);

    Task<Result> DeleteAsync(
        int enrollmentId,
        CancellationToken ct = default);
}
