using MediatR;

namespace Application.Queries;

public class GetUserByEmailQuery : IRequest<User?>
{
    public string Email { get; set; } = string.Empty;
}
