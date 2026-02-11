using API.DTOs.Request;
using API.DTOs.Request.Validators;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetAllCustomersRequestDtoValidatorTests
{
    private readonly GetAllCustomersRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_NoFilter()
    {
        var dto = new GetAllCustomersRequestDto { CustomerType = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Pass_When_ValidFilter()
    {
        var dto = new GetAllCustomersRequestDto { CustomerType = 1 };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_InvalidFilter()
    {
        var dto = new GetAllCustomersRequestDto { CustomerType = 99 };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerType)
            .WithErrorMessage("CustomerType must be 1 (Customer) or 2 (Crew).");
    }
}
