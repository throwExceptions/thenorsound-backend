using Application.Clients.DTOs.Response;

namespace Application.Clients;

public interface ISampleClient
{
    Task<SampleRespnseDto> GetAsync<T>(string endpoint);
    Task<SampleRespnseDto> PostAsync<T>(string endpoint, Customer sample);
}