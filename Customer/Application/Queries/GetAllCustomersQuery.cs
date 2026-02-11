using Domain.Enums;
using Domain.Models;
using MediatR;

namespace Application.Queries;

public class GetAllCustomersQuery : IRequest<IEnumerable<Customer>>
{
    public CustomerType? CustomerType { get; set; }
}