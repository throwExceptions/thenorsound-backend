using Application.Clients;
using Application.Clients.DTOs.Response;
using Infra.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infra.Clients;

public class UserClient(
    HttpClient httpClient,
    ILogger<UserClient> logger,
    IOptions<UserClientConfiguration> config) : IUserClient
{
    private readonly UserClientConfiguration _config = config.Value;

    public async Task<UserClientResponseDto?> GetByEmailAsync(string email)
    {
        var endpoint = string.Format(_config.GetByEmailEndpoint, email);
        logger.LogInformation("GET request to {Endpoint}", endpoint);

        try
        {
            var response = await httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("GET failed. Status: {Status}, Reason: {Reason}, Url: {Url}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    response.RequestMessage?.RequestUri?.AbsoluteUri);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<BaseClientResponseDto<UserClientResponseDto>>();

            if (body?.Result == null)
            {
                logger.LogError("Failed to deserialize response from {Endpoint}", endpoint);
                return null;
            }

            logger.LogInformation("GET request successful for {Endpoint}", endpoint);
            return body.Result;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error calling GET {Endpoint}", endpoint);
            throw;
        }
    }
}

internal class BaseClientResponseDto<T>
{
    public T? Result { get; set; }
    public bool Success { get; set; }
}
