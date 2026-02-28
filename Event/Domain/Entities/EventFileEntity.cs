using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class EventFileEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;
}
