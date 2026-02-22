using Application.Clients.DTOs.Response;

namespace Application.Services;

public interface IJwtService
{
    string GenerateAccessToken(UserClientResponseDto user);
    string GenerateRefreshToken();
}
