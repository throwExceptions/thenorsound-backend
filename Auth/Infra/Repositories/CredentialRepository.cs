using Domain.Entities;
using Domain.Models;
using Domain.Repositories;
using Mapster;
using MongoDB.Driver;

namespace Infra.Repositories;

public class CredentialRepository(IMongoCollection<CredentialEntity> credentials) : ICredentialRepository
{
    public async Task<Credential?> GetByEmailAsync(string email)
    {
        var entity = await credentials
            .Find(c => c.Email == email)
            .FirstOrDefaultAsync();

        return entity?.Adapt<Credential>();
    }

    public async Task<Credential?> GetByRefreshTokenAsync(string refreshToken)
    {
        var entity = await credentials
            .Find(c => c.RefreshToken == refreshToken)
            .FirstOrDefaultAsync();

        return entity?.Adapt<Credential>();
    }

    public async Task<Credential> CreateAsync(Credential credential)
    {
        var entity = credential.Adapt<CredentialEntity>();
        await credentials.InsertOneAsync(entity);
        return entity.Adapt<Credential>();
    }

    public async Task<bool> UpdateAsync(string id, Credential credential)
    {
        var existingEntity = await credentials
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.UserId = credential.UserId;
        existingEntity.Email = credential.Email;
        existingEntity.PasswordHash = credential.PasswordHash;
        existingEntity.RefreshToken = credential.RefreshToken;
        existingEntity.RefreshTokenExpiry = credential.RefreshTokenExpiry;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var result = await credentials.ReplaceOneAsync(c => c.Id == id, existingEntity);
        return result.ModifiedCount > 0;
    }
}
