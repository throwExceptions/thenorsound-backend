using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetEventByIdRequestDtoValidator : AbstractValidator<GetEventByIdRequestDto>
{
    public GetEventByIdRequestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}
