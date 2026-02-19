using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class UpdateUserRequestDtoValidatorTests
{
    private readonly UpdateUserRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_OnlyValidId()
    {
        var dto = new UpdateUserRequestDto { Id = TestDataFactory.ValidMongoId };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdEmpty()
    {
        var dto = new UpdateUserRequestDto { Id = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_Should_Fail_When_IdNotValidObjectId()
    {
        var dto = new UpdateUserRequestDto { Id = "not-a-valid-id" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id must be a valid MongoDB ObjectId.");
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdInvalidObjectId()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            CustomerId = "not-valid",
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("CustomerId must be a valid MongoDB ObjectId.");
    }

    [Fact]
    public void Validate_Should_Pass_When_CustomerIdValidObjectId()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            CustomerId = TestDataFactory.ValidMongoId2,
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_Should_Pass_When_CustomerIdNull()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            CustomerId = null,
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Validate_Should_Fail_When_EmailInvalid()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Email = "bad-email",
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_Should_Fail_When_FirstNameTooLong()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            FirstName = new string('A', 201),
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.FirstName)
            .WithErrorMessage("FirstName cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_PhoneInvalid()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Phone = "123",
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_Should_Fail_When_RoleOutOfRange()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Role = 99,
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Validate_Should_Pass_When_SkillsNull()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Skills = null,
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_SkillValuesOutOfRange()
    {
        var dto = new UpdateUserRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Skills = new List<SkillItemDto>
            {
                new SkillItemDto { Category = 6, Skill = 4 },
            },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Skills[0].Category");
        result.ShouldHaveValidationErrorFor("Skills[0].Skill");
    }
}
