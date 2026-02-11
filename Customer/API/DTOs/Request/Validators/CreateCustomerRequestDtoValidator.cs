using FluentValidation;

namespace API.DTOs.Request.Validators;

public class CreateCustomerRequestDtoValidator : AbstractValidator<CreateCustomerRequestDto>
{
    public CreateCustomerRequestDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.OrgNumber)
            .NotEmpty().WithMessage("Organization number is required.")
            .Matches(@"^\d{6}-?\d{4}$|^\d{10}$")
            .WithMessage("Organization number must be in format XXXXXX-XXXX or XXXXXXXXXX.");

        RuleFor(x => x.Mail)
            .NotEmpty().WithMessage("Mail is required.")
            .EmailAddress().WithMessage("Mail must be a valid email address.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Matches(@"^(\+46|0)[1-9]\d{7,9}$")
            .WithMessage("Phone must be a valid Swedish phone number.");

        RuleFor(x => x.CustomerType)
            .IsInEnum().WithMessage("CustomerType must be 1 (Customer) or 2 (Crew).");

        // Contact is required for CrewCompany (customerType 2)
        When(x => x.CustomerType == Domain.Enums.CustomerType.Crew, () =>
        {
            RuleFor(x => x.Contact)
                .NotEmpty().WithMessage("Contact person is required for Crew Companies.");
        });

        // Tariffs are only allowed for CustomerType.Customer (EventOrganizer)
        When(x => x.Tariffs.Count > 0, () =>
        {
            RuleFor(x => x.CustomerType)
                .Equal(Domain.Enums.CustomerType.Customer)
                .WithMessage("Only Event Organizers (CustomerType = Customer) can have tariffs.");
        });

        RuleForEach(x => x.Tariffs).ChildRules(tariff =>
        {
            tariff.RuleFor(t => t.Category)
                .InclusiveBetween(1, 5).WithMessage("Category must be between 1 and 5.");
            tariff.RuleFor(t => t.Skill)
                .InclusiveBetween(1, 3).WithMessage("Skill must be between 1 and 3.");
            tariff.RuleFor(t => t.TimeType)
                .InclusiveBetween(1, 3).WithMessage("TimeType must be between 1 and 3.");
            tariff.RuleFor(t => t.Tariff)
                .GreaterThan(0).WithMessage("Tariff must be greater than 0.")
                .LessThanOrEqualTo(100000).WithMessage("Tariff cannot exceed 100000.");
        });
    }
}