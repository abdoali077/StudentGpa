namespace StudentGpa.Domain.Entities;

/// <summary>One band of the grading scale. Matches when FromGrade &lt;= score &lt; ToGrade. Stored in DB, never hardcoded.</summary>
public class GradeScale
{
    public int Id { get; set; }
    public decimal FromGrade { get; set; }
    public decimal ToGrade { get; set; }
    public decimal Points { get; set; }
    public string Grade { get; set; } = string.Empty;
}
