using Application.Exceptions;
using Domain.Repositories;
using Mapster;
using MediatR;

namespace Application.Commands;

public class CreateUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<CreateUserCommand, User>
{
    public async Task<User> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Check if user with same email already exists
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new DuplicateException($"User with email '{request.Email}' already exists.");
        }

        return await userRepository.CreateAsync(request.Adapt<User>());
    }
}
