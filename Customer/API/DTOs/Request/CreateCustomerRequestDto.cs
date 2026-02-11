using Domain.Enums;

namespace API.DTOs.Request;

public class CreateCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string OrgNumber { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public CustomerType CustomerType { get; set; }
    public List<TariffItemDto> Tariffs { get; set; } = new();
}