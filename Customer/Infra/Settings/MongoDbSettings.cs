namespace Infra.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CustomerCollectionName { get; set; } = "customers";
}
