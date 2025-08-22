using System.Text.Json;

namespace MauiSender.Services
{
    public sealed class TemplatesRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
        private List<string> _cache = new();

        public TemplatesRepo(string path)
        {
            _path = path;
            EnsureFile();
            _cache = Load();
        }

        private void EnsureFile()
        {
            if (!File.Exists(_path))
            {
                var defaults = new List<string>
                {
                    "Привіт! Мене звати Анна. --- Рада знайомству!",
                    "Твій профіль мене зацікавив. ---5--- Я зараз онлайн."
                };
                File.WriteAllText(_path, JsonSerializer.Serialize(defaults, _opts));
            }
        }

        private List<string> Load()
        {
            try
            {
                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new();
            }
            catch { return new(); }
        }

        private void Save() =>
            File.WriteAllText(_path, JsonSerializer.Serialize(_cache, _opts));

        public IReadOnlyList<string> All => _cache;

        public void ReplaceAll(IEnumerable<string> items)
        {
            _cache = items?.ToList() ?? new();
            Save();
        }

        public void Add(string t) { _cache.Add(t); Save(); }
        public void RemoveAt(int idx) { _cache.RemoveAt(idx); Save(); }
        public void UpdateAt(int idx, string t) { _cache[idx] = t; Save(); }
    }
}
