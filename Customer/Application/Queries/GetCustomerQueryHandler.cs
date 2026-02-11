using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetCustomerQueryHandler(ICustomerRepository customerRepository)
    : IRequestHandler<GetCustomerQuery, Customer>
{
    public async Task<Customer> Handle(GetCustomerQuery request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID '{request.Id}' not found.");
        }

        return customer;
    }
}