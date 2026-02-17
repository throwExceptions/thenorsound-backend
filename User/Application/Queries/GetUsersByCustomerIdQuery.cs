using MediatR;

namespace Application.Queries;

public class GetUsersByCustomerIdQuery : IRequest<IEnumerable<User>>
{
    public string CustomerId { get; set; } = string.Empty;
}
