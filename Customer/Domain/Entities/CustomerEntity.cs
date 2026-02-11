using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class CustomerEntity : BaseEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("orgNumber")]
    public string OrgNumber { get; set; } = string.Empty;

    [BsonElement("adress")]
    public string Adress { get; set; } = string.Empty;

    [BsonElement("city")]
    public string City { get; set; } = string.Empty;

    [BsonElement("zip")]
    public string Zip { get; set; } = string.Empty;

    [BsonElement("mail")]
    public string Mail { get; set; } = string.Empty;

    [BsonElement("phone")]
    public string Phone { get; set; } = string.Empty;

    [BsonElement("contact")]
    public string? Contact { get; set; }

    [BsonElement("customerType")]
    public int CustomerType { get; set; }

    [BsonElement("tariffs")]
    public List<TariffEntity> Tariffs { get; set; } = new();

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}