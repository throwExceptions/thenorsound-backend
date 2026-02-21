using FluentValidation;
using MongoDB.Bson;

namespace API.DTOs.Request.Validators;

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.")
            .Must(id => ObjectId.TryParse(id, out _))
            .WithMessage("Id must be a valid MongoDB ObjectId.");

        When(x => !string.IsNullOrWhiteSpace(x.CustomerId), () =>
        {
            RuleFor(x => x.CustomerId)
                .Must(id => ObjectId.TryParse(id, out _))
                .WithMessage("CustomerId must be a valid MongoDB ObjectId.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Email), () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email must be a valid email address.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.FirstName), () =>
        {
            RuleFor(x => x.FirstName)
                .MaximumLength(200).WithMessage("FirstName cannot exceed 200 characters.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.LastName), () =>
        {
            RuleFor(x => x.LastName)
                .MaximumLength(200).WithMessage("LastName cannot exceed 200 characters.");
        });

        When(x => x.Role.HasValue, () =>
        {
            RuleFor(x => x.Role)
                .InclusiveBetween(1, 3).WithMessage("Role must be 1 (Superuser), 2 (Admin) or 3 (User).");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Phone), () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^(\+46|0)[\s\-]?[1-9][\d\s\-]{6,13}$")
                .WithMessage("Phone must be a valid Swedish phone number.");
        });

        When(x => x.Skills != null, () =>
        {
            RuleForEach(x => x.Skills).ChildRules(skill =>
            {
                skill.RuleFor(s => s.Category)
                    .InclusiveBetween(1, 5).WithMessage("Skill category must be between 1 and 5.");
                skill.RuleFor(s => s.Skill)
                    .InclusiveBetween(1, 3).WithMessage("Skill level must be between 1 and 3.");
            });
        });
    }
}
