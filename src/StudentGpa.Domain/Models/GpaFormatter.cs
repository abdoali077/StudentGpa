using System.Globalization;
using StudentGpa.Domain.Constants;

namespace StudentGpa.Domain.Models;

public static class GpaFormatter
{
    /// <summary>Rounds only for display (2 dp, half away from zero). Calculations keep full decimal precision.</summary>
    public static string Format(decimal? gpa) =>
        gpa is null
            ? AcademicRules.GpaNotAvailable
            : Math.Round(gpa.Value, AcademicRules.GpaDisplayDecimalPlaces, MidpointRounding.AwayFromZero)
                  .ToString("0.00", CultureInfo.InvariantCulture);
}
