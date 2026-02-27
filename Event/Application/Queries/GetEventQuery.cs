using MediatR;

namespace Application.Queries;

public class GetEventQuery : IRequest<Event>
{
    public string Id { get; set; } = string.Empty;
}
