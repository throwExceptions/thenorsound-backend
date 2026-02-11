using MediatR;

namespace Application.Queries;

public class GetCustomerQuery : IRequest<Customer>
{
    public string Id { get; set; } = string.Empty;
}