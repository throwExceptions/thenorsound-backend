using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class UpdateCustomerRequestDtoValidatorTests
{
    private readonly UpdateCustomerRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_OnlyValidId()
    {
        var dto = new UpdateCustomerRequestDto { Id = TestDataFactory.ValidMongoId };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_IdEmpty()
    {
        var dto = new UpdateCustomerRequestDto { Id = string.Empty };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_Should_Fail_When_IdNotValidObjectId()
    {
        var dto = new UpdateCustomerRequestDto { Id = "not-a-valid-id" };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id must be a valid MongoDB ObjectId.");
    }

    [Fact]
    public void Validate_Should_Fail_When_NameTooLong()
    {
        var dto = new UpdateCustomerRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Name = new string('A', 201),
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_MailInvalid()
    {
        var dto = new UpdateCustomerRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Mail = "bad-email",
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Mail);
    }

    [Fact]
    public void Validate_Should_Fail_When_PhoneInvalid()
    {
        var dto = new UpdateCustomerRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Phone = "123",
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Validate_Should_Pass_When_TariffsNull()
    {
        var dto = new UpdateCustomerRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = null,
        };
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_TariffValuesOutOfRange()
    {
        var dto = new UpdateCustomerRequestDto
        {
            Id = TestDataFactory.ValidMongoId,
            Tariffs = new List<TariffItemDto>
            {
                new TariffItemDto { Category = 6, Skill = 4, TimeType = 4, Tariff = 100001 },
            },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Tariffs[0].Category");
        result.ShouldHaveValidationErrorFor("Tariffs[0].Skill");
        result.ShouldHaveValidationErrorFor("Tariffs[0].TimeType");
        result.ShouldHaveValidationErrorFor("Tariffs[0].Tariff");
    }
}
