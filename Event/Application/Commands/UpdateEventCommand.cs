using MediatR;

namespace Application.Commands;

public class UpdateEventCommand : IRequest<bool>
{
    public string Id { get; set; } = string.Empty;
    public string? Project { get; set; }
    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }
    public string? ProjectNumber { get; set; }
    public string? BookingResponsible { get; set; }
    public decimal? TotalTechnicalCost { get; set; }
    public List<Slot>? Slots { get; set; }
}
