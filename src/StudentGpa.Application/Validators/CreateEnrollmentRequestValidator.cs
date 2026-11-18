using FluentValidation;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class CreateEnrollmentRequestValidator : AbstractValidator<CreateEnrollmentRequest>
{
    public CreateEnrollmentRequestValidator()
    {
        RuleFor(x => x.StudentId)
            .GreaterThan(0)
            .WithMessage("Student ID must be greater than zero.");

        RuleFor(x => x.SemesterId)
            .GreaterThan(0)
            .WithMessage("Semester ID must be greater than zero.");

        RuleFor(x => x.CourseId)
            .GreaterThan(0)
            .WithMessage("Course ID must be greater than zero.");
    }
}
