using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class Localization
{
    static Dictionary<string, string> table = new();
    private static Dictionary<string, Dictionary<string, string>> scopes = new();

    public static void LoadScope(string scopePath, string jsonContent)
    {
        var root = JObject.Parse(jsonContent);
        
        JToken targetToken = root.SelectToken(scopePath);

        if (targetToken != null && targetToken is JObject scopeData)
        {
            if (!scopes.ContainsKey(scopePath))
                scopes[scopePath] = new Dictionary<string, string>();
            else
                scopes[scopePath].Clear();

            ParseObjectByScope(scopes[scopePath], scopeData, "");
            Debug.Log($"[Localization] Scope '{scopePath}' loaded.");
        }
        else
        {
            Debug.LogWarning($"[Localization] Path '{scopePath}' not found or is not an object!");
        }
    }

    public static void UnloadScope(string scopeName)
    {
        if (scopes.ContainsKey(scopeName))
        {
            scopes[scopeName].Clear();
            scopes.Remove(scopeName);
            System.GC.Collect();
        }
    }

    private static void ParseObjectByScope(Dictionary<string, string> targetDict, JObject obj, string prefix)
    {
        foreach (var prop in obj.Properties())
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
            if (prop.Value is JObject child)
                ParseObjectByScope(targetDict, child, key);
            else
                targetDict[key] = prop.Value.ToString();
        }
    }

    public static string GetByScope(string scopeName, string key)
    {
        if (scopes.TryGetValue(scopeName, out var table))
        {
            if (table.TryGetValue(key, out var value)) return value;
        }
        return $"#{key}";
    }

    public static void LoadFromJson(string json, bool clearExisting = false)
    {
        if (clearExisting) table.Clear();
        
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
