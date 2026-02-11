using Domain.Enums;

namespace API.DTOs.Response;

public class CustomerResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string OrgNumber { get; set; } = string.Empty;
    public string Adress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public int CustomerType { get; set; } // Return as int to match frontend
    public List<TariffResponseDto> Tariffs { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}