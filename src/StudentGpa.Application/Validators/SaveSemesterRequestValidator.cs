using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class SaveSemesterRequestValidator : AbstractValidator<SaveSemesterRequest>
{
    public SaveSemesterRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage("Semester code is required.")
            .MaximumLength(FieldLengths.Code)
            .WithMessage($"Semester code cannot exceed {FieldLengths.Code} characters.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Semester name is required.")
            .MaximumLength(FieldLengths.SemesterName)
            .WithMessage($"Semester name cannot exceed {FieldLengths.SemesterName} characters.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("Semester end date must be after its start date.");
    }
}
