namespace StudentGpa.Domain.Constants;

public static class AcademicRules
{
    public const int MinCourseCreditHours = 1;
    public const int MaxCourseCreditHours = 3;
    public const int MaxSemesterCreditHours = 18;

    public const decimal MinScore = 0m;
    public const decimal MaxScore = 100m;
    public const int ScoreDecimalPlaces = 2;   // must match decimal(5,2) column

    public const string AbsentGrade = "ABS";
    public const string WithdrawnGrade = "W";
    public const string IncompleteGrade = "I";

    public const int GpaDisplayDecimalPlaces = 2;
    public const string GpaNotAvailable = "N/A";
}
