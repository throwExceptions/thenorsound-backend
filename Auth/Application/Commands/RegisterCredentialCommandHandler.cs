using Application.Clients;
using Application.Exceptions;
using Domain.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class RegisterCredentialCommandHandler(
    ICredentialRepository credentialRepository,
    IUserClient userClient) : IRequestHandler<RegisterCredentialCommand>
{
    public async Task Handle(RegisterCredentialCommand request, CancellationToken cancellationToken)
    {
        var user = await userClient.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException("User not found", "User", request.Email);
        }

        var existing = await credentialRepository.GetByEmailAsync(request.Email);
        if (existing != null)
        {
            throw new DuplicateException("Credentials already exist for this email");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        await credentialRepository.CreateAsync(new Credential
        {
            UserId = user.Id,
            Email = request.Email,
            PasswordHash = passwordHash
        });
    }
}
