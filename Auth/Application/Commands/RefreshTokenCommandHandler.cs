using Application.Clients;
using Application.Services;
using Domain.Repositories;
using MediatR;
using System.Security.Authentication;

namespace Application.Commands;

public class RefreshTokenCommandHandler(
    ICredentialRepository credentialRepository,
    IUserClient userClient,
    IJwtService jwtService) : IRequestHandler<RefreshTokenCommand, LoginResult>
{
    public async Task<LoginResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (credential == null)
        {
            throw new AuthenticationException("Invalid refresh token");
        }

        if (credential.RefreshTokenExpiry < DateTime.UtcNow)
        {
            throw new AuthenticationException("Token expired");
        }

        var user = await userClient.GetByEmailAsync(credential.Email);
        if (user == null)
        {
            throw new AuthenticationException("Invalid refresh token");
        }

        var accessToken = jwtService.GenerateAccessToken(user);
        var newRefreshToken = jwtService.GenerateRefreshToken();

        credential.RefreshToken = newRefreshToken;
        credential.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await credentialRepository.UpdateAsync(credential.Id, credential);

        return new LoginResult
        {
            AccessToken = accessToken,
            ExpiresMs = 15 * 60 * 1000,
            RefreshToken = newRefreshToken
        };
    }
}
