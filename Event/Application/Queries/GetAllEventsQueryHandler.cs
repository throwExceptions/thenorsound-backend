using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetAllEventsQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetAllEventsQuery, IEnumerable<Event>>
{
    public async Task<IEnumerable<Event>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
    {
        return await eventRepository.GetAllAsync(request.CustomerId);
    }
}
