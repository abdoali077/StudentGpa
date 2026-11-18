using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class SaveCourseRequestValidator : AbstractValidator<SaveCourseRequest>
{
    public SaveCourseRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Course code is required.")
            .MaximumLength(FieldLengths.Code)
            .WithMessage($"Course code cannot exceed {FieldLengths.Code} characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Course name is required.")
            .MaximumLength(FieldLengths.CourseName)
            .WithMessage($"Course name cannot exceed {FieldLengths.CourseName} characters.");
    }
}
