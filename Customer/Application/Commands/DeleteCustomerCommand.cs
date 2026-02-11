using MediatR;

namespace Application.Commands;

public class DeleteCustomerCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
}