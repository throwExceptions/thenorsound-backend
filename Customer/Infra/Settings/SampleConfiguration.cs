namespace Infra.Settings;

public class SampleConfiguration
{
    public string ApiKey { get; set; }
    public string BaseUrl { get; set; }
    public string GetByIdEndpoint { get; set; } = "samples/{0}";
    public string GetAllEndpoint { get; set; } = "samples";
    public string CreateEndpoint { get; set; } = "samples";
    public string UpdateEndpoint { get; set; } = "samples/{0}";
    public string DeleteEndpoint { get; set; } = "samples/{0}";
}
