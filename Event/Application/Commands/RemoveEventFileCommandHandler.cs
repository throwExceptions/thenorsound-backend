using Application.Exceptions;
using Application.Services;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class RemoveEventFileCommandHandler(
    IEventRepository eventRepository,
    IBlobStorageService blobStorageService)
    : IRequestHandler<RemoveEventFileCommand, bool>
{
    public async Task<bool> Handle(RemoveEventFileCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.EventId)
            ?? throw new NotFoundException($"Event {request.EventId} not found");

        var fileExists = ev.Files.Any(f => f.Url == request.FileUrl);
        if (!fileExists)
        {
            throw new NotFoundException($"File not found on event {request.EventId}");
        }

        await blobStorageService.DeleteAsync(request.FileUrl);
        return await eventRepository.RemoveFileAsync(request.EventId, request.FileUrl);
    }
}
