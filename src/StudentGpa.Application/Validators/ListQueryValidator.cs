using FluentValidation;
using StudentGpa.Application.Common;
using StudentGpa.Application.DTOs;

namespace StudentGpa.Application.Validators;

public sealed class ListQueryValidator : AbstractValidator<ListQuery>
{
    public ListQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than zero.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Search)
            .MaximumLength(FieldLengths.Search)
            .WithMessage($"Search cannot exceed {FieldLengths.Search} characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Search));
    }
}
