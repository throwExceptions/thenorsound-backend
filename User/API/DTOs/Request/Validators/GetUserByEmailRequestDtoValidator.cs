using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetUserByEmailRequestDtoValidator : AbstractValidator<GetUserByEmailRequestDto>
{
    public GetUserByEmailRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}
