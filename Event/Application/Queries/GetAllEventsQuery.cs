using MediatR;

namespace Application.Queries;

public class GetAllEventsQuery : IRequest<IEnumerable<Event>>
{
    public string? CustomerId { get; set; }
}
