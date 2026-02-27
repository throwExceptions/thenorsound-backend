namespace API.DTOs.Response;

public class SlotResponseDto
{
    public string Date { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string? CrewId { get; set; }
    public int SkillCategory { get; set; }
    public int SkillLevel { get; set; }
    public decimal Tariff { get; set; }
    public decimal HourAmount { get; set; }
    public decimal Sum { get; set; }
}
