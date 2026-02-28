using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UpdateEventCommandHandler(IEventRepository eventRepository)
    : IRequestHandler<UpdateEventCommand, Event>
{
    public async Task<Event> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.Id);
        if (ev == null)
        {
            throw new NotFoundException($"Event with ID '{request.Id}' not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Project))
        {
            ev.Project = request.Project;
        }

        if (request.Start.HasValue)
        {
            ev.Start = request.Start.Value;
        }

        if (request.End.HasValue)
        {
            ev.End = request.End.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.ProjectNumber))
        {
            ev.ProjectNumber = request.ProjectNumber;
        }

        if (request.BookingResponsible != null)
        {
            ev.BookingResponsible = request.BookingResponsible;
        }

        if (request.TotalTechnicalCost.HasValue)
        {
            ev.TotalTechnicalCost = request.TotalTechnicalCost.Value;
        }

        if (request.Slots != null)
        {
            ev.Slots = request.Slots;
        }

        ev.UpdatedAt = DateTime.UtcNow;

        var success = await eventRepository.UpdateAsync(request.Id, ev);
        if (!success)
        {
            throw new NotFoundException($"Event with ID '{request.Id}' could not be updated.");
        }

        return ev;
    }
}
