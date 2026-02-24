using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UpdateCredentialEmailCommandHandler(
    ICredentialRepository credentialRepository) : IRequestHandler<UpdateCredentialEmailCommand>
{
    public async Task Handle(UpdateCredentialEmailCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.GetByEmailAsync(request.OldEmail);
        if (credential == null)
        {
            throw new NotFoundException($"Credentials for '{request.OldEmail}' not found.");
        }

        var existing = await credentialRepository.GetByEmailAsync(request.NewEmail);
        if (existing != null)
        {
            throw new DuplicateException($"Credentials already exist for '{request.NewEmail}'.");
        }

        credential.Email = request.NewEmail;
        await credentialRepository.UpdateAsync(credential.Id, credential);
    }
}
