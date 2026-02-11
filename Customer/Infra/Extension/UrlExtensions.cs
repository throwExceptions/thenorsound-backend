using System.Reflection;
using System.Web;

namespace Infra.Extension;
public static class UrlExtensions
{
    public static string AppendQueryString<T>(this string url, T dto)
    {
        if (dto == null || EqualityComparer<T>.Default.Equals(dto, default(T)))
        {
            return url;
        }

        var queryString = dto.SerializeToQueryString();
        if (string.IsNullOrEmpty(queryString))
        {
            return url;
        }

        var separator = url.Contains("?") ? "&" : "?";
        return $"{url}{separator}{queryString}";
    }

    private static string SerializeToQueryString<T>(this T dto)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var keyValuePairs = new List<string>();
        foreach (var property in properties)
        {
            var value = property.GetValue(dto);
            if (value != null && !IsDefaultValue(value))
            {
                keyValuePairs.Add($"{property.Name.ToLower()}={HttpUtility.UrlEncode(value.ToString())}");
            }
        }

        return string.Join("&", keyValuePairs);
    }

    private static bool IsDefaultValue(object value)
    {
        if (value == null)
        {
            return true;
        }

        var type = value.GetType();
        if (type.IsValueType)
        {
            return value.Equals(Activator.CreateInstance(type));
        }

        return false;
    }
}
