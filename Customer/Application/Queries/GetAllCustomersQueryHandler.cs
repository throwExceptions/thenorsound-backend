using Domain.Models;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetAllCustomersQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetAllCustomersQuery, IEnumerable<Customer>>
{
    public async Task<IEnumerable<Customer>> Handle(GetAllCustomersQuery request, CancellationToken cancellationToken)
    {
        var customerType = request.CustomerType.HasValue ? (int?)request.CustomerType.Value : null;
        return await customerRepository.GetAllAsync(customerType);
    }
}