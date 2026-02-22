namespace API.DTOs.Response;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public long Expires { get; set; }
}
