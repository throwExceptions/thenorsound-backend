using Domain.Models;

namespace Domain.Repositories;

public interface ICredentialRepository
{
    Task<Credential?> GetByEmailAsync(string email);
    Task<Credential?> GetByRefreshTokenAsync(string refreshToken);
    Task<Credential> CreateAsync(Credential credential);
    Task<bool> UpdateAsync(string id, Credential credential);
}
