using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetUserQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{request.Id}' not found.");
        }

        return user;
    }
}
