using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MauiSender.Models;

public class Template
{
    public int Id { get; set; }
    public string Name { get; set; } = "Новий";
    public string RawText { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int Weight { get; set; } = 1;

    [JsonIgnore]
    public string[] Parts =>
        RawText
            .Replace("\r\n", "\n")
            .Split(new[] { "\n---\n", "---" }, StringSplitOptions.None)
            .Select(p => p?.Trim() ?? "")
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();
}
