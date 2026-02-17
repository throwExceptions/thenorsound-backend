using API.DTOs.Request;
using API.DTOs.Request.Validators;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetUserByEmailRequestDtoValidatorTests
{
    private readonly GetUserByEmailRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_ValidEmail()
    {
        var dto = new GetUserByEmailRequestDto { Email = "test@example.com" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailEmpty()
    {
        var dto = new GetUserByEmailRequestDto { Email = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailInvalid()
    {
        var dto = new GetUserByEmailRequestDto { Email = "not-an-email" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}
