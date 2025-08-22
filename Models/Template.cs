using System.Text.Json.Serialization;

namespace MauiSender.Models;

public enum TemplateMode
{
    RandomWeighted,
    RoundRobin
}

public class Template
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("name")]
    public string Name { get; set; } = "New Template";

    [JsonPropertyName("text")]
    public string RawText { get; set; } = string.Empty;

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("weight")]
    public int Weight { get; set; } = 1;

    [JsonPropertyName("parts")]
    public List<string> Parts { get; set; } = new();

    public void ParseParts(char[]? trimChars = null)
    {
        trimChars ??= new[] { ' ', '\t', '\r', '\n' };

        var split = RawText
            .Replace("\r\n", "\n")
            .Split(new[] { "\n---\n", "\n---", "---\n", "---" }, StringSplitOptions.None)
            .Select(s => (s ?? string.Empty).Trim(trimChars))
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();

        Parts = split;
    }
}
