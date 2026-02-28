using Application.Services;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Infra.Settings;
using Microsoft.Extensions.Options;

namespace Infra.Services;

public class BlobStorageService(IOptions<BlobStorageSettings> settings) : IBlobStorageService
{
    private readonly BlobStorageSettings _settings = settings.Value;

    public async Task<string> UploadAsync(string eventId, string fileName, Stream content, string contentType)
    {
        var containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobName = $"{eventId}/{Guid.NewGuid()}-{fileName}";
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(content, new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.ToString();
    }
}
