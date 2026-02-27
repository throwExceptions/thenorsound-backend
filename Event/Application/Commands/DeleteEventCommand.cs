using MediatR;

namespace Application.Commands;

public class DeleteEventCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}
