using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class CreateCustomerRequestDtoValidatorTests
{
    private readonly CreateCustomerRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_AllFieldsValid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_NameEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Name = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_NameTooLong()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Name = new string('A', 201);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Name cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_OrgNumberEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.OrgNumber = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.OrgNumber);
    }

    [Fact]
    public void Validate_Should_Fail_When_OrgNumberInvalidFormat()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.OrgNumber = "ABC-1234";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.OrgNumber)
            .WithErrorMessage("Organization number must be in format XXXXXX-XXXX or XXXXXXXXXX.");
    }

    [Fact]
    public void Validate_Should_Pass_When_OrgNumberWithDash()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.OrgNumber = "123456-7890";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.OrgNumber);
    }

    [Fact]
    public void Validate_Should_Pass_When_OrgNumberWithoutDash()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.OrgNumber = "1234567890";
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveValidationErrorFor(x => x.OrgNumber);
    }

    [Fact]
    public void Validate_Should_Fail_When_MailInvalid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Mail = "not-an-email";
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Mail);
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
    public void Validate_Should_Fail_When_CustomerTypeInvalid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.CustomerType = (CustomerType)99;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerType);
    }

    [Fact]
    public void Validate_Should_Fail_When_CrewWithoutContact()
    {
        var dto = TestDataFactory.ValidCreateRequest(CustomerType.Crew);
        dto.Contact = null;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Contact)
            .WithErrorMessage("Contact person is required for Crew Companies.");
    }

    [Fact]
    public void Validate_Should_Fail_When_CrewHasTariffs()
    {
        var dto = TestDataFactory.ValidCreateRequest(CustomerType.Crew);
        dto.Contact = "Anna Andersson";
        dto.Tariffs = new List<TariffItemDto> { TestDataFactory.ValidTariffItemDto() };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerType)
            .WithErrorMessage("Only Event Organizers (CustomerType = Customer) can have tariffs.");
    }

    [Fact]
    public void Validate_Should_Fail_When_TariffValuesOutOfRange()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Tariffs = new List<TariffItemDto>
        {
            new TariffItemDto { Category = 0, Skill = 0, TimeType = 0, Tariff = -1 },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Tariffs[0].Category");
        result.ShouldHaveValidationErrorFor("Tariffs[0].Skill");
        result.ShouldHaveValidationErrorFor("Tariffs[0].TimeType");
        result.ShouldHaveValidationErrorFor("Tariffs[0].Tariff");
    }
}
