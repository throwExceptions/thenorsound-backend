using FluentValidation;
using MongoDB.Bson;

namespace API.DTOs.Request.Validators;

public class UpdateCustomerRequestDtoValidator : AbstractValidator<UpdateCustomerRequestDto>
{
    public UpdateCustomerRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.")
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid MongoDB ObjectId.");

        When(x => !string.IsNullOrWhiteSpace(x.Name), () =>
        {
            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Mail), () =>
        {
            RuleFor(x => x.Mail)
                .EmailAddress().WithMessage("Mail must be a valid email address.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^(\+46|0)[1-9]\d{7,9}$")
                .WithMessage("Phone must be a valid Swedish phone number.");
        });

        When(x => x.Tariffs != null, () =>
        {
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
        });
    }
}