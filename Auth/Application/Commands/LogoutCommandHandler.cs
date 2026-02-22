using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class LogoutCommandHandler(ICredentialRepository credentialRepository) : IRequestHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var credential = await credentialRepository.GetByRefreshTokenAsync(request.RefreshToken);
        if (credential != null)
        {
            credential.RefreshToken = null;
            credential.RefreshTokenExpiry = null;
            await credentialRepository.UpdateAsync(credential.Id, credential);
        }
    }
}
