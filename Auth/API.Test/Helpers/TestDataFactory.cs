using Application.Clients.DTOs.Response;
using Domain.Models;

namespace API.Test.Helpers;

public static class TestDataFactory
{
    public const string ValidMongoId = "507f1f77bcf86cd799439011";
    public const string ValidMongoId2 = "507f1f77bcf86cd799439022";
    public const string ValidRefreshToken = "dGVzdFJlZnJlc2hUb2tlbg==";

    public static Credential ValidCredential(string? refreshToken = null, DateTime? refreshTokenExpiry = null)
    {
        return new Credential
        {
            Id = ValidMongoId,
            UserId = ValidMongoId2,
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 4),
            RefreshToken = refreshToken,
            RefreshTokenExpiry = refreshTokenExpiry,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static UserClientResponseDto ValidUser()
    {
        return new UserClientResponseDto
        {
            Id = ValidMongoId2,
            Email = "test@example.com",
            FirstName = "Testare",
            LastName = "Testsson",
            Role = 3,
            CustomerId = ValidMongoId
        };
    }
}
