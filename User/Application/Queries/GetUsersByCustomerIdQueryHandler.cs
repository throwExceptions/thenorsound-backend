using Domain.Repositories;
using MediatR;

namespace Application.Queries;

public class GetUsersByCustomerIdQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersByCustomerIdQuery, IEnumerable<User>>
{
    public async Task<IEnumerable<User>> Handle(GetUsersByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        return await userRepository.GetByCustomerIdAsync(request.CustomerId);
    }
}
