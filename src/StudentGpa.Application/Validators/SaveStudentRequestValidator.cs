using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class SaveStudentRequestValidator : AbstractValidator<SaveStudentRequest>
{
    public SaveStudentRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Student code is required.")
            .MaximumLength(FieldLengths.Code)
            .WithMessage($"Student code cannot exceed {FieldLengths.Code} characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Student name is required.")
            .MaximumLength(FieldLengths.StudentName)
            .WithMessage($"Student name cannot exceed {FieldLengths.StudentName} characters.");
    }
}
