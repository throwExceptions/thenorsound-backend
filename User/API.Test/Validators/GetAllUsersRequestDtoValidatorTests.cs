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
        var dto = new GetAllUsersRequestDto { UserType = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Pass_When_ValidFilter()
    {
        var dto = new GetAllUsersRequestDto { UserType = 1 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Pass_When_FilterIsCrew()
    {
        var dto = new GetAllUsersRequestDto { UserType = 3 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_InvalidFilter()
    {
        var dto = new GetAllUsersRequestDto { UserType = 99 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.UserType)
            .WithErrorMessage("UserType must be 1 (Admin), 2 (Customer), or 3 (Crew).");
    }
}
