using API.DTOs.Request;
using Domain.Enums;

namespace API.Test.Helpers;

public static class TestDataFactory
{
    public const string ValidMongoId = "507f1f77bcf86cd799439011";
    public const string ValidMongoId2 = "507f1f77bcf86cd799439022";

    public static User ValidUser(
        UserType userType = UserType.Customer,
        List<Skill>? skills = null)
    {
        return new User
        {
            Id = ValidMongoId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = Role.CustomerUser,
            UserType = userType,
            CustomerId = ValidMongoId2,
            Occupation = userType == UserType.Crew ? "Ljudtekniker" : null,
            CostPerHour = userType == UserType.Crew ? 500m : null,
            About = userType == UserType.Crew ? "Erfaren tekniker" : null,
            PreviousJobs = userType == UserType.Crew ? new List<string> { "Festival 2024" } : null,
            Skills = skills,
            Image = userType == UserType.Crew ? "profile.jpg" : null,
            Phone = "+46701234567",
            Ssn = userType == UserType.Crew ? "199001011234" : null,
            Address = userType == UserType.Crew ? "Testgatan 1" : null,
            City = userType == UserType.Crew ? "Stockholm" : null,
            Zip = userType == UserType.Crew ? "11122" : null,
            Co = null,
            Bank = userType == UserType.Crew ? "Nordea" : null,
            BankAccount = userType == UserType.Crew ? "1234-5678901234" : null,
            IsEmployee = userType == UserType.Crew ? false : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public static Skill ValidSkill()
    {
        return new Skill
        {
            Category = SkillCategory.Ljudtekniker,
            Level = SkillLevel.A,
        };
    }

    public static SkillItemDto ValidSkillItemDto()
    {
        return new SkillItemDto
        {
            Category = 1,
            Skill = 1,
        };
    }

    public static CreateUserRequestDto ValidCreateRequest(
        int userType = 2)
    {
        return new CreateUserRequestDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = 3,
            UserType = userType,
            CustomerId = ValidMongoId2,
            Occupation = userType == 3 ? "Ljudtekniker" : null,
            Phone = "+46701234567",
        };
    }

    public static UpdateUserRequestDto ValidUpdateRequest(string? id = null)
    {
        return new UpdateUserRequestDto
        {
            Id = id ?? ValidMongoId,
            FirstName = "Updated",
        };
    }
}
