using System.Reflection;

namespace API.Utilities.Extensions;

public static class UrlHelperExtensions
{
    public static IDictionary<string, string> ToDictionary(this object src) =>
        src
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetValue(src, null) != null)
            .ToDictionary(p => p.Name, p => p.GetValue(src, null)?.ToString() ?? string.Empty);
}
