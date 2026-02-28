using MediatR;

namespace Application.Commands;

public class UploadEventFilesCommand : IRequest<List<EventFile>>
{
    public string EventId { get; set; } = string.Empty;
    public List<UploadFileItem> Files { get; set; } = new();
}

public class UploadFileItem
{
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream Content { get; set; } = Stream.Null;
    public long Length { get; set; }
}
