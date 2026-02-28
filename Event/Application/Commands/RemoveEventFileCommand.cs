using MediatR;

namespace Application.Commands;

public class RemoveEventFileCommand : IRequest<bool>
{
    public string EventId { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
}
