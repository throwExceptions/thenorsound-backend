using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetEventQueryHandler(IEventRepository eventRepository)
    : IRequestHandler<GetEventQuery, Event>
{
    public async Task<Event> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.Id);
        if (ev == null)
        {
            throw new NotFoundException($"Event with ID '{request.Id}' not found.");
        }

        return ev;
    }
}
