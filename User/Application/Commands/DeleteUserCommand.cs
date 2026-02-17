using MediatR;

namespace Application.Commands;

public class DeleteUserCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
