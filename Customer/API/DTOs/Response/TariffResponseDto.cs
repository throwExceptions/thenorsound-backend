namespace API.DTOs.Response;

public class TariffResponseDto
{
    public string Id { get; set; } = string.Empty;
    public int Category { get; set; }
    public int Skill { get; set; }
    public int TimeType { get; set; }
    public decimal Tariff { get; set; }
}
