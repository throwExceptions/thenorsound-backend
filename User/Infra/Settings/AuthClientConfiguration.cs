namespace Infra.Settings;

public class AuthClientConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public string RegisterCredentialEndpoint { get; set; } = "api/Auth/credentials";
    public string UpdateCredentialEmailEndpoint { get; set; } = "api/Auth/credentials/email";
}
