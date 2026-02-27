using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class DeleteEventCommandHandler(IEventRepository eventRepository)
    : IRequestHandler<DeleteEventCommand, bool>
{
    public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.Id);
        if (ev == null)
        {
            throw new NotFoundException($"Event with ID '{request.Id}' not found.");
        }

        return await eventRepository.DeleteAsync(request.Id);
    }
}
