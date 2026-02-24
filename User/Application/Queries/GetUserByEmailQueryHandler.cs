using Application.Clients;
using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetUserByEmailQueryHandler(IUserRepository userRepository, ICustomerClient customerClient)
    : IRequestHandler<GetUserByEmailQuery, User?>
{
    public async Task<User?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException($"User with email '{request.Email}' not found.");
        }

        if (user.CustomerType == 0 && !string.IsNullOrEmpty(user.CustomerId))
        {
            var customer = await customerClient.GetByIdAsync(user.CustomerId);
            if (customer != null)
            {
                user.CustomerType = (int)customer.CustomerType;
                user.UpdatedAt = DateTime.UtcNow;
                await userRepository.UpdateAsync(user.Id, user);
            }
        }

        return user;
    }
}
