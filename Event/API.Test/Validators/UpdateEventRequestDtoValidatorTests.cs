using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class UpdateEventRequestDtoValidatorTests
{
    private readonly UpdateEventRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_OnlyIdProvided()
    {
        var dto = TestDataFactory.ValidUpdateRequest();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdEmpty()
    {
        var dto = new UpdateEventRequestDto { Id = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_ProjectEmpty_WhenProvided()
    {
        var dto = TestDataFactory.ValidUpdateRequest();
        dto.Project = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Project);
    }

    [Fact]
    public void Validate_Should_Pass_When_ProjectNull()
    {
        var dto = TestDataFactory.ValidUpdateRequest();
        dto.Project = null;
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Project);
    }
}
