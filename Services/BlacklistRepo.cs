using System.Text.Json;

namespace MauiSender.Services
{
    public sealed class BlacklistRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };
        private HashSet<string> _ids = new(StringComparer.OrdinalIgnoreCase);

        public BlacklistRepo(string path)
        {
            _path = path;
            if (!File.Exists(_path)) File.WriteAllText(_path, "[]");
            _ids = Load();
        }

        private HashSet<string> Load()
        {
            try
            {
                var json = File.ReadAllText(_path);
                var list = JsonSerializer.Deserialize<List<string>>(json) ?? new();
                return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
            }
            catch { return new(); }
        }

        private void Save() =>
            File.WriteAllText(_path, JsonSerializer.Serialize(_ids.ToList(), _opts));

        public IReadOnlyCollection<string> All => _ids;

        public void ReplaceAll(IEnumerable<string> items)
        {
            _ids = new HashSet<string>(items ?? Enumerable.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            Save();
        }

        public bool Contains(string id) => _ids.Contains(id);
        public void Add(string id) { if (_ids.Add(id)) Save(); }
        public void Remove(string id) { if (_ids.Remove(id)) Save(); }
    }
}
