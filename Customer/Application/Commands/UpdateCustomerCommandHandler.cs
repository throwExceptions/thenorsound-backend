using Application.Exceptions;
using Domain.Enums;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class UpdateCustomerCommandHandler(ICustomerRepository customerRepository)
    : IRequestHandler<UpdateCustomerCommand, bool>
{
    public async Task<bool> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID '{request.Id}' not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
            customer.Name = request.Name;

        if (!string.IsNullOrWhiteSpace(request.Adress))
            customer.Adress = request.Adress;

        if (!string.IsNullOrWhiteSpace(request.City))
            customer.City = request.City;

        if (!string.IsNullOrWhiteSpace(request.Zip))
            customer.Zip = request.Zip;

        if (!string.IsNullOrWhiteSpace(request.Mail))
            customer.Mail = request.Mail;

        if (!string.IsNullOrWhiteSpace(request.Phone))
            customer.Phone = request.Phone;

        if (!string.IsNullOrWhiteSpace(request.Contact))
            customer.Contact = request.Contact;

        if (request.Tariffs != null)
        {
            // Only EventOrganizers (CustomerType.Customer) can have tariffs
            if (request.Tariffs.Count > 0 && customer.CustomerType != CustomerType.Customer)
            {
                throw new BadRequestException("Only Event Organizers (CustomerType = Customer) can have tariffs.");
            }

            // Assign IDs to new tariffs (those without an ID)
            foreach (var tariff in request.Tariffs)
            {
                if (string.IsNullOrEmpty(tariff.Id))
                {
                    tariff.Id = Guid.NewGuid().ToString();
                }
            }

            customer.Tariffs = request.Tariffs;
        }

        customer.UpdatedAt = DateTime.UtcNow;

        return await customerRepository.UpdateAsync(request.Id, customer);
    }
}