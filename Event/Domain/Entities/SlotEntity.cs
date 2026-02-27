using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class SlotEntity
{
    [BsonElement("date")]
    public string Date { get; set; } = string.Empty;

    [BsonElement("start")]
    public string Start { get; set; } = string.Empty;

    [BsonElement("end")]
    public string End { get; set; } = string.Empty;

    [BsonElement("crewId")]
    public string? CrewId { get; set; }

    [BsonElement("skillCategory")]
    public int SkillCategory { get; set; }

    [BsonElement("skillLevel")]
    public int SkillLevel { get; set; }

    [BsonElement("tariff")]
    public decimal Tariff { get; set; }

    [BsonElement("hourAmount")]
    public decimal HourAmount { get; set; }

    [BsonElement("sum")]
    public decimal Sum { get; set; }
}
