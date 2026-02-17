using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class SkillEntity
{
    [BsonElement("category")]
    public int Category { get; set; }

    [BsonElement("skill")]
    public int Skill { get; set; }
}
