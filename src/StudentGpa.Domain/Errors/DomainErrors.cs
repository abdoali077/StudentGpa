using StudentGpa.Domain.Common;
using StudentGpa.Domain.Constants;

namespace StudentGpa.Domain.Errors;

public static class DomainErrors
{
    public static class General
    {
        public static readonly Error ConcurrencyConflict = Error.Conflict(
            "General.ConcurrencyConflict", "The record was modified by another user. Reload and try again.");

        public static readonly Error UniqueViolation = Error.Conflict(
            "General.UniqueViolation", "A record with the same unique value already exists.");
    }

    public static class Students
    {
        public static readonly Error NotFound = Error.NotFound("Students.NotFound", "Student was not found.");

        public static Error CodeAlreadyExists(string code) =>
            Error.Conflict("Students.CodeAlreadyExists", $"A student with code '{code}' already exists.");

        public static readonly Error HasEnrollments = Error.BusinessRule(
            "Students.HasEnrollments", "Cannot delete a student that has registered courses.");
    }

    public static class Courses
    {
        public static readonly Error NotFound = Error.NotFound("Courses.NotFound", "Course was not found.");

        public static Error CodeAlreadyExists(string code) =>
            Error.Conflict("Courses.CodeAlreadyExists", $"A course with code '{code}' already exists.");

        public static readonly Error InvalidCreditHours = Error.Validation(
            "Courses.InvalidCreditHours",
            $"Credit hours must be between {AcademicRules.MinCourseCreditHours} and {AcademicRules.MaxCourseCreditHours}.");

        public static readonly Error CreditHoursLocked = Error.BusinessRule(
            "Courses.CreditHoursLocked", "Credit hours cannot be changed once students are registered in the course.");

        public static readonly Error HasEnrollments = Error.BusinessRule(
            "Courses.HasEnrollments", "Cannot delete a course that has registrations.");
    }

    public static class Semesters
    {
        public static readonly Error NotFound = Error.NotFound("Semesters.NotFound", "Semester was not found.");

        public static Error CodeAlreadyExists(string code) =>
            Error.Conflict("Semesters.CodeAlreadyExists", $"A semester with code '{code}' already exists.");

        public static readonly Error InvalidDateRange = Error.Validation(
            "Semesters.InvalidDateRange", "Semester end date must be after its start date.");

        public static readonly Error HasEnrollments = Error.BusinessRule(
            "Semesters.HasEnrollments", "Cannot delete a semester that has registrations.");
    }

    public static class Enrollments
    {
        public static readonly Error NotFound = Error.NotFound("Enrollments.NotFound", "Registration was not found.");

        public static readonly Error Duplicate = Error.Conflict(
            "Enrollments.Duplicate", "The student is already registered in this course for this semester.");

        public static Error CreditLimitExceeded(int registered, int requested) =>
            Error.BusinessRule("Enrollments.CreditLimitExceeded",
                $"Registering {requested} credit hour(s) would exceed the {AcademicRules.MaxSemesterCreditHours}-hour semester limit " +
                $"(already registered: {registered}).");
    }

    public static class Grades
    {
        public static readonly Error ScoreOutOfRange = Error.Validation(
            "Grades.ScoreOutOfRange",
            $"Score must be between {AcademicRules.MinScore} and {AcademicRules.MaxScore} inclusive.");

        public static readonly Error ScorePrecision = Error.Validation(
            "Grades.ScorePrecision",
            $"Score cannot have more than {AcademicRules.ScoreDecimalPlaces} decimal places.");

        public static readonly Error GradeRequired = Error.Validation(
            "Grades.GradeRequired", "Either a numeric score or a special academic status is required.");

        public static readonly Error ScoreAndStatusConflict = Error.Validation(
            "Grades.ScoreAndStatusConflict", "Provide either a numeric score or a special status, not both.");

        public static readonly Error InvalidStatus = Error.Validation(
            "Grades.InvalidStatus", "The special academic status is not valid.");

        public static readonly Error ScaleNotConfigured = Error.Failure(
            "Grades.ScaleNotConfigured", "The grading scale is not configured in the database.");

        public static Error NoMatchingBand(decimal score) =>
            Error.Failure("Grades.NoMatchingBand", $"No grading-scale band matches the score {score}.");
    }
}
