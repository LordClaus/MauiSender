// MauiSender/Services/BlacklistRepo.cs
using System.Text.Json;

namespace MauiSender.Services;

public class BlacklistRepo
{
    private readonly string _file;
    private HashSet<string> _ids = new();

    public BlacklistRepo(FileSystemService fs)
    {
        _file = fs.PathFor("blacklist.json");
    }

    public async Task<HashSet<string>> LoadAsync()
    {
        if (!File.Exists(_file))
        {
            await File.WriteAllTextAsync(_file, "[]");
            _ids = new();
            return _ids;
        }
        var json = await File.ReadAllTextAsync(_file);
        _ids = JsonSerializer.Deserialize<HashSet<string>>(json) ?? new();
        return _ids;
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_ids, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_file, json);
    }

    public bool Contains(string id) => _ids.Contains(id);
    public void Add(string id) => _ids.Add(id);
    public void Remove(string id) => _ids.Remove(id);
}
