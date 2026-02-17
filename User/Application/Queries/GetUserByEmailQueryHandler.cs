using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetUserByEmailQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserByEmailQuery, User?>
{
    public async Task<User?> Handle(GetUserByEmailQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new NotFoundException($"User with email '{request.Email}' not found.");
        }

        return user;
    }
}
