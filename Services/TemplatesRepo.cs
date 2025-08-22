// MauiSender/Services/TemplatesRepo.cs
using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class TemplatesRepo
{
    private readonly string _file;
    private List<Template> _templates = new();

    public TemplatesRepo(FileSystemService fs)
    {
        _file = fs.PathFor("templates.json");
    }

    public async Task<List<Template>> LoadAsync()
    {
        if (!File.Exists(_file))
        {
            var demo = new List<Template>
            {
                new Template{ Name="Привітання 1", Text="Привіт! --- Як справи?", Weight=2 },
                new Template{ Name="Привітання 2", Text="Вітаю! Мене звати Анна. --- Чи любиш подорожі?", Weight=1 }
            };
            foreach (var t in demo) t.ParseParts();
            await SaveAsync(demo);
            _templates = demo;
            return _templates;
        }

        var json = await File.ReadAllTextAsync(_file);
        _templates = JsonSerializer.Deserialize<List<Template>>(json) ?? new();
        foreach (var t in _templates) t.ParseParts();
        return _templates;
    }

    public async Task SaveAsync(IEnumerable<Template> templates)
    {
        var list = templates.ToList();
        foreach (var t in list) t.ParseParts();
        var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_file, json);
        _templates = list;
    }

    public Template? PickRandomWeighted()
    {
        var enabled = _templates.Where(t => t.Enabled && t.Parts.Count > 0).ToList();
        if (enabled.Count == 0) return null;
        var sum = enabled.Sum(t => Math.Max(1, t.Weight));
        var rnd = Random.Shared.Next(sum);
        var acc = 0;
        foreach (var t in enabled)
        {
            acc += Math.Max(1, t.Weight);
            if (rnd < acc) return t;
        }
        return enabled[0];
    }

    private int _rr = -1;
    public Template? PickRoundRobin()
    {
        var enabled = _templates.Where(t => t.Enabled && t.Parts.Count > 0).ToList();
        if (enabled.Count == 0) return null;
        _rr = (_rr + 1) % enabled.Count;
        return enabled[_rr];
    }

    public List<Template> All => _templates;
}
