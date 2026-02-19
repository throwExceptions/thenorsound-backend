using Application.Clients;
using Application.Exceptions;
using Domain.Repositories;
using Mapster;
using MediatR;

namespace Application.Commands;

public class CreateUserCommandHandler(IUserRepository userRepository, ICustomerClient customerClient)
    : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new DuplicateException($"User with email '{request.Email}' already exists.");
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var customer = await customerClient.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID '{request.CustomerId}' not found.", "Customer", request.CustomerId);
            }
        }

        return await userRepository.CreateAsync(request.Adapt<User>());
    }
}
