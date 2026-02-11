namespace Domain.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(string id);
    Task<Customer?> GetByOrgNumberAsync(string orgNumber);
    Task<IEnumerable<Customer>> GetAllAsync(int? customerType = null);
    Task<Customer> CreateAsync(Customer customer);
    Task<bool> UpdateAsync(string id, Customer customer);
    Task<bool> DeleteAsync(string id);
}