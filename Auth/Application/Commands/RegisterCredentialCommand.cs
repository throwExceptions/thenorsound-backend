using MediatR;

namespace Application.Commands;

public class RegisterCredentialCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
