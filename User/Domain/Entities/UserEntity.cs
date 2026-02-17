using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class UserEntity : BaseEntity
{
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [BsonElement("lastName")]
    public string LastName { get; set; } = string.Empty;

    [BsonElement("role")]
    public int Role { get; set; }

    [BsonElement("userType")]
    public int UserType { get; set; }

    [BsonElement("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    // Crew-specific fields
    [BsonElement("occupation")]
    public string? Occupation { get; set; }

    [BsonElement("costPerHour")]
    public decimal? CostPerHour { get; set; }

    [BsonElement("about")]
    public string? About { get; set; }

    [BsonElement("previousJobs")]
    public List<string>? PreviousJobs { get; set; }

    [BsonElement("skills")]
    public List<SkillEntity>? Skills { get; set; }

    [BsonElement("image")]
    public string? Image { get; set; }

    [BsonElement("phone")]
    public string? Phone { get; set; }

    [BsonElement("ssn")]
    public string? Ssn { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("zip")]
    public string? Zip { get; set; }

    [BsonElement("co")]
    public string? Co { get; set; }

    [BsonElement("bank")]
    public string? Bank { get; set; }

    [BsonElement("bankAccount")]
    public string? BankAccount { get; set; }

    [BsonElement("isEmployee")]
    public bool? IsEmployee { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;
}
