using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MauiSender.Models;

namespace MauiSender.Services;

public class SettingsService
{
    private readonly string _path;
    public SettingsModel Model { get; private set; }

    public SettingsService()
    {
        _path = Path.Combine(FileSystem.AppDataDirectory, "settings.json");
        Model = Load();
    }

    private SettingsModel Load()
    {
        if (!File.Exists(_path)) return new SettingsModel();
        var json = File.ReadAllText(_path);
        return JsonSerializer.Deserialize<SettingsModel>(json) ?? new SettingsModel();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(Model, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
