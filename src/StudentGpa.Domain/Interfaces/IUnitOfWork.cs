using StudentGpa.Domain.Common;

namespace StudentGpa.Domain.Interfaces;

public interface IUnitOfWork
{
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    ISemesterRepository Semesters { get; }
    IGradeScaleRepository GradeScales { get; }
    IEnrollmentRepository Enrollments { get; }

    /// <summary>Persists changes. Unique-index violations and concurrency conflicts come back as failed Results.</summary>
    Task<Result> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs the operation inside a serializable transaction (commit on success, rollback on a failed Result or exception).
    /// Use it for "check 18-hour limit, then insert" so two concurrent requests cannot both pass the check.
    /// </summary>
    Task<Result> ExecuteInTransactionAsync(Func<CancellationToken, Task<Result>> operation, CancellationToken ct = default);

    Task<Result<T>> ExecuteInTransactionWithResultAsync<T>(Func<CancellationToken, Task<Result<T>>> operation, CancellationToken ct = default);
}
