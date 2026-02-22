using MediatR;

namespace Application.Commands;

public class LoginCommand : IRequest<LoginResult>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResult
{
    public string AccessToken { get; set; } = string.Empty;
    public long ExpiresMs { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
