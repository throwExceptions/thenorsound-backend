using Application.Clients.DTOs.Response;

namespace Application.Clients;

public interface ICustomerClient
{
    Task<CustomerClientResponseDto?> GetByIdAsync(string customerId);
}
