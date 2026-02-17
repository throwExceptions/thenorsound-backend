using FluentValidation;

namespace API.DTOs.Request.Validators;

public class GetAllUsersRequestDtoValidator : AbstractValidator<GetAllUsersRequestDto>
{
    public GetAllUsersRequestDtoValidator()
    {
        When(x => x.UserType.HasValue, () =>
        {
            RuleFor(x => x.UserType)
                .InclusiveBetween(1, 3)
                .WithMessage("UserType must be 1 (Admin), 2 (Customer), or 3 (Crew).");
        });
    }
}
