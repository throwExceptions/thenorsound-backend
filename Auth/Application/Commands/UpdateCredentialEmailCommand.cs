using MediatR;

namespace Application.Commands;

public class UpdateCredentialEmailCommand : IRequest
{
    public string OldEmail { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
}
