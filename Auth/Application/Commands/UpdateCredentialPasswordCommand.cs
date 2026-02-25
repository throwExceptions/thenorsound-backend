using MediatR;

namespace Application.Commands;

public class UpdateCredentialPasswordCommand : IRequest
{
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
