using Domain.Entities;
using Domain.Repositories;
using Mapster;
using MongoDB.Driver;

namespace Infra.Repositories;

public class UserRepository(IMongoCollection<UserEntity> users) : IUserRepository
{
    public async Task<User?> GetByIdAsync(string id)
    {
        var entity = await users
            .Find(u => u.Id == id && u.IsActive)
            .FirstOrDefaultAsync();

        return entity?.Adapt<User>();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var entity = await users
            .Find(u => u.Email == email && u.IsActive)
            .FirstOrDefaultAsync();

        return entity?.Adapt<User>();
    }

    public async Task<IEnumerable<User>> GetAllAsync(int? userType = null)
    {
        var filterBuilder = Builders<UserEntity>.Filter;
        var filter = filterBuilder.Eq(u => u.IsActive, true);

        if (userType.HasValue)
        {
            filter &= filterBuilder.Eq(u => u.UserType, userType.Value);
        }

        var entities = await users
            .Find(filter)
            .ToListAsync();

        return entities.Adapt<List<User>>();
    }

    public async Task<IEnumerable<User>> GetByCustomerIdAsync(string customerId, int? userType = null)
    {
        var filterBuilder = Builders<UserEntity>.Filter;
        var filter = filterBuilder.Eq(u => u.IsActive, true)
                   & filterBuilder.Eq(u => u.CustomerId, customerId);

        if (userType.HasValue)
        {
            filter &= filterBuilder.Eq(u => u.UserType, userType.Value);
        }

        var entities = await users
            .Find(filter)
            .ToListAsync();

        return entities.Adapt<List<User>>();
    }

    public async Task<User> CreateAsync(User user)
    {
        var entity = user.Adapt<UserEntity>();
        await users.InsertOneAsync(entity);
        return entity.Adapt<User>();
    }

    public async Task<bool> UpdateAsync(string id, User user)
    {
        var existingEntity = await users
            .Find(u => u.Id == id && u.IsActive)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
        {
            return false;
        }

        // Update fields
        existingEntity.Email = user.Email;
        existingEntity.FirstName = user.FirstName;
        existingEntity.LastName = user.LastName;
        existingEntity.Role = (int)user.Role;
        existingEntity.Occupation = user.Occupation;
        existingEntity.CostPerHour = user.CostPerHour;
        existingEntity.About = user.About;
        existingEntity.PreviousJobs = user.PreviousJobs;
        existingEntity.Skills = user.Skills?.Adapt<List<SkillEntity>>();
        existingEntity.Image = user.Image;
        existingEntity.Phone = user.Phone;
        existingEntity.Ssn = user.Ssn;
        existingEntity.Address = user.Address;
        existingEntity.City = user.City;
        existingEntity.Zip = user.Zip;
        existingEntity.Co = user.Co;
        existingEntity.Bank = user.Bank;
        existingEntity.BankAccount = user.BankAccount;
        existingEntity.IsEmployee = user.IsEmployee;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var result = await users.ReplaceOneAsync(u => u.Id == id, existingEntity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<UserEntity>.Update
            .Set(u => u.IsActive, false)
            .Set(u => u.UpdatedAt, DateTime.UtcNow);

        var result = await users.UpdateOneAsync(u => u.Id == id, update);
        return result.ModifiedCount > 0;
    }
}
