namespace Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync(int? userType = null);
    Task<IEnumerable<User>> GetByCustomerIdAsync(string customerId, int? userType = null);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateAsync(string id, User user);
    Task<bool> DeleteAsync(string id);
}
