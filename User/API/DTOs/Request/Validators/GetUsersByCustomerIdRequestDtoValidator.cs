using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetUsersByCustomerIdRequestDtoValidator : AbstractValidator<GetUsersByCustomerIdRequestDto>
{
    public GetUsersByCustomerIdRequestDtoValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .Length(24)
            .WithMessage("CustomerId must be a valid MongoDB ObjectId (24 characters)")
            .Matches("^[a-f0-9]{24}$")
            .WithMessage("CustomerId must be a valid hexadecimal MongoDB ObjectId");
    }
}
