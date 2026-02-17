using Application.Exceptions;
using Domain.Repositories;
using MediatR;

namespace Application.Commands;

public class DeleteUserCommandHandler(IUserRepository userRepository)
    : IRequestHandler<DeleteUserCommand, bool>
{
    public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id);
        if (user == null)
        {
            throw new NotFoundException($"User with ID '{request.Id}' not found.");
        }

        return await userRepository.DeleteAsync(request.Id);
    }
}
