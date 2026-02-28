using Application.Exceptions;
using Application.Services;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UploadEventFilesCommandHandler(
    IEventRepository eventRepository,
    IBlobStorageService blobStorageService)
    : IRequestHandler<UploadEventFilesCommand, List<EventFile>>
{
    public async Task<List<EventFile>> Handle(UploadEventFilesCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.EventId)
            ?? throw new NotFoundException($"Event {request.EventId} not found");

        var newFiles = new List<EventFile>();
        foreach (var file in request.Files)
        {
            var url = await blobStorageService.UploadAsync(ev.Id, file.FileName, file.Content, file.ContentType);
            newFiles.Add(new EventFile { Name = file.FileName, Url = url });
        }

        await eventRepository.AddFilesAsync(ev.Id, newFiles);

        var updated = await eventRepository.GetByIdAsync(ev.Id);
        return updated!.Files;
    }
}
