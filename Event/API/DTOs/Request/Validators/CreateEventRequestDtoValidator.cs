using FluentValidation;

namespace API.DTOs.Request.Validators;

public class CreateEventRequestDtoValidator : AbstractValidator<CreateEventRequestDto>
{
    public CreateEventRequestDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.");

        RuleFor(x => x.Project)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name cannot exceed 200 characters.");

        RuleFor(x => x.ProjectNumber)
            .NotEmpty().WithMessage("Project number is required.")
            .MaximumLength(100).WithMessage("Project number cannot exceed 100 characters.");

        RuleFor(x => x.Start)
            .NotEqual(default(DateTime)).WithMessage("Start date is required.");

        RuleFor(x => x.End)
            .NotEqual(default(DateTime)).WithMessage("End date is required.")
            .GreaterThan(x => x.Start).WithMessage("End date must be after Start date.");

        RuleFor(x => x.TotalTechnicalCost)
            .GreaterThanOrEqualTo(0).WithMessage("TotalTechnicalCost cannot be negative.");

        RuleForEach(x => x.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.Date)
                .NotEmpty().WithMessage("Slot date is required.")
                .Matches(@"^\d{4}-\d{2}-\d{2}$").WithMessage("Slot date must be in YYYY-MM-DD format.");

            slot.RuleFor(s => s.Start)
                .NotEmpty().WithMessage("Slot start time is required.");

            slot.RuleFor(s => s.End)
                .NotEmpty().WithMessage("Slot end time is required.");

            slot.RuleFor(s => s.SkillCategory)
                .InclusiveBetween(1, 5).WithMessage("SkillCategory must be between 1 and 5.");

            slot.RuleFor(s => s.SkillLevel)
                .InclusiveBetween(1, 3).WithMessage("SkillLevel must be between 1 and 3.");

            slot.RuleFor(s => s.Tariff)
                .GreaterThanOrEqualTo(0).WithMessage("Tariff cannot be negative.");

            slot.RuleFor(s => s.HourAmount)
                .GreaterThanOrEqualTo(0).WithMessage("HourAmount cannot be negative.");

            slot.RuleFor(s => s.Sum)
                .GreaterThanOrEqualTo(0).WithMessage("Sum cannot be negative.");
        });
    }
}
