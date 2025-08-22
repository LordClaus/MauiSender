// MauiSender/Services/AccountsService.cs
using System.Collections.ObjectModel;
using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class AccountsService
{
    private readonly FileSystemService _fs;
    private readonly string _file;

    public AccountsService(FileSystemService fs)
    {
        _fs = fs;
        _file = fs.PathFor("accounts.json");
    }

    public async Task<ObservableCollection<Account>> LoadAsync()
    {
        await _fs.EnsureExistsAsync("accounts.json");
        var json = await File.ReadAllTextAsync(_file);
        var list = JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
        return new ObservableCollection<Account>(list);
    }

    public async Task SaveAsync(IEnumerable<Account> accounts)
    {
        var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_file, json);
    }
}
