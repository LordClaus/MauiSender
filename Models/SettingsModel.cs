using System.Text.Json.Serialization;

namespace MauiSender.Models;

public class SettingsModel
{
    [JsonPropertyName("baseIntervalSec")]
    public int BaseIntervalSec { get; set; } = 45;

    [JsonPropertyName("partIntervalSec")]
    public int PartIntervalSec { get; set; } = 45;

    [JsonPropertyName("jitterPct")]
    public int JitterPct { get; set; } = 10;

    [JsonPropertyName("templateMode")]
    public TemplateMode TemplateMode { get; set; } = TemplateMode.RandomWeighted;

    [JsonPropertyName("maxFailuresBeforePause")]
    public int MaxFailuresBeforePause { get; set; } = 3;

    [JsonPropertyName("stopOnCaptcha")]
    public bool StopOnCaptcha { get; set; } = true;

    [JsonPropertyName("language")]
    public string Language { get; set; } = "UA";

    [JsonPropertyName("selectorMapPath")]
    public string SelectorMapPath { get; set; } = "selectors.json";
}
