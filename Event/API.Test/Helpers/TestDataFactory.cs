using API.DTOs.Request;
using Domain.Enums;

namespace API.Test.Helpers;

public static class TestDataFactory
{
    public const string ValidMongoId = "507f1f77bcf86cd799439011";
    public const string ValidMongoId2 = "507f1f77bcf86cd799439022";
    public const string ValidCustomerId = "507f1f77bcf86cd799439033";

    public static Event ValidEvent(List<Slot>? slots = null)
    {
        return new Event
        {
            Id = ValidMongoId,
            CustomerId = ValidCustomerId,
            Project = "Test Event",
            Start = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc),
            ProjectNumber = "PRJ-001",
            BookingResponsible = "Anna Andersson",
            TotalTechnicalCost = 10000m,
            Slots = slots ?? new List<Slot>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Slot ValidSlot()
    {
        return new Slot
        {
            Date = "2025-06-01",
            Start = "09:00",
            End = "18:00",
            SkillCategory = SkillCategory.Ljudtekniker,
            SkillLevel = SkillLevel.A,
            Tariff = 500m,
            HourAmount = 8m,
            Sum = 4000m,
        };
    }

    public static SlotItemDto ValidSlotItemDto()
    {
        return new SlotItemDto
        {
            Date = "2025-06-01",
            Start = "09:00",
            End = "18:00",
            SkillCategory = 1,
            SkillLevel = 1,
            Tariff = 500m,
            HourAmount = 8m,
            Sum = 4000m,
        };
    }

    public static CreateEventRequestDto ValidCreateRequest()
    {
        return new CreateEventRequestDto
        {
            CustomerId = ValidCustomerId,
            Project = "Test Event",
            Start = new DateTime(2025, 6, 1, 9, 0, 0, DateTimeKind.Utc),
            End = new DateTime(2025, 6, 1, 18, 0, 0, DateTimeKind.Utc),
            ProjectNumber = "PRJ-001",
            BookingResponsible = "Anna Andersson",
            TotalTechnicalCost = 10000m,
            Slots = new List<SlotItemDto>(),
        };
    }

    public static UpdateEventRequestDto ValidUpdateRequest(string? id = null)
    {
        return new UpdateEventRequestDto
        {
            Id = id ?? ValidMongoId,
            Project = "Updated Event",
        };
    }
}
