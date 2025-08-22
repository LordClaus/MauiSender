using System.Text.Json.Serialization;

namespace MauiSender.Models;

public class Account
{
    [JsonPropertyName("login")]
    public string Login { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [JsonPropertyName("proxy")]
    public string? Proxy { get; set; }
}
