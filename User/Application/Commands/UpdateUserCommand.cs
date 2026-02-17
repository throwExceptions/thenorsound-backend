using MediatR;

namespace Application.Commands;

public class UpdateUserCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? Role { get; set; }

    // Crew-specific fields
    public string? Occupation { get; set; }
    public decimal? CostPerHour { get; set; }
    public string? About { get; set; }
    public List<string>? PreviousJobs { get; set; }
    public List<Skill>? Skills { get; set; }
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
}
