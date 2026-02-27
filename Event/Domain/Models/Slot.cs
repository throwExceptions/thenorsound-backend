using Domain.Enums;

public class Slot
{
    public string Date { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string? CrewId { get; set; }
    public SkillCategory SkillCategory { get; set; }
    public SkillLevel SkillLevel { get; set; }
    public decimal Tariff { get; set; }
    public decimal HourAmount { get; set; }
    public decimal Sum { get; set; }
}
