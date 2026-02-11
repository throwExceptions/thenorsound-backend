using API.DTOs.Request;
using Domain.Enums;

namespace API.Test.Helpers;

public static class TestDataFactory
{
    public const string ValidMongoId = "507f1f77bcf86cd799439011";
    public const string ValidMongoId2 = "507f1f77bcf86cd799439022";

    public static Customer ValidCustomer(
        CustomerType customerType = CustomerType.Customer,
        List<Tariff>? tariffs = null)
    {
        return new Customer
        {
            Id = ValidMongoId,
            Name = "Test AB",
            OrgNumber = "123456-7890",
            Adress = "Testgatan 1",
            City = "Stockholm",
            Zip = "11122",
            Mail = "test@example.com",
            Phone = "+46701234567",
            Contact = customerType == CustomerType.Crew ? "Anna Andersson" : null,
            CustomerType = customerType,
            Tariffs = tariffs ?? new List<Tariff>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Tariff ValidTariff(string? id = null)
    {
        return new Tariff
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Category = TariffCategory.Ljudtekniker,
            Skill = TariffSkill.A,
            TimeType = TariffTimeType.Heldag,
            TariffValue = 5000m,
        };
    }

    public static TariffItemDto ValidTariffItemDto(string? id = null)
    {
        return new TariffItemDto
        {
            Id = id,
            Category = 1,
            Skill = 1,
            TimeType = 1,
            Tariff = 5000m,
        };
    }

    public static CreateCustomerRequestDto ValidCreateRequest(
        CustomerType customerType = CustomerType.Customer)
    {
        return new CreateCustomerRequestDto
        {
            Name = "Test AB",
            OrgNumber = "123456-7890",
            Adress = "Testgatan 1",
            City = "Stockholm",
            Zip = "11122",
            Mail = "test@example.com",
            Phone = "+46701234567",
            Contact = customerType == CustomerType.Crew ? "Anna Andersson" : null,
            CustomerType = customerType,
            Tariffs = new List<TariffItemDto>(),
        };
    }

    public static UpdateCustomerRequestDto ValidUpdateRequest(string? id = null)
    {
        return new UpdateCustomerRequestDto
        {
            Id = id ?? ValidMongoId,
            Name = "Updated AB",
        };
    }
}
