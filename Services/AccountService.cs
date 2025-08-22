using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class AccountService
{
    private readonly string _filePath;
    private List<UserAccount> _accounts = new();

    public AccountService()
    {
        // зберігаємо у локальній папці застосунку
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "accounts.json");
        Load();
    }

    public IReadOnlyList<UserAccount> GetAll() => _accounts;

    public void Add(UserAccount acc)
    {
        _accounts.Add(acc);
        Save();
    }

    public void Remove(UserAccount acc)
    {
        _accounts.Remove(acc);
        Save();
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_accounts, new JsonSerializerOptions { WriteIndented = true });
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        File.WriteAllText(_filePath, json);
    }

    private void Load()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        if (!File.Exists(_filePath))
        {
            _accounts = new List<UserAccount>();
            Save(); // створюємо ПУСТИЙ файл одразу
            return;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            _accounts = JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new List<UserAccount>();
        }
        catch
        {
            // якщо файл битий — перезаписати порожнім
            _accounts = new List<UserAccount>();
            Save();
        }
    }
}
