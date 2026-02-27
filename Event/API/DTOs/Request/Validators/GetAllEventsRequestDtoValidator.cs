using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetAllEventsRequestDtoValidator : AbstractValidator<GetAllEventsRequestDto>
{
    public GetAllEventsRequestDtoValidator()
    {
        When(x => x.CustomerId != null, () =>
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty().WithMessage("CustomerId cannot be an empty string when provided.");
        });
    }
}
