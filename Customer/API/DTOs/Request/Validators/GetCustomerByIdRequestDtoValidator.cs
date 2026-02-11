using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetCustomerByIdRequestDtoValidator : AbstractValidator<GetCustomerByIdRequestDto>
{
    public GetCustomerByIdRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .Length(24)
            .WithMessage("Id must be a valid MongoDB ObjectId (24 characters)")
            .Matches("^[a-f0-9]{24}$")
            .WithMessage("Id must be a valid hexadecimal MongoDB ObjectId");
    }
}
