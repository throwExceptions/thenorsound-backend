using Domain.Repositories;
using Mapster;
using MediatR;

namespace Application.Commands;

public class CreateEventCommandHandler(IEventRepository eventRepository)
    : IRequestHandler<CreateEventCommand, Event>
{
    public async Task<Event> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        return await eventRepository.CreateAsync(request.Adapt<Event>());
    }
}
