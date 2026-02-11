using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using Mapster;
using MediatR;

namespace Application.Commands;

public class CreateCustomerCommandHandler(ICustomerRepository customerRepository)
    : IRequestHandler<CreateCustomerCommand, Customer>
{
    public async Task<Customer> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Check if customer with same OrgNumber already exists
        var existingCustomer = await customerRepository.GetByOrgNumberAsync(request.OrgNumber);
        if (existingCustomer != null)
        {
            throw new DuplicateException($"Customer with organization number '{request.OrgNumber}' already exists.");
        }

        // Only EventOrganizers (CustomerType.Customer) can have tariffs
        if (request.Tariffs.Count > 0 && request.CustomerType != CustomerType.Customer)
        {
            throw new BadRequestException("Only Event Organizers (CustomerType = Customer) can have tariffs.");
        }

        // Assign IDs to all tariffs
        foreach (var tariff in request.Tariffs)
        {
            tariff.Id = Guid.NewGuid().ToString();
        }

        return await customerRepository.CreateAsync(request.Adapt<Customer>());
    }
}   
