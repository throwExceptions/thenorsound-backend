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
    private const int MaxFileSizeBytes = 3 * 1024 * 1024;
    private const int MaxFileCount = 10;

    private static readonly HashSet<string> AllowedExtensions =
        [".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png"];

    private static readonly HashSet<string> AllowedContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "text/plain",
        "image/jpeg",
        "image/png",
    ];

    public async Task<List<EventFile>> Handle(UploadEventFilesCommand request, CancellationToken cancellationToken)
    {
        var ev = await eventRepository.GetByIdAsync(request.EventId)
            ?? throw new NotFoundException($"Event {request.EventId} not found");

        if (request.Files.Count == 0)
        {
            return ev.Files;
        }

        if (request.Files.Count > MaxFileCount)
        {
            throw new BadRequestException($"Max {MaxFileCount} filer per uppladdning.");
        }

        foreach (var file in request.Files)
        {
            if (file.Length > MaxFileSizeBytes)
            {
                throw new BadRequestException($"Filen '{file.FileName}' överstiger maxgränsen på 3 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                throw new BadRequestException($"Filtypen '{extension}' är inte tillåten.");
            }

            if (!AllowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new BadRequestException($"Content-type '{file.ContentType}' är inte tillåten.");
            }
        }

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
