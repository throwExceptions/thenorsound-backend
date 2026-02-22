using MongoDB.Bson.Serialization.Attributes;

namespace Domain.Entities;

public class CredentialEntity : BaseEntity
{
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("refreshToken")]
    public string? RefreshToken { get; set; }

    [BsonElement("refreshTokenExpiry")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? RefreshTokenExpiry { get; set; }
}
