using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;
using StudentGpa.Domain.Entities;
using StudentGpa.Domain.Enums;
using StudentGpa.Domain.Errors;
using StudentGpa.Domain.Models;
using StudentGpa.Domain.Services;
using Xunit;

namespace StudentGpa.Tests;

// =====================================================================================
//  Shared test data / builders
// =====================================================================================
internal static class TestData
{
    /// <summary>Same grading scale that the migration seeds into the database.</summary>
    public static List<GradeScale> Scale() => new()
    {
        new() { Id = 1,  FromGrade = 96, ToGrade = 101, Points = 4.0m, Grade = "A+" },
        new() { Id = 2,  FromGrade = 92, ToGrade = 96,  Points = 3.7m, Grade = "A"  },
        new() { Id = 3,  FromGrade = 88, ToGrade = 92,  Points = 3.4m, Grade = "A-" },
        new() { Id = 4,  FromGrade = 84, ToGrade = 88,  Points = 3.2m, Grade = "B+" },
        new() { Id = 5,  FromGrade = 80, ToGrade = 84,  Points = 3.0m, Grade = "B"  },
        new() { Id = 6,  FromGrade = 76, ToGrade = 80,  Points = 2.8m, Grade = "B-" },
        new() { Id = 7,  FromGrade = 72, ToGrade = 76,  Points = 2.6m, Grade = "C+" },
        new() { Id = 8,  FromGrade = 68, ToGrade = 72,  Points = 2.4m, Grade = "C"  },
        new() { Id = 9,  FromGrade = 64, ToGrade = 68,  Points = 2.2m, Grade = "C-" },
        new() { Id = 10, FromGrade = 60, ToGrade = 64,  Points = 2.0m, Grade = "D+" },
        new() { Id = 11, FromGrade = 55, ToGrade = 60,  Points = 1.5m, Grade = "D"  },
        new() { Id = 12, FromGrade = 50, ToGrade = 55,  Points = 1.0m, Grade = "D-" },
        new() { Id = 13, FromGrade = 0,  ToGrade = 50,  Points = 0.0m, Grade = "F"  }
    };

    public static readonly IEnrollmentGradingService Grading =
        new EnrollmentGradingService(new AcademicPolicy(), new GradeResolver());

    public static Student Student(int id = 1, string code = "STU-001", string name = "Ahmed Hassan") =>
        new() { Id = id, Code = code, Name = name };

    public static Semester Semester(int id, string code, DateOnly start) =>
        new() { Id = id, Code = code, Name = code, StartDate = start, EndDate = start.AddMonths(4) };

    public static Course Course(int id, string code, int creditHours) =>
        new() { Id = id, Code = code, Name = code, CreditHours = creditHours };

    /// <summary>Builds a graded enrollment using the real grading service (same path as production).</summary>
    public static Enrollment Enroll(
        Student student, Semester semester, Course course, decimal? score, AcademicStatus? status = null)
    {
        var enrollment = new Enrollment
        {
            Id = course.Id * 100 + semester.Id,
            StudentId = student.Id,
            SemesterId = semester.Id,
            CourseId = course.Id,
            CreditHours = course.CreditHours,
            Student = student,
            Semester = semester,
            Course = course
        };

        var result = Grading.Apply(enrollment, score, status, Scale());
        Assert.True(result.IsSuccess, "Test setup failed: " + (result.IsFailure ? result.Error.Description : ""));
        return enrollment;
    }
}

// =====================================================================================
//  1. GradeResolver  - score -> letter grade / points (FromGrade <= score < ToGrade)
// =====================================================================================
public class GradeResolverTests
{
    private readonly GradeResolver _sut = new();

    public static TheoryData<decimal, string, decimal> ScoreToGrade => new()
    {
        { 0m,      "F",  0.0m },
        { 49.99m,  "F",  0.0m },
        { 50m,     "D-", 1.0m },
        { 54.99m,  "D-", 1.0m },
        { 55m,     "D",  1.5m },
        { 59.99m,  "D",  1.5m },
        { 60m,     "D+", 2.0m },
        { 63.99m,  "D+", 2.0m },
        { 64m,     "C-", 2.2m },
        { 67.99m,  "C-", 2.2m },
        { 68m,     "C",  2.4m },
        { 70m,     "C",  2.4m },
        { 72m,     "C+", 2.6m },
        { 76m,     "B-", 2.8m },
        { 80m,     "B",  3.0m },
        { 84m,     "B+", 3.2m },
        { 85m,     "B+", 3.2m },
        { 88m,     "A-", 3.4m },
        { 90m,     "A-", 3.4m },
        { 92m,     "A",  3.7m },
        { 95.99m,  "A",  3.7m },   // upper bound is exclusive
        { 96m,     "A+", 4.0m },   // lower bound is inclusive
        { 100m,    "A+", 4.0m }
    };

    [Theory]
    [MemberData(nameof(ScoreToGrade))]
    public void Resolve_ReturnsExpectedLetterAndPoints(decimal score, string letter, decimal points)
    {
        var result = _sut.Resolve(score, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Equal(letter, result.Value.LetterGrade);
        Assert.Equal(points, result.Value.Points);
    }

    [Fact]
    public void Resolve_DoesNotDependOnScaleOrder()
    {
        var shuffled = TestData.Scale().OrderBy(b => b.FromGrade).ToList();

        var result = _sut.Resolve(96m, shuffled);

        Assert.True(result.IsSuccess);
        Assert.Equal("A+", result.Value.LetterGrade);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    [InlineData("-5")]
    [InlineData("1000")]
    public void Resolve_ScoreOutsideRange_FailsWithScoreOutOfRange(string raw)
    {
        var score = decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);

        var result = _sut.Resolve(score, TestData.Scale());

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScoreOutOfRange.Code, result.Error.Code);
    }

    [Fact]
    public void Resolve_EmptyScale_FailsWithScaleNotConfigured()
    {
        var result = _sut.Resolve(80m, new List<GradeScale>());

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScaleNotConfigured.Code, result.Error.Code);
        Assert.Equal(ErrorType.Failure, result.Error.Type);
    }

    [Fact]
    public void Resolve_ScaleWithGap_FailsWithNoMatchingBand()
    {
        var scaleWithGap = TestData.Scale().Where(b => b.Grade != "B").ToList(); // removes 80..84

        var result = _sut.Resolve(82m, scaleWithGap);

        Assert.True(result.IsFailure);
        Assert.Equal("Grades.NoMatchingBand", result.Error.Code);
    }
}

// =====================================================================================
//  2. AcademicPolicy - credit hours, semester limit, grade input
// =====================================================================================
public class AcademicPolicyTests
{
    private readonly AcademicPolicy _sut = new();

    // ---- Course credit hours: 1..3 -------------------------------------------------
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void ValidateCourseCreditHours_InsideRange_Succeeds(int hours) =>
        Assert.True(_sut.ValidateCourseCreditHours(hours).IsSuccess);

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(10)]
    public void ValidateCourseCreditHours_OutsideRange_Fails(int hours)
    {
        var result = _sut.ValidateCourseCreditHours(hours);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Courses.InvalidCreditHours.Code, result.Error.Code);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
    }

    // ---- Semester limit: max 18 hours ----------------------------------------------
    [Theory]
    [InlineData(0, 3)]
    [InlineData(15, 3)]   // exactly 18 is allowed
    [InlineData(17, 1)]
    [InlineData(18, 0)]
    public void ValidateSemesterCreditLimit_WithinLimit_Succeeds(int registered, int requested) =>
        Assert.True(_sut.ValidateSemesterCreditLimit(registered, requested).IsSuccess);

    [Theory]
    [InlineData(16, 3)]   // 19
    [InlineData(18, 1)]   // 19
    [InlineData(17, 2)]   // 19
    public void ValidateSemesterCreditLimit_ExceedsLimit_FailsWithBusinessRule(int registered, int requested)
    {
        var result = _sut.ValidateSemesterCreditLimit(registered, requested);

        Assert.True(result.IsFailure);
        Assert.Equal("Enrollments.CreditLimitExceeded", result.Error.Code);
        Assert.Equal(ErrorType.BusinessRule, result.Error.Type);
    }

    // ---- Grade input: exactly one of score / status --------------------------------
    [Fact]
    public void ValidateGradeInput_NeitherScoreNorStatus_Fails()
    {
        var result = _sut.ValidateGradeInput(null, null);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.GradeRequired.Code, result.Error.Code);
    }

    [Fact]
    public void ValidateGradeInput_BothScoreAndStatus_Fails()
    {
        var result = _sut.ValidateGradeInput(80m, AcademicStatus.Absent);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScoreAndStatusConflict.Code, result.Error.Code);
    }

    [Fact]
    public void ValidateGradeInput_UndefinedStatus_Fails()
    {
        var result = _sut.ValidateGradeInput(null, (AcademicStatus)99);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.InvalidStatus.Code, result.Error.Code);
    }

    [Theory]
    [InlineData(AcademicStatus.Absent)]
    [InlineData(AcademicStatus.Withdrawn)]
    [InlineData(AcademicStatus.Incomplete)]
    public void ValidateGradeInput_ValidStatus_Succeeds(AcademicStatus status) =>
        Assert.True(_sut.ValidateGradeInput(null, status).IsSuccess);

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("87.5")]
    [InlineData("95.99")]
    [InlineData("49.99")]
    public void ValidateGradeInput_ValidScore_Succeeds(string raw)
    {
        var score = decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);

        Assert.True(_sut.ValidateGradeInput(score, null).IsSuccess);
    }

    [Theory]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    [InlineData("101")]
    [InlineData("-1")]
    public void ValidateGradeInput_ScoreOutOfRange_Fails(string raw)
    {
        var score = decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);

        var result = _sut.ValidateGradeInput(score, null);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScoreOutOfRange.Code, result.Error.Code);
    }

    [Theory]
    [InlineData("95.999")]   // would silently become 96.00 in decimal(5,2) and change the grade
    [InlineData("80.123")]
    [InlineData("0.001")]
    public void ValidateGradeInput_MoreThanTwoDecimals_Fails(string raw)
    {
        var score = decimal.Parse(raw, System.Globalization.CultureInfo.InvariantCulture);

        var result = _sut.ValidateGradeInput(score, null);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScorePrecision.Code, result.Error.Code);
    }
}

// =====================================================================================
//  3. Enrollment entity - derived members (eligibility, quality points, display grade)
// =====================================================================================
public class EnrollmentDerivedMembersTests
{
    [Fact]
    public void NumericScore_IsEligible_AndQualityPointsEqualPointsTimesHours()
    {
        var e = new Enrollment { CreditHours = 3, Score = 90m, GradePoints = 3.4m, LetterGrade = "A-" };

        Assert.True(e.IsGpaEligible);
        Assert.Equal(10.2m, e.QualityPoints);
        Assert.Equal("A-", e.DisplayGrade);
    }

    [Fact]
    public void Absent_IsEligible_WithZeroQualityPoints()
    {
        var e = new Enrollment { CreditHours = 3, SpecialStatus = AcademicStatus.Absent, GradePoints = 0m };

        Assert.True(e.IsGpaEligible);
        Assert.Equal(0m, e.QualityPoints);
        Assert.Equal("ABS", e.DisplayGrade);
    }

    [Theory]
    [InlineData(AcademicStatus.Withdrawn, "W")]
    [InlineData(AcademicStatus.Incomplete, "I")]
    public void WithdrawnAndIncomplete_AreNotEligible_AndHaveNoQualityPoints(AcademicStatus status, string display)
    {
        var e = new Enrollment { CreditHours = 3, SpecialStatus = status, GradePoints = null };

        Assert.False(e.IsGpaEligible);
        Assert.Null(e.QualityPoints);
        Assert.Equal(display, e.DisplayGrade);
    }
}

// =====================================================================================
//  4. EnrollmentGradingService - applies score / status to an enrollment
// =====================================================================================
public class EnrollmentGradingServiceTests
{
    private readonly EnrollmentGradingService _sut = new(new AcademicPolicy(), new GradeResolver());

    private static Enrollment NewEnrollment() => new() { CreditHours = 3 };

    [Fact]
    public void Apply_NumericScore_SetsScoreLetterAndPoints()
    {
        var e = NewEnrollment();

        var result = _sut.Apply(e, 85m, null, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Equal(85m, e.Score);
        Assert.Null(e.SpecialStatus);
        Assert.Equal("B+", e.LetterGrade);
        Assert.Equal(3.2m, e.GradePoints);
    }

    [Fact]
    public void Apply_Absent_CountsAsZeroPoints()
    {
        var e = NewEnrollment();

        var result = _sut.Apply(e, null, AcademicStatus.Absent, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Null(e.Score);
        Assert.Equal(AcademicStatus.Absent, e.SpecialStatus);
        Assert.Equal(AcademicRules.AbsentGrade, e.LetterGrade);
        Assert.Equal(0m, e.GradePoints);
    }

    [Theory]
    [InlineData(AcademicStatus.Withdrawn, "W")]
    [InlineData(AcademicStatus.Incomplete, "I")]
    public void Apply_WithdrawnOrIncomplete_HasNullPoints(AcademicStatus status, string letter)
    {
        var e = NewEnrollment();

        var result = _sut.Apply(e, null, status, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Equal(letter, e.LetterGrade);
        Assert.Null(e.GradePoints);
        Assert.Null(e.Score);
    }

    [Fact]
    public void Apply_StatusDoesNotNeedGradeScale()
    {
        var e = NewEnrollment();

        var result = _sut.Apply(e, null, AcademicStatus.Withdrawn, new List<GradeScale>());

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Apply_ScoreAfterStatus_ClearsSpecialStatus()
    {
        // Editing a registration: ABS -> 90 must recalculate with fresh values.
        var e = TestData.Enroll(TestData.Student(), TestData.Semester(1, "S1", new DateOnly(2025, 9, 1)),
                                TestData.Course(1, "CS101", 3), null, AcademicStatus.Absent);

        var result = _sut.Apply(e, 90m, null, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Null(e.SpecialStatus);
        Assert.Equal(90m, e.Score);
        Assert.Equal("A-", e.LetterGrade);
        Assert.Equal(3.4m, e.GradePoints);
        Assert.Equal(10.2m, e.QualityPoints);
    }

    [Fact]
    public void Apply_StatusAfterScore_ClearsScore()
    {
        var e = TestData.Enroll(TestData.Student(), TestData.Semester(1, "S1", new DateOnly(2025, 9, 1)),
                                TestData.Course(1, "CS101", 3), 90m);

        var result = _sut.Apply(e, null, AcademicStatus.Withdrawn, TestData.Scale());

        Assert.True(result.IsSuccess);
        Assert.Null(e.Score);
        Assert.Null(e.GradePoints);
        Assert.False(e.IsGpaEligible);
    }

    [Fact]
    public void Apply_InvalidInput_FailsAndDoesNotMutateEnrollment()
    {
        var e = TestData.Enroll(TestData.Student(), TestData.Semester(1, "S1", new DateOnly(2025, 9, 1)),
                                TestData.Course(1, "CS101", 3), 90m);

        var result = _sut.Apply(e, 150m, null, TestData.Scale());

        Assert.True(result.IsFailure);
        Assert.Equal(90m, e.Score);
        Assert.Equal("A-", e.LetterGrade);
        Assert.Equal(3.4m, e.GradePoints);
    }

    [Fact]
    public void Apply_ScoreAndStatusTogether_Fails()
    {
        var result = _sut.Apply(NewEnrollment(), 80m, AcademicStatus.Absent, TestData.Scale());

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScoreAndStatusConflict.Code, result.Error.Code);
    }

    [Fact]
    public void Apply_NoScoreNoStatus_Fails()
    {
        var result = _sut.Apply(NewEnrollment(), null, null, TestData.Scale());

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.GradeRequired.Code, result.Error.Code);
    }

    [Fact]
    public void Apply_ScoreWithEmptyScale_PropagatesScaleNotConfigured()
    {
        var result = _sut.Apply(NewEnrollment(), 80m, null, new List<GradeScale>());

        Assert.True(result.IsFailure);
        Assert.Equal(DomainErrors.Grades.ScaleNotConfigured.Code, result.Error.Code);
    }
}

// =====================================================================================
//  5. GpaCalculator - semester GPA, cumulative GPA, ABS / W / I handling
// =====================================================================================
public class GpaCalculatorTests
{
    private readonly GpaCalculator _sut = new();

    private static readonly Student Ahmed = TestData.Student();
    private static readonly Semester Fall = TestData.Semester(1, "2025-FALL", new DateOnly(2025, 9, 1));
    private static readonly Semester Spring = TestData.Semester(2, "2026-SPRING", new DateOnly(2026, 2, 1));

    private static Course C(int id, string code, int hours) => TestData.Course(id, code, hours);

    // ---- The verification example from the assessment ----------------------------------
    [Fact]
    public void Calculate_TaskExample_ReturnsExpectedSemesterAndCumulativeGpa()
    {
        var enrollments = new[]
        {
            // Semester 1: 90 (3h), 85 (2h), ABS (3h), W (2h), I (3h)
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3),   90m),
            TestData.Enroll(Ahmed, Fall, C(2, "MATH101", 2), 85m),
            TestData.Enroll(Ahmed, Fall, C(3, "ENG101", 3),  null, AcademicStatus.Absent),
            TestData.Enroll(Ahmed, Fall, C(4, "PHY101", 2),  null, AcademicStatus.Withdrawn),
            TestData.Enroll(Ahmed, Fall, C(5, "IT101", 3),   null, AcademicStatus.Incomplete),
            // Semester 2: 96 (3h), 70 (2h)
            TestData.Enroll(Ahmed, Spring, C(6, "CS102", 3),   96m),
            TestData.Enroll(Ahmed, Spring, C(7, "MATH102", 2), 70m)
        };

        var report = _sut.Calculate(Ahmed, enrollments);

        Assert.Equal(2, report.Semesters.Count);

        var s1 = report.Semesters[0];
        Assert.Equal("2025-FALL", s1.SemesterCode);
        Assert.Equal(13, s1.RegisteredCreditHours);    // 3+2+3+2+3 (W and I still registered)
        Assert.Equal(8, s1.EligibleCreditHours);       // 3+2+3
        Assert.Equal(16.6m, s1.TotalQualityPoints);    // 10.2 + 6.4 + 0
        Assert.Equal(2.075m, s1.Gpa);
        Assert.Equal("2.08", s1.GpaDisplay);

        var s2 = report.Semesters[1];
        Assert.Equal(5, s2.RegisteredCreditHours);
        Assert.Equal(5, s2.EligibleCreditHours);
        Assert.Equal(16.8m, s2.TotalQualityPoints);    // 12 + 4.8
        Assert.Equal(3.36m, s2.Gpa);
        Assert.Equal("3.36", s2.GpaDisplay);

        Assert.Equal(18, report.TotalRegisteredCreditHours);
        Assert.Equal(13, report.TotalEligibleCreditHours);
        Assert.Equal(33.4m, report.TotalQualityPoints);
        Assert.Equal("2.57", report.CumulativeGpaDisplay);
        Assert.Equal(2.5692m, Math.Round(report.CumulativeGpa!.Value, 4));
    }

    [Fact]
    public void Calculate_CumulativeGpa_IsCreditWeighted_NotAverageOfSemesterGpas()
    {
        var enrollments = new[]
        {
        TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 90m),
        TestData.Enroll(Ahmed, Fall, C(2, "MATH101", 2), 85m),
        TestData.Enroll(Ahmed, Fall, C(3, "ENG101", 3), null, AcademicStatus.Absent),
        TestData.Enroll(Ahmed, Spring, C(6, "CS102", 3), 96m),
        TestData.Enroll(Ahmed, Spring, C(7, "MATH102", 2), 70m)
    };

        var report = _sut.Calculate(Ahmed, enrollments);

        var averageOfSemesterGpas =
            (report.Semesters[0].Gpa!.Value + report.Semesters[1].Gpa!.Value) / 2m;

        Assert.Equal("2.57", report.CumulativeGpaDisplay);
        Assert.Equal("2.72", GpaFormatter.Format(averageOfSemesterGpas));
        Assert.NotEqual(
            report.CumulativeGpaDisplay,
            GpaFormatter.Format(averageOfSemesterGpas));
    }
    // ---- ABS / W / I -----------------------------------------------------------------
    [Fact]
    public void Calculate_Absent_IsIncludedWithZeroPoints_AndLowersGpa()
    {
        var withAbsent = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 100m),
            TestData.Enroll(Ahmed, Fall, C(2, "ENG101", 3), null, AcademicStatus.Absent)
        });

        Assert.Equal(6, withAbsent.Semesters[0].EligibleCreditHours);
        Assert.Equal(12m, withAbsent.Semesters[0].TotalQualityPoints);
        Assert.Equal(2.0m, withAbsent.Semesters[0].Gpa);
    }

    [Theory]
    [InlineData(AcademicStatus.Withdrawn)]
    [InlineData(AcademicStatus.Incomplete)]
    public void Calculate_WithdrawnOrIncomplete_AreExcludedFromGpa_ButCountAsRegisteredHours(AcademicStatus status)
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 100m),
            TestData.Enroll(Ahmed, Fall, C(2, "ENG101", 3), null, status)
        });

        var sem = report.Semesters[0];
        Assert.Equal(6, sem.RegisteredCreditHours);
        Assert.Equal(3, sem.EligibleCreditHours);
        Assert.Equal(4.0m, sem.Gpa);   // not diluted by the excluded course
    }

    [Fact]
    public void Calculate_ExcludedCourses_AreMarkedNotEligibleInCourseLines()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 90m),
            TestData.Enroll(Ahmed, Fall, C(2, "ENG101", 3), null, AcademicStatus.Absent),
            TestData.Enroll(Ahmed, Fall, C(3, "PHY101", 2), null, AcademicStatus.Withdrawn),
            TestData.Enroll(Ahmed, Fall, C(4, "IT101", 3), null, AcademicStatus.Incomplete)
        });

        var lines = report.Semesters[0].Courses.ToDictionary(l => l.CourseCode);

        Assert.True(lines["CS101"].IsGpaEligible);
        Assert.Equal("A-", lines["CS101"].DisplayGrade);

        Assert.True(lines["ENG101"].IsGpaEligible);
        Assert.Equal("ABS", lines["ENG101"].DisplayGrade);
        Assert.Equal(0m, lines["ENG101"].QualityPoints);

        Assert.False(lines["PHY101"].IsGpaEligible);
        Assert.Equal("W", lines["PHY101"].DisplayGrade);
        Assert.Null(lines["PHY101"].QualityPoints);

        Assert.False(lines["IT101"].IsGpaEligible);
        Assert.Equal("I", lines["IT101"].DisplayGrade);
        Assert.Null(lines["IT101"].QualityPoints);
    }

    // ---- N/A vs 0.00 -------------------------------------------------------------------
    [Fact]
    public void Calculate_NoEnrollments_GpaIsNotAvailable()
    {
        var report = _sut.Calculate(Ahmed, Array.Empty<Enrollment>());

        Assert.Empty(report.Semesters);
        Assert.Null(report.CumulativeGpa);
        Assert.Equal("N/A", report.CumulativeGpaDisplay);
        Assert.Equal(0, report.TotalRegisteredCreditHours);
    }

    [Fact]
    public void Calculate_OnlyWithdrawnAndIncomplete_GpaIsNotAvailable_NotZero()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), null, AcademicStatus.Withdrawn),
            TestData.Enroll(Ahmed, Fall, C(2, "ENG101", 3), null, AcademicStatus.Incomplete)
        });

        Assert.Null(report.Semesters[0].Gpa);
        Assert.Equal("N/A", report.Semesters[0].GpaDisplay);
        Assert.Null(report.CumulativeGpa);
        Assert.Equal("N/A", report.CumulativeGpaDisplay);
        Assert.Equal(6, report.TotalRegisteredCreditHours);
        Assert.Equal(0, report.TotalEligibleCreditHours);
    }

    [Fact]
    public void Calculate_OnlyAbsent_GpaIsZero_NotNotAvailable()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), null, AcademicStatus.Absent)
        });

        Assert.Equal(0m, report.Semesters[0].Gpa);
        Assert.Equal("0.00", report.Semesters[0].GpaDisplay);
        Assert.Equal("0.00", report.CumulativeGpaDisplay);
    }

    [Fact]
    public void Calculate_SemesterWithOnlyExcludedCourses_DoesNotAffectCumulativeGpa()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 100m),                                  // 4.0
            TestData.Enroll(Ahmed, Spring, C(2, "CS102", 3), null, AcademicStatus.Incomplete)      // N/A
        });

        Assert.Equal("4.00", report.Semesters[0].GpaDisplay);
        Assert.Equal("N/A", report.Semesters[1].GpaDisplay);
        Assert.Equal("4.00", report.CumulativeGpaDisplay);
    }

    // ---- Boundary student (second sample student) -------------------------------------
    [Fact]
    public void Calculate_BoundaryScores_ProduceExpectedGpas()
    {
        var sara = TestData.Student(2, "STU-002", "Sara Ali");

        var report = _sut.Calculate(sara, new[]
        {
            TestData.Enroll(sara, Fall, C(1, "CS101", 3),   95.99m),   // A  3.7 -> 11.1
            TestData.Enroll(sara, Fall, C(2, "MATH101", 2), 55m),      // D  1.5 ->  3.0
            TestData.Enroll(sara, Fall, C(3, "ENG101", 3),  100m),     // A+ 4.0 -> 12.0
            TestData.Enroll(sara, Spring, C(4, "CS102", 3),   49.99m), // F  0.0 ->  0.0
            TestData.Enroll(sara, Spring, C(5, "MATH102", 2), null, AcademicStatus.Incomplete)
        });

        Assert.Equal(26.1m, report.Semesters[0].TotalQualityPoints);
        Assert.Equal("3.26", report.Semesters[0].GpaDisplay);      // 3.2625 -> 3.26

        Assert.Equal(0m, report.Semesters[1].Gpa);                 // an F is 0.00, not N/A
        Assert.Equal("0.00", report.Semesters[1].GpaDisplay);

        Assert.Equal(11, report.TotalEligibleCreditHours);
        Assert.Equal("2.37", report.CumulativeGpaDisplay);         // 26.1 / 11
    }

    // ---- Precision, ordering, guards ---------------------------------------------------
    [Fact]
    public void Calculate_KeepsFullPrecision_AndRoundsOnlyForDisplay()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3),   90m),
            TestData.Enroll(Ahmed, Fall, C(2, "MATH101", 2), 85m),
            TestData.Enroll(Ahmed, Fall, C(3, "ENG101", 3),  null, AcademicStatus.Absent)
        });

        Assert.Equal(2.075m, report.Semesters[0].Gpa);        // raw value is not rounded
        Assert.Equal("2.08", report.Semesters[0].GpaDisplay);  // display rounds half away from zero
    }

    [Fact]
    public void Calculate_OrdersSemestersChronologically_AndCoursesByCode()
    {
        var report = _sut.Calculate(Ahmed, new[]
        {
            TestData.Enroll(Ahmed, Spring, C(3, "CS102", 3), 80m),
            TestData.Enroll(Ahmed, Fall, C(2, "MATH101", 2), 80m),
            TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 80m)
        });

        Assert.Equal(new[] { "2025-FALL", "2026-SPRING" }, report.Semesters.Select(s => s.SemesterCode));
        Assert.Equal(new[] { "CS101", "MATH101" }, report.Semesters[0].Courses.Select(c => c.CourseCode));
    }

    [Fact]
    public void Calculate_UsesEnrollmentCreditHoursSnapshot_NotCurrentCourseValue()
    {
        var course = C(1, "CS101", 3);
        var enrollment = TestData.Enroll(Ahmed, Fall, course, 100m);
        course.CreditHours = 1;   // course changed after registration; snapshot must win

        var report = _sut.Calculate(Ahmed, new[] { enrollment });

        Assert.Equal(3, report.Semesters[0].RegisteredCreditHours);
        Assert.Equal(12m, report.Semesters[0].TotalQualityPoints);
    }

    [Fact]
    public void Calculate_EnrollmentWithoutLoadedSemester_Throws()
    {
        var enrollment = TestData.Enroll(Ahmed, Fall, C(1, "CS101", 3), 90m);
        enrollment.Semester = null;

        Assert.Throws<InvalidOperationException>(() => _sut.Calculate(Ahmed, new[] { enrollment }));
    }

    [Fact]
    public void Calculate_CopiesStudentInfoIntoReport()
    {
        var report = _sut.Calculate(Ahmed, Array.Empty<Enrollment>());

        Assert.Equal(Ahmed.Id, report.StudentId);
        Assert.Equal("STU-001", report.StudentCode);
        Assert.Equal("Ahmed Hassan", report.StudentName);
    }
}

// =====================================================================================
//  6. GpaFormatter - display rounding
// =====================================================================================
public class GpaFormatterTests
{
    public static TheoryData<decimal, string> Values => new()
    {
        { 2.075m,   "2.08" },   // midpoint rounds away from zero
        { 2.074m,   "2.07" },
        { 3.2625m,  "3.26" },
        { 2.56923m, "2.57" },
        { 0m,       "0.00" },
        { 4m,       "4.00" },
        { 3.5m,     "3.50" },
        { 2.085m,   "2.09" }   // banker's rounding would give 2.08
    };

    [Theory]
    [MemberData(nameof(Values))]
    public void Format_RoundsToTwoDecimals(decimal value, string expected) =>
        Assert.Equal(expected, GpaFormatter.Format(value));

    [Fact]
    public void Format_Null_ReturnsNotAvailable() =>
        Assert.Equal("N/A", GpaFormatter.Format(null));

    [Fact]
    public void Format_IsCultureInvariant()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-EG");
            Assert.Equal("3.50", GpaFormatter.Format(3.5m));   // dot, never a comma
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }
}

// =====================================================================================
//  7. Result pattern - invariants the services rely on
// =====================================================================================
public class ResultPatternTests
{
    private static readonly Error E1 = Error.Validation("A.One", "one");
    private static readonly Error E2 = Error.BusinessRule("A.Two", "two");

    [Fact]
    public void Success_HasNoErrors()
    {
        var r = Result.Success();

        Assert.True(r.IsSuccess);
        Assert.False(r.IsFailure);
        Assert.Empty(r.Errors);
    }

    [Fact]
    public void Failure_ExposesFirstErrorAndAllErrors()
    {
        var r = Result.Failure(new[] { E1, E2 });

        Assert.True(r.IsFailure);
        Assert.Equal(E1, r.Error);
        Assert.Equal(2, r.Errors.Count);
    }

    [Fact]
    public void Failure_WithoutErrors_Throws() =>
        Assert.Throws<InvalidOperationException>(() => Result.Failure(Array.Empty<Error>()));

    [Fact]
    public void FailedResultOfT_AccessingValue_Throws()
    {
        var r = Result.Failure<int>(E1);

        Assert.Throws<InvalidOperationException>(() => r.Value);
    }

    [Fact]
    public void Combine_CollectsAllErrors()
    {
        var combined = Result.Combine(Result.Success(), Result.Failure(E1), Result.Failure(E2));

        Assert.True(combined.IsFailure);
        Assert.Equal(new[] { E1, E2 }, combined.Errors);
    }

    [Fact]
    public void Combine_AllSuccess_Succeeds() =>
        Assert.True(Result.Combine(Result.Success(), Result.Success()).IsSuccess);

    [Fact]
    public void ImplicitConversions_WorkForValueAndError()
    {
        Result<int> ok = 5;
        Result<int> bad = E1;

        Assert.True(ok.IsSuccess);
        Assert.Equal(5, ok.Value);
        Assert.True(bad.IsFailure);
        Assert.Equal(E1, bad.Error);
    }

    [Fact]
    public void ErrorTypes_MapToExpectedCategories()
    {
        Assert.Equal(ErrorType.Validation, DomainErrors.Grades.ScoreOutOfRange.Type);
        Assert.Equal(ErrorType.NotFound, DomainErrors.Students.NotFound.Type);
        Assert.Equal(ErrorType.Conflict, DomainErrors.Enrollments.Duplicate.Type);
        Assert.Equal(ErrorType.BusinessRule, DomainErrors.Courses.CreditHoursLocked.Type);
        Assert.Equal(ErrorType.Failure, DomainErrors.Grades.ScaleNotConfigured.Type);
    }
}