namespace API.DTOs.Request;

public class CreateEventRequestDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string ProjectNumber { get; set; } = string.Empty;
    public string? BookingResponsible { get; set; }
    public decimal TotalTechnicalCost { get; set; }
    public List<SlotItemDto> Slots { get; set; } = new();
}
