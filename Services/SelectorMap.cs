using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiSender.Services;

public class SelectorMap
{
    public Dictionary<string, string> Map { get; set; } = new();

    public string this[string key] => Map.TryGetValue(key, out var v) ? v : "";

    public static SelectorMap LoadFromEmbedded()
    {
        using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream("selectorMap.json");
        using var r = new StreamReader(s!);
        var json = r.ReadToEnd();
        return new SelectorMap { Map = JsonSerializer.Deserialize<Dictionary<string, string>>(json)! };
    }
}
