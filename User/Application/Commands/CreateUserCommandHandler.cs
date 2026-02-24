using Application.Clients;
using Application.Exceptions;
using Domain.Repositories;
using Mapster;
using MediatR;

namespace Application.Commands;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    ICustomerClient customerClient,
    IAuthClient authClient)
    : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new DuplicateException($"User with email '{request.Email}' already exists.");
        }

        int customerType = 0;
        if (!string.IsNullOrWhiteSpace(request.CustomerId))
        {
            var customer = await customerClient.GetByIdAsync(request.CustomerId);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID '{request.CustomerId}' not found.", "Customer", request.CustomerId);
            }
            customerType = (int)customer.CustomerType;
        }

        var user = request.Adapt<User>();
        user.CustomerType = customerType;

        var createdUser = await userRepository.CreateAsync(user);

        var credentialRegistered = await authClient.RegisterCredentialAsync(request.Email, request.Password);
        if (!credentialRegistered)
        {
            await userRepository.DeleteAsync(createdUser.Id);
            throw new BadRequestException("Failed to register credentials. User creation rolled back.");
        }

        return createdUser;
    }
}
