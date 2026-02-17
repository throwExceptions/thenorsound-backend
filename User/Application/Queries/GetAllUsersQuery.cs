using Domain.Enums;
using MediatR;

namespace Application.Queries;

public class GetAllUsersQuery : IRequest<IEnumerable<User>>
{
    public UserType? UserType { get; set; }
}
