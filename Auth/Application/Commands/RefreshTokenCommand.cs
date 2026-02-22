using MediatR;

namespace Application.Commands;

public class RefreshTokenCommand : IRequest<LoginResult>
{
    public string RefreshToken { get; set; } = string.Empty;
}
