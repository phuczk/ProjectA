using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static class Localization
{
    static Dictionary<string, string> table = new();

    public static void LoadFromJson(string json)
    {
        table.Clear();
        var root = JObject.Parse(json);
        ParseObject(root, "");
    }

    static void ParseObject(JObject obj, string prefix)
    {
        foreach (var prop in obj.Properties())
        {
            string key = string.IsNullOrEmpty(prefix)
                ? prop.Name
                : $"{prefix}.{prop.Name}";

            if (prop.Value is JObject child)
                ParseObject(child, key);
            else
                table[key] = prop.Value.ToString();
        }
    }

    public static string Get(string key)
    {
        return table.TryGetValue(key, out var value)
            ? value
            : $"#{key}";
    }
}
