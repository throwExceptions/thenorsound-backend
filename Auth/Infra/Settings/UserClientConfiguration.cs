namespace Infra.Settings;

public class UserClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string GetByEmailEndpoint { get; set; } = "api/User/by-email/{0}";
}
