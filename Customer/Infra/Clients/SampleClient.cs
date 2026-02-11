//using Application.Clients;
//using Application.Clients.DTOs.Request;
//using Application.Clients.DTOs.Response;
//using Domain.Entities;
//using Infra.Extension;
//using Infra.Settings;
//using Mapster;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System.Net.Http.Json;
//using System.Text;

//namespace Infra.Clients;

//public class SampleClient(HttpClient httpClient,
//    ILogger<SampleClient> logger,
//    IOptions<SampleConfiguration> config) : ISampleClient
//{
//    private readonly SampleConfiguration _sampleConfiguration = config.Value;

//    public async Task<SampleRespnseDto> GetAsync<T>(string endpoint)
//    {
//        logger.LogInformation("GET request to {Endpoint}", endpoint);

//        try
//        {
//            var response = await httpClient.GetAsync(endpoint);

//            if (!response.IsSuccessStatusCode)
//            {
//                logger.LogError("GET failed. Status: {Status}, Reason: {Reason}, Url: {Url}",
//                    response.StatusCode,
//                    response.ReasonPhrase,
//                    response.RequestMessage?.RequestUri?.AbsoluteUri);
//                return default;
//            }

//            var responseDto = await response.Content.ReadFromJsonAsync<SampleRespnseDto>();

//            if (responseDto == null)
//            {
//                logger.LogError("Failed to deserialize response from {Endpoint}", endpoint);
//                return default;
//            }

//            logger.LogInformation("GET request successful for {Endpoint}", endpoint);
//            return responseDto;
//        }
//        catch (HttpRequestException ex)
//        {
//            logger.LogError(ex, "HTTP error calling GET {Endpoint}", endpoint);
//            throw;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Unexpected error calling GET {Endpoint}", endpoint);
//            throw;
//        }
//    }

//    public async Task<SampleRespnseDto> PostAsync<T>(string endpoint, Customer sample)
//    {
//        var dto = sample.Adapt<SampleRequestDto>();
//        var json = dto.SerializeToLowercaseJson();
//        logger.LogInformation("POST request to {Endpoint} with data {@Data}", endpoint, json);
//        try
//        {
//            var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
//            var response = await httpClient.PostAsync(endpoint, httpContent);

//            if (!response.IsSuccessStatusCode)
//            {
//                logger.LogError("POST failed. Status: {Status}, Reason: {Reason}, Url: {Url}",
//                    response.StatusCode,
//                    response.ReasonPhrase,
//                    response.RequestMessage?.RequestUri?.AbsoluteUri);
//                return default;
//            }

//            var jsonResponse = await response.Content.ReadAsStringAsync();
//            var responseDto = await response.Content.ReadFromJsonAsync<SampleRespnseDto>();
//            if (responseDto == null)
//            {
//                logger.LogError("Failed to deserialize response from {Endpoint}", endpoint);
//                return default;
//            }

//            logger.LogInformation("POST request successful for {Endpoint}", endpoint);
//            return responseDto;
//        }
//        catch (HttpRequestException ex)
//        {
//            logger.LogError(ex, "Error calling POST {Endpoint}", endpoint);
//            throw;
//        }
//    }
//}
