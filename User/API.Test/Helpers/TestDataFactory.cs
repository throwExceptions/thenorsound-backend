using API.DTOs.Request;
using Domain.Enums;

namespace API.Test.Helpers;

public static class TestDataFactory
{
    public const string ValidMongoId = "507f1f77bcf86cd799439011";
    public const string ValidMongoId2 = "507f1f77bcf86cd799439022";

    public static User ValidUser(
        bool isCrew = false,
        List<Skill>? skills = null,
        Role role = Role.User,
        int customerType = 1)
    {
        return new User
        {
            Id = ValidMongoId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = role,
            CustomerId = ValidMongoId2,
            CustomerType = customerType,
            Occupation = isCrew ? "Ljudtekniker" : null,
            About = isCrew ? "Erfaren tekniker" : null,
            PreviousJobs = isCrew ? new List<string> { "Festival 2024" } : null,
            Skills = skills,
            Image = isCrew ? "profile.jpg" : null,
            Phone = "+46701234567",
            Ssn = isCrew ? "199001011234" : null,
            Address = isCrew ? "Testgatan 1" : null,
            City = isCrew ? "Stockholm" : null,
            Zip = isCrew ? "11122" : null,
            Co = null,
            Bank = isCrew ? "Nordea" : null,
            BankAccount = isCrew ? "1234-5678901234" : null,
            IsEmployee = isCrew ? false : null,
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

    public static CreateUserRequestDto ValidCreateRequest()
    {
        return new CreateUserRequestDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "Testsson",
            Role = 3,
            CustomerId = ValidMongoId2,
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
