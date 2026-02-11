using System.Text.Json;

namespace Infra.Extension;

public static class JsonExtension
{
    public static string SerializeToLowercaseJson<T>(this T obj)
    {
        if(obj == null)
        {
            return string.Empty;
        }
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        return JsonSerializer.Serialize(obj, options);
    }
}
