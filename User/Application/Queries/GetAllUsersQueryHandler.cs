using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetAllUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetAllUsersQuery, IEnumerable<User>>
{
    public async Task<IEnumerable<User>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var userType = request.UserType.HasValue ? (int?)request.UserType.Value : null;
        return await userRepository.GetAllAsync(userType);
    }
}
