using StudentGpa.Domain.Common;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Models;

namespace StudentGpa.Domain.Services;

public interface IGradeResolver
{
    /// <summary>Maps a score to letter grade + points using the DB-stored scale: FromGrade &lt;= score &lt; ToGrade.</summary>
    Result<GradeResult> Resolve(decimal score, IReadOnlyCollection<GradeScale> scale);
}
