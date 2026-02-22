using Application.Clients;
using Application.Exceptions;
using Application.Services;
using Domain.Repositories;
using MediatR;
using System.Security.Authentication;

namespace Application.Commands;

public class LoginCommandHandler(
    ICredentialRepository credentialRepository,
    IUserClient userClient,
    IJwtService jwtService) : IRequestHandler<LoginCommand, LoginResult>
{
    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.GetByEmailAsync(request.Email);
        if (credential == null)
        {
            throw new AuthenticationException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, credential.PasswordHash))
        {
            throw new AuthenticationException("Invalid email or password");
        }

        var user = await userClient.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new AuthenticationException("Invalid email or password");
        }

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = jwtService.GenerateRefreshToken();

        credential.RefreshToken = refreshToken;
        credential.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await credentialRepository.UpdateAsync(credential.Id, credential);

        return new LoginResult
        {
            AccessToken = accessToken,
            ExpiresMs = 15 * 60 * 1000,
            RefreshToken = refreshToken
        };
    }
}
