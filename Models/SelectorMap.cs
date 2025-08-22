// Models/SelectorMap.cs
using System.Text.Json.Serialization;

namespace MauiSender.Models;

public class SelectorMap
{
    [JsonPropertyName("list.item")]
    public string ListItem { get; set; } = ".ladies-online .user-card";

    [JsonPropertyName("list.item.id")]
    public string ListItemIdAttr { get; set; } = "[data-user-id]";

    [JsonPropertyName("chat.input")]
    public string ChatInput { get; set; } = "#messageBox, textarea[name='message']";

    [JsonPropertyName("chat.send")]
    public string ChatSend { get; set; } = "button.send, .send-btn";

    [JsonPropertyName("chat.active.marker")]
    public string ChatActiveMarker { get; set; } = ".chat-open";

    [JsonPropertyName("profile.name")]
    public string ProfileName { get; set; } = ".username";
}
