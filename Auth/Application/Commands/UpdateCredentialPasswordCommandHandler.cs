using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UpdateCredentialPasswordCommandHandler(
    ICredentialRepository credentialRepository) : IRequestHandler<UpdateCredentialPasswordCommand>
{
    public async Task Handle(UpdateCredentialPasswordCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.GetByEmailAsync(request.Email);
        if (credential == null)
        {
            throw new NotFoundException($"Credentials for '{request.Email}' not found.");
        }

        credential.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        credential.UpdatedAt = DateTime.UtcNow;
        await credentialRepository.UpdateAsync(credential.Id, credential);
    }
}
