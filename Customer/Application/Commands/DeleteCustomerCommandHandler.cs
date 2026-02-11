using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class DeleteCustomerCommandHandler(ICustomerRepository customerRepository)
    : IRequestHandler<DeleteCustomerCommand, bool>
{
    public async Task<bool> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await customerRepository.GetByIdAsync(request.Id);
        if (customer == null)
        {
            throw new NotFoundException($"Customer with ID '{request.Id}' not found.");
        }

        return await customerRepository.DeleteAsync(request.Id);
    }
}