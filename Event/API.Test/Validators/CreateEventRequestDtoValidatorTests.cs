using API.DTOs.Request;
using API.DTOs.Request.Validators;
using API.Test.Helpers;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace API.Test.Validators;

public class CreateEventRequestDtoValidatorTests
{
    private readonly CreateEventRequestDtoValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_When_AllFieldsValid()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        var result = _validator.TestValidate(dto);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_Should_Fail_When_CustomerIdEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.CustomerId = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.CustomerId)
            .WithErrorMessage("CustomerId is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_ProjectEmpty()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Project = string.Empty;
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Project)
            .WithErrorMessage("Project name is required.");
    }

    [Fact]
    public void Validate_Should_Fail_When_ProjectTooLong()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Project = new string('A', 201);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.Project)
            .WithErrorMessage("Project name cannot exceed 200 characters.");
    }

    [Fact]
    public void Validate_Should_Fail_When_EndBeforeStart()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Start = new DateTime(2025, 6, 2, 0, 0, 0, DateTimeKind.Utc);
        dto.End = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor(x => x.End)
            .WithErrorMessage("End date must be after Start date.");
    }

    [Fact]
    public void Validate_Should_Fail_When_SlotSkillCategoryOutOfRange()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Slots = new List<SlotItemDto>
        {
            new SlotItemDto { Date = "2025-06-01", Start = "09:00", End = "18:00", SkillCategory = 0, SkillLevel = 1 },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Slots[0].SkillCategory");
    }

    [Fact]
    public void Validate_Should_Fail_When_SlotSkillLevelOutOfRange()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Slots = new List<SlotItemDto>
        {
            new SlotItemDto { Date = "2025-06-01", Start = "09:00", End = "18:00", SkillCategory = 1, SkillLevel = 4 },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Slots[0].SkillLevel");
    }

    [Fact]
    public void Validate_Should_Fail_When_SlotDateInvalidFormat()
    {
        var dto = TestDataFactory.ValidCreateRequest();
        dto.Slots = new List<SlotItemDto>
        {
            new SlotItemDto { Date = "01-06-2025", Start = "09:00", End = "18:00", SkillCategory = 1, SkillLevel = 1 },
        };
        var result = _validator.TestValidate(dto);
        result.ShouldHaveValidationErrorFor("Slots[0].Date");
    }
}
