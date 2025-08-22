// MauiSender/Services/SelectorMapService.cs
using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class SelectorMapService
{
    private readonly string _file;
    private SelectorMap _map = new();

    public SelectorMapService(FileSystemService fs)
    {
        _file = fs.PathFor("selectors.json");
    }

    public async Task<SelectorMap> LoadAsync()
    {
        if (!File.Exists(_file))
        {
            await SaveAsync(_map);
            return _map;
        }
        var json = await File.ReadAllTextAsync(_file);
        _map = JsonSerializer.Deserialize<SelectorMap>(json) ?? new SelectorMap();
        return _map;
    }

    public async Task SaveAsync(SelectorMap map)
    {
        _map = map;
        var json = JsonSerializer.Serialize(map, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_file, json);
    }

    public SelectorMap Current => _map;
}
