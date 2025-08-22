using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services
{
    public sealed class SettingsService
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
        private SettingsModel _cache = new();

        public SettingsService(string path)
        {
            _path = path;
            Load();
        }

        public SettingsModel Current => _cache;

        public void Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var raw = File.ReadAllText(_path);
                    _cache = JsonSerializer.Deserialize<SettingsModel>(raw) ?? new SettingsModel();
                }
                else
                {
                    Save();
                }
            }
            catch { _cache = new SettingsModel(); }
        }

        public void Save()
        {
            File.WriteAllText(_path, JsonSerializer.Serialize(_cache, _opts));
        }
    }
}
