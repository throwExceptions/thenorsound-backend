using Domain.Enums;

namespace Application.Clients.DTOs.Response;

public class CustomerClientResponseDto
{
    public string Id { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; }
}
