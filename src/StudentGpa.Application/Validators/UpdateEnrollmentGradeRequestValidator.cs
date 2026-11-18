using FluentValidation;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class UpdateEnrollmentGradeRequestValidator : AbstractValidator<UpdateEnrollmentGradeRequest>
{
    public UpdateEnrollmentGradeRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x.Score.HasValue || x.SpecialStatus.HasValue)
            .WithMessage("Either a numeric score or a special academic status is required.");

        RuleFor(x => x)
            .Must(x => !(x.Score.HasValue && x.SpecialStatus.HasValue))
            .WithMessage("Provide either a numeric score or a special academic status, not both.");
    }
}
