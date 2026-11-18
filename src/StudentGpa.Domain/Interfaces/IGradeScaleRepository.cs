using StudentGpa.Domain.Entities;

namespace StudentGpa.Domain.Interfaces;

public interface IGradeScaleRepository
{
    /// <summary>Whole scale ordered by FromGrade descending (read-only, no tracking).</summary>
    Task<IReadOnlyList<GradeScale>> GetAllAsync(CancellationToken ct = default);
}
