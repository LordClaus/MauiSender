using MauiSender.Models;

namespace MauiSender.Services;

public class AccountsService
{
    private readonly string _path = FileUtil.PathInData("accounts.json");

    public async Task EnsureAsync()
        => await FileUtil.EnsureJsonFileAsync(_path, new List<Account>());

    public async Task<List<Account>> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new List<Account>());

    public async Task SaveAsync(List<Account> accounts)
        => await FileUtil.SaveJsonAsync(_path, accounts);

    public async Task AddAsync(Account acc)
    {
        var list = await LoadAsync();
        list.Add(acc);
        await SaveAsync(list);
    }

    public async Task RemoveByLoginAsync(string login)
    {
        var list = await LoadAsync();
        list.RemoveAll(x => string.Equals(x.Login, login, StringComparison.OrdinalIgnoreCase));
        await SaveAsync(list);
    }
}
