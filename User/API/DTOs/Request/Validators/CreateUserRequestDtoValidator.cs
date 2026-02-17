using FluentValidation;
using MongoDB.Bson;

namespace API.DTOs.Request.Validators;

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required.")
            .MaximumLength(200).WithMessage("FirstName cannot exceed 200 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required.")
            .MaximumLength(200).WithMessage("LastName cannot exceed 200 characters.");

        RuleFor(x => x.Role)
            .InclusiveBetween(1, 5).WithMessage("Role must be between 1 and 5.");

        RuleFor(x => x.UserType)
            .InclusiveBetween(1, 3).WithMessage("UserType must be between 1 and 3.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.")
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("CustomerId must be a valid MongoDB ObjectId.");

        // Crew-specific validations (userType 3)
        When(x => x.UserType == 3, () =>
        {
            RuleFor(x => x.Occupation)
                .NotEmpty().WithMessage("Occupation is required for crew members.");
        });

        When(x => x.Skills != null && x.Skills.Count > 0, () =>
        {
            RuleForEach(x => x.Skills).ChildRules(skill =>
            {
                skill.RuleFor(s => s.Category)
                    .InclusiveBetween(1, 5).WithMessage("Skill category must be between 1 and 5.");
                skill.RuleFor(s => s.Skill)
                    .InclusiveBetween(1, 3).WithMessage("Skill level must be between 1 and 3.");
            });
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^(\+46|0)[1-9]\d{7,9}$")
                .WithMessage("Phone must be a valid Swedish phone number.");
        });
    }
}
