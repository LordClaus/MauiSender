// MauiSender/Services/SettingsService.cs
using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class SettingsService
{
    private readonly FileSystemService _fs;
    private readonly string _file;
    private SettingsModel _cached = new();

    public SettingsService(FileSystemService fs)
    {
        _fs = fs;
        _file = fs.PathFor("settings.json");
    }

    public async Task<SettingsModel> LoadAsync()
    {
        if (!File.Exists(_file))
        {
            await SaveAsync(_cached);
            return _cached;
        }

        var json = await File.ReadAllTextAsync(_file);
        _cached = JsonSerializer.Deserialize<SettingsModel>(json) ?? new SettingsModel();
        return _cached;
    }

    public async Task SaveAsync(SettingsModel settings)
    {
        _cached = settings;
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_file, json);
    }

    public SettingsModel Current => _cached;
}
