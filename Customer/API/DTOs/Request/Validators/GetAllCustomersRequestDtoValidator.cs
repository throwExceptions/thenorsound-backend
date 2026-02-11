using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetAllCustomersRequestDtoValidator : AbstractValidator<GetAllCustomersRequestDto>
{
    public GetAllCustomersRequestDtoValidator()
    {
        When(x => x.CustomerType.HasValue, () =>
        {
            RuleFor(x => x.CustomerType)
                .InclusiveBetween(1, 2)
                .WithMessage("CustomerType must be 1 (Customer) or 2 (Crew).");
        });
    }
}
