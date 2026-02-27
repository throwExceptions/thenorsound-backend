using MediatR;

namespace Application.Queries;

public class GetEventsByCustomerIdQuery : IRequest<IEnumerable<Event>>
{
    public string CustomerId { get; set; } = string.Empty;
}
