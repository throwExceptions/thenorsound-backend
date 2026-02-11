using Domain.Enums;

public class Tariff
{
    public string Id { get; set; } = string.Empty;
    public TariffCategory Category { get; set; }
    public TariffSkill Skill { get; set; }
    public TariffTimeType TimeType { get; set; }
    public decimal TariffValue { get; set; }
}