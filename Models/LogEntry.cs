using System.Text.Json.Serialization;

namespace MauiSender.Models;

public class LogEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("templateId")]
    public string TemplateId { get; set; } = string.Empty;

    [JsonPropertyName("partIndex")]
    public int PartIndex { get; set; }

    [JsonPropertyName("ts")]
    public DateTime Ts { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("status")]
    public string Status { get; set; } = "OK";

    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
