using API.DTOs.Request;
using API.DTOs.Request.Validators;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetAllUsersRequestDtoValidatorTests
{
    private readonly GetAllUsersRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_NoFilter()
    {
        var dto = new GetAllUsersRequestDto();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
