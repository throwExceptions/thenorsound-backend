using Application.Clients;
using Infra.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Infra.Clients;

public class AuthClient(HttpClient httpClient,
    ILogger<AuthClient> logger,
    IOptions<AuthClientConfiguration> config) : IAuthClient
{
    private readonly AuthClientConfiguration _config = config.Value;

    public async Task<bool> RegisterCredentialAsync(string email, string password)
    {
        logger.LogInformation("POST request to {Endpoint}", _config.RegisterCredentialEndpoint);

        try
        {
            var response = await httpClient.PostAsJsonAsync(_config.RegisterCredentialEndpoint, new
            {
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("POST failed. Status: {Status}, Reason: {Reason}, Url: {Url}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    response.RequestMessage?.RequestUri?.AbsoluteUri);
                return false;
            }

            logger.LogInformation("POST request successful for {Endpoint}", _config.RegisterCredentialEndpoint);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error calling POST {Endpoint}", _config.RegisterCredentialEndpoint);
            throw;
        }
    }

    public async Task<bool> UpdateEmailAsync(string oldEmail, string newEmail)
    {
        logger.LogInformation("PUT request to {Endpoint}", _config.UpdateCredentialEmailEndpoint);

        try
        {
            var response = await httpClient.PutAsJsonAsync(_config.UpdateCredentialEmailEndpoint, new
            {
                oldEmail,
                newEmail
            });

            if (!response.IsSuccessStatusCode)
            {
                logger.LogError("PUT failed. Status: {Status}, Reason: {Reason}, Url: {Url}",
                    response.StatusCode,
                    response.ReasonPhrase,
                    response.RequestMessage?.RequestUri?.AbsoluteUri);
                return false;
            }

            logger.LogInformation("PUT request successful for {Endpoint}", _config.UpdateCredentialEmailEndpoint);
            return true;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error calling PUT {Endpoint}", _config.UpdateCredentialEmailEndpoint);
            throw;
        }
    }
}
