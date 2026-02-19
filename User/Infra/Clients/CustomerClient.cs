using Application.Clients;
using Application.Clients.DTOs.Response;
using Infra.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infra.Clients;

public class CustomerClient(HttpClient httpClient,
    ILogger<CustomerClient> logger,
    IOptions<CustomerClientConfiguration> config) : ICustomerClient
{
    private readonly CustomerClientConfiguration _config = config.Value;

    public async Task<CustomerClientResponseDto?> GetByIdAsync(string customerId)
    {
        var endpoint = string.Format(_config.GetByIdEndpoint, customerId);
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

            var body = await response.Content.ReadFromJsonAsync<BaseClientResponseDto<CustomerClientResponseDto>>();

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
