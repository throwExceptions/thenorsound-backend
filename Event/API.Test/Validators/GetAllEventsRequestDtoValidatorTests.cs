using API.DTOs.Request;
using API.DTOs.Request.Validators;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetAllEventsRequestDtoValidatorTests
{
    private readonly GetAllEventsRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_CustomerIdNull()
    {
        var dto = new GetAllEventsRequestDto { CustomerId = null };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Pass_When_CustomerIdProvided()
    {
        var dto = new GetAllEventsRequestDto { CustomerId = "507f1f77bcf86cd799439033" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdEmptyString()
    {
        var dto = new GetAllEventsRequestDto { CustomerId = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }
}
