namespace Application.Clients.DTOs.Response;

public class UserClientResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Role { get; set; }
    public string CustomerId { get; set; } = string.Empty;
}
