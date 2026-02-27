using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetEventsByCustomerIdQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventsByCustomerIdQuery, IEnumerable<Event>>
{
    public async Task<IEnumerable<Event>> Handle(GetEventsByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        return await eventRepository.GetByCustomerIdAsync(request.CustomerId);
    }
}
