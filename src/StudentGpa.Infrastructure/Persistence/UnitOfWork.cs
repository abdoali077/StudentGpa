using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StudentGpa.Domain.Common;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Interfaces;

namespace StudentGpa.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private const int SqlUniqueIndexViolation = 2601;
    private const int SqlUniqueConstraintViolation = 2627;

    private readonly AppDbContext _db;

    public UnitOfWork(
        AppDbContext db,
        IStudentRepository students,
        ICourseRepository courses,
        ISemesterRepository semesters,
        IGradeScaleRepository gradeScales,
        IEnrollmentRepository enrollments)
    {
        _db = db;
        Students = students;
        Courses = courses;
        Semesters = semesters;
        GradeScales = gradeScales;
        Enrollments = enrollments;
    }

    public IStudentRepository Students { get; }
    public ICourseRepository Courses { get; }
    public ISemesterRepository Semesters { get; }
    public IGradeScaleRepository GradeScales { get; }
    public IEnrollmentRepository Enrollments { get; }

    public async Task<Result> SaveChangesAsync(
        CancellationToken ct = default)
    {
        try
        {
            await _db.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Failure(
                DomainErrors.General.ConcurrencyConflict);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException
            {
                Number: SqlUniqueIndexViolation or SqlUniqueConstraintViolation
            })
        {
            // Last line of defence against duplicate records.
            return Result.Failure(
                DomainErrors.General.UniqueViolation);
        }
    }

    public Task<Result> ExecuteInTransactionAsync(
        Func<CancellationToken, Task<Result>> operation,
        CancellationToken ct = default) =>
        RunAsync(operation, ct);

    public Task<Result<T>> ExecuteInTransactionWithResultAsync<T>(
        Func<CancellationToken, Task<Result<T>>> operation,
        CancellationToken ct = default) =>
        RunAsync(operation, ct);

    private async Task<TResult> RunAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken ct)
        where TResult : Result
    {
        // Join the existing transaction if one is already active.
        if (_db.Database.CurrentTransaction is not null)
            return await operation(ct);

        var strategy = _db.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async token =>
        {
            // Clear tracked entities before retrying the operation.
            _db.ChangeTracker.Clear();

            await using var transaction =
                await _db.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    token);

            var result = await operation(token);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(token);
                return result;
            }

            await transaction.CommitAsync(token);
            return result;
        }, ct);
    }
}
