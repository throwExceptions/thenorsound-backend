namespace API.DTOs.Request;

public class RegisterCredentialRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
