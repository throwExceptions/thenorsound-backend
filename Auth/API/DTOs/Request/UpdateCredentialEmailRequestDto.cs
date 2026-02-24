namespace API.DTOs.Request;

public class UpdateCredentialEmailRequestDto
{
    public string OldEmail { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
}
