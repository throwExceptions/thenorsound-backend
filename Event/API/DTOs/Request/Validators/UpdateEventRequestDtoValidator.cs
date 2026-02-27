using FluentValidation;

namespace API.DTOs.Request.Validators;

public class UpdateEventRequestDtoValidator : AbstractValidator<UpdateEventRequestDto>
{
    public UpdateEventRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        When(x => x.Project != null, () =>
        {
            RuleFor(x => x.Project)
                .NotEmpty().WithMessage("Project name cannot be empty when provided.")
                .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");
        });

        When(x => x.ProjectNumber != null, () =>
        {
            RuleFor(x => x.ProjectNumber)
                .NotEmpty().WithMessage("Project number cannot be empty when provided.")
                .MaximumLength(100).WithMessage("Project number cannot exceed 100 characters.");
        });

        When(x => x.Start.HasValue && x.End.HasValue, () =>
        {
            RuleFor(x => x.End)
                .GreaterThan(x => x.Start).WithMessage("End date must be after Start date.");
        });

        When(x => x.TotalTechnicalCost.HasValue, () =>
        {
            RuleFor(x => x.TotalTechnicalCost)
                .GreaterThanOrEqualTo(0).WithMessage("TotalTechnicalCost cannot be negative.");
        });

        When(x => x.Slots != null, () =>
        {
            RuleForEach(x => x.Slots).ChildRules(slot =>
            {
                slot.RuleFor(s => s.Date)
                    .NotEmpty().WithMessage("Slot date is required.")
                    .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Slot date must be in YYYY-MM-DD format.");

                slot.RuleFor(s => s.SkillCategory)
                    .InclusiveBetween(1, 5).WithMessage("SkillCategory must be between 1 and 5.");

                slot.RuleFor(s => s.SkillLevel)
                    .InclusiveBetween(1, 3).WithMessage("SkillLevel must be between 1 and 3.");
            });
        });
    }
}
