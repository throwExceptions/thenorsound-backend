using Domain.Entities;
using Domain.Repositories;
using Mapster;
using MongoDB.Driver;

namespace Infra.Repositories;

public class EventRepository(IMongoCollection<EventEntity> events) : IEventRepository
{
    public async Task<Event?> GetByIdAsync(string id)
    {
        var entity = await events
            .Find(e => e.Id == id && e.IsActive)
            .FirstOrDefaultAsync();

        return entity?.Adapt<Event>();
    }

    public async Task<IEnumerable<Event>> GetAllAsync(string? customerId = null)
    {
        var filterBuilder = Builders<EventEntity>.Filter;
        var filter = filterBuilder.Eq(e => e.IsActive, true);

        if (!string.IsNullOrEmpty(customerId))
        {
            filter &= filterBuilder.Eq(e => e.CustomerId, customerId);
        }

        var entities = await events
            .Find(filter)
            .ToListAsync();

        return entities.Adapt<List<Event>>();
    }

    public async Task<IEnumerable<Event>> GetByCustomerIdAsync(string customerId)
    {
        var entities = await events
            .Find(e => e.CustomerId == customerId && e.IsActive)
            .ToListAsync();

        return entities.Adapt<List<Event>>();
    }

    public async Task<Event> CreateAsync(Event ev)
    {
        var entity = ev.Adapt<EventEntity>();
        await events.InsertOneAsync(entity);
        return entity.Adapt<Event>();
    }

    public async Task<bool> UpdateAsync(string id, Event ev)
    {
        var existingEntity = await events
            .Find(e => e.Id == id && e.IsActive)
            .FirstOrDefaultAsync();

        if (existingEntity == null)
        {
            return false;
        }

        existingEntity.Project = ev.Project;
        existingEntity.Start = ev.Start;
        existingEntity.End = ev.End;
        existingEntity.ProjectNumber = ev.ProjectNumber;
        existingEntity.BookingResponsible = ev.BookingResponsible;
        existingEntity.TotalTechnicalCost = ev.TotalTechnicalCost;
        existingEntity.Slots = ev.Slots.Adapt<List<SlotEntity>>();
        existingEntity.UpdatedAt = DateTime.UtcNow;

        var result = await events.ReplaceOneAsync(e => e.Id == id, existingEntity);
        return result.ModifiedCount > 0;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var update = Builders<EventEntity>.Update
            .Set(e => e.IsActive, false)
            .Set(e => e.UpdatedAt, DateTime.UtcNow);

        var result = await events.UpdateOneAsync(e => e.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task AddFilesAsync(string id, List<EventFile> files)
    {
        var entities = files.Adapt<List<EventFileEntity>>();
        var update = Builders<EventEntity>.Update.PushEach(e => e.Files, entities);
        await events.UpdateOneAsync(e => e.Id == id, update);
    }
}
