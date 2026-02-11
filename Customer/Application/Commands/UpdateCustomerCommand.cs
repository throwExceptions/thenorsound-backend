using MediatR;

namespace Application.Commands;

public class UpdateCustomerCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Adress { get; set; }
    public string? City { get; set; }
    public string? Zip { get; set; }
    public string? Mail { get; set; }
    public string? Phone { get; set; }
    public string? Contact { get; set; }
    public List<Tariff>? Tariffs { get; set; }
}