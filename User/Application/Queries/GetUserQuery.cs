using MediatR;

namespace Application.Queries;

public class GetUserQuery : IRequest<User>
{
    public string Id { get; set; } = string.Empty;
}
