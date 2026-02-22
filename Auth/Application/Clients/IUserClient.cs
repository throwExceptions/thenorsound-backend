using Application.Clients.DTOs.Response;

namespace Application.Clients;

public interface IUserClient
{
    Task<UserClientResponseDto?> GetByEmailAsync(string email);
}
