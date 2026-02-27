namespace Domain.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string id);
    Task<IEnumerable<Event>> GetAllAsync(string? customerId = null);
    Task<IEnumerable<Event>> GetByCustomerIdAsync(string customerId);
    Task<Event> CreateAsync(Event ev);
    Task<bool> UpdateAsync(string id, Event ev);
    Task<bool> DeleteAsync(string id);
}
