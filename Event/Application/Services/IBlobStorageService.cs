namespace Application.Services;

public interface IBlobStorageService
{
    Task<string> UploadAsync(string eventId, string fileName, Stream content, string contentType);
}
