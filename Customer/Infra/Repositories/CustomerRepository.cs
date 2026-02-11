using Domain.Entities;
using Domain.Repositories;
using Mapster;
using MongoDB.Driver;

namespace Infra.Repositories;

public class CustomerRepository(IMongoCollection<CustomerEntity> customers) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(string id)
    {
        var entity = await customers
            .Find(c => c.Id == id && c.IsActive)
            .FirstOrDefaultAsync();

        return entity?.Adapt<Customer>();
    }

    public async Task<Customer?> GetByOrgNumberAsync(string orgNumber)
    {
        var entity = await customers
            .Find(c => c.OrgNumber == orgNumber && c.IsActive)
            .FirstOrDefaultAsync();

        return entity?.Adapt<Customer>();
    }

    public async Task<IEnumerable<Customer>> GetAllAsync(int? customerType = null)
    {
        var filterBuilder = Builders<CustomerEntity>.Filter;
        var filter = filterBuilder.Eq(c => c.IsActive, true);

        if (customerType.HasValue)
        {
            filter &= filterBuilder.Eq(c => c.CustomerType, customerType.Value);
        }

        var entities = await customers
            .Find(filter)
            .ToListAsync();

        return entities.Adapt<List<Customer>>();
    }

    public async Task<Customer> CreateAsync(Customer customer)
    {
        var entity = customer.Adapt<CustomerEntity>();
        await customers.InsertOneAsync(entity);
        return entity.Adapt<Customer>();
    }

    public async Task<bool> UpdateAsync(string id, Customer customer)
    {
        var existingEntity = await customers
            .Find(c => c.Id == id && c.IsActive)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
        {
            return false;
        }

        // Update fields
        existingEntity.Name = customer.Name;
        existingEntity.Adress = customer.Adress;
        existingEntity.City = customer.City;
        existingEntity.Zip = customer.Zip;
        existingEntity.Mail = customer.Mail;
        existingEntity.Phone = customer.Phone;
        existingEntity.Contact = customer.Contact;
        existingEntity.Tariffs = customer.Tariffs.Adapt<List<TariffEntity>>();
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var result = await customers.ReplaceOneAsync(c => c.Id == id, existingEntity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<CustomerEntity>.Update
            .Set(c => c.IsActive, false)
            .Set(c => c.UpdatedAt, DateTime.UtcNow);

        var result = await customers.UpdateOneAsync(c => c.Id == id, update);
        return result.ModifiedCount > 0;
    }

}