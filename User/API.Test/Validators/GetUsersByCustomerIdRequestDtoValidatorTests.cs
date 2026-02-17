using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetUsersByCustomerIdRequestDtoValidatorTests
{
    private readonly GetUsersByCustomerIdRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Valid24CharHex()
    {
        var dto = new GetUsersByCustomerIdRequestDto { CustomerId = TestDataFactory.ValidMongoId };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdEmpty()
    {
        var dto = new GetUsersByCustomerIdRequestDto { CustomerId = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdWrongLength()
    {
        var dto = new GetUsersByCustomerIdRequestDto { CustomerId = "507f1f77bcf8" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdNotHex()
    {
        var dto = new GetUsersByCustomerIdRequestDto { CustomerId = "ZZZZZZZZZZZZZZZZZZZZZZZZ" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }
}
