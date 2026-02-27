using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class EventEntity : BaseEntity
{
    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [BsonElement("project")]
    public string Project { get; set; } = string.Empty;

    [BsonElement("start")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Start { get; set; }

    [BsonElement("end")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime End { get; set; }

    [BsonElement("projectNumber")]
    public string ProjectNumber { get; set; } = string.Empty;

    [BsonElement("bookingResponsible")]
    public string? BookingResponsible { get; set; }

    [BsonElement("totalTechnicalCost")]
    public decimal TotalTechnicalCost { get; set; }

    [BsonElement("slots")]
    public List<SlotEntity> Slots { get; set; } = new();

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
