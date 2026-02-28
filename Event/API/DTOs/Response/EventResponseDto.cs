namespace API.DTOs.Response;

public class EventResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string? BookingResponsible { get; set; }
    public decimal TotalTechnicalCost { get; set; }
    public List<SlotResponseDto> Slots { get; set; } = new();
    public List<EventFileResponseDto> Files { get; set; } = new();
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
