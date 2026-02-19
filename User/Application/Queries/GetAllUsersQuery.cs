using MediatR;

namespace Application.Queries;

public class GetAllUsersQuery : IRequest<IEnumerable<User>>
{
}
