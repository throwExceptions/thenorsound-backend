namespace API.DTOs.Request;

public class TariffItemDto
{
    public string? Id { get; set; }
    public int Category { get; set; }
    public int Skill { get; set; }
    public int TimeType { get; set; }
    public decimal Tariff { get; set; }
}
