namespace MauiSender.Models;

public class Template
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Новий шаблон";
    public string Text { get; set; } = "";
    public bool Enabled { get; set; } = true;
    public int Weight { get; set; } = 1;
    public List<string> Parts { get; set; } = new();

    public void ParseParts()
    {
        var raw = Text.Replace("\r\n", "\n");
        var parts = raw.Split(new[] { "\n---\n", "---" }, StringSplitOptions.None)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();
        Parts = parts;
    }
}
