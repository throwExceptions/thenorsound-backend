namespace API.DTOs.Response;

public class UserResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Role { get; set; }
    public int UserType { get; set; }
    public string CustomerId { get; set; } = string.Empty;

    // Crew-specific fields
    public string? Occupation { get; set; }
    public decimal? CostPerHour { get; set; }
    public string? About { get; set; }
    public List<string>? PreviousJobs { get; set; }
    public List<SkillResponseDto>? Skills { get; set; }
    public string? Image { get; set; }
    public string? Phone { get; set; }
    public string? Ssn { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Zip { get; set; }
    public string? Co { get; set; }
    public string? Bank { get; set; }
    public string? BankAccount { get; set; }
    public bool? IsEmployee { get; set; }

    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
