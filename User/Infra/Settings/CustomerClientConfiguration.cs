namespace Infra.Settings;

public class CustomerClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string GetByIdEndpoint { get; set; } = "api/Customer/{0}";
}
