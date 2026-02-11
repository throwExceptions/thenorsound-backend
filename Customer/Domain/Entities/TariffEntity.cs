using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class TariffEntity
{
    [BsonElement("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("category")]
    public int Category { get; set; }

    [BsonElement("skill")]
    public int Skill { get; set; }

    [BsonElement("timeType")]
    public int TimeType { get; set; }

    [BsonElement("tariff")]
    public decimal Tariff { get; set; }
}
