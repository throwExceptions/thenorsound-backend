using Domain.Enums;
using MediatR;

namespace Application.Commands;

public class CreateUserCommand : IRequest<User>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Role Role { get; set; }
    public string CustomerId { get; set; } = string.Empty;

    // Crew-specific fields
    public string? Occupation { get; set; }
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
