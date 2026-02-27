using API.DTOs.Request;
using API.DTOs.Request.Validators;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class GetEventByIdRequestDtoValidatorTests
{
    private readonly GetEventByIdRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_IdProvided()
    {
        var dto = new GetEventByIdRequestDto { Id = "507f1f77bcf86cd799439011" };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdEmpty()
    {
        var dto = new GetEventByIdRequestDto { Id = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }
}
