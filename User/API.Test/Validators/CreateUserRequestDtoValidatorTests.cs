using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class CreateUserRequestDtoValidatorTests
{
    private readonly CreateUserRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_AllFieldsValid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Email = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("Email is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailInvalid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Email = "not-an-email";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_Fail_When_FirstNameEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.FirstName = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_FirstNameTooLong()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.FirstName = new string('A', 201);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_LastNameEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.LastName = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.LastName)
            .WithErrorMessage("LastName is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_RoleOutOfRange()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Role = 99;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Validate_Should_Pass_When_CustomerIdEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.CustomerId = null;
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdInvalidObjectId()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.CustomerId = "not-valid";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("CustomerId must be a valid MongoDB ObjectId.");
    }

    [Fact]
    public void Validate_Should_Pass_When_OccupationProvided()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Occupation = "Ljudtekniker";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.Occupation);
    }

    [Fact]
    public void Validate_Should_Fail_When_PhoneInvalid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Phone = "123";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_Should_Fail_When_SkillValuesOutOfRange()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Skills = new List<SkillItemDto>
        {
            new SkillItemDto { Category = 0, Skill = 0 },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Skills[0].Category");
        result.ShouldHaveValidationErrorFor("Skills[0].Skill");
    }
}
