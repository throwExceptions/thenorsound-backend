using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetUserByIdRequestDtoValidatorTests
{
    private readonly GetUserByIdRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_Valid24CharHex()
    {
        var dto = new GetUserByIdRequestDto { Id = TestDataFactory.ValidMongoId };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdEmpty()
    {
        var dto = new GetUserByIdRequestDto { Id = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_Should_Fail_When_IdWrongLength()
    {
        var dto = new GetUserByIdRequestDto { Id = "507f1f77bcf8" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_Should_Fail_When_IdNotHex()
    {
        var dto = new GetUserByIdRequestDto { Id = "ZZZZZZZZZZZZZZZZZZZZZZZZ" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }
}
