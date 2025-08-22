namespace MauiSender.Services;

public class BlacklistRepo
{
    private readonly string _path = FileUtil.PathInData("blacklist.json");

    public async Task EnsureAsync()
    {
        await FileUtil.EnsureJsonFileAsync(_path, new List<string>());
    }

    public async Task<HashSet<string>> LoadAsync()
    {
        var list = await FileUtil.LoadJsonAsync(_path, new List<string>());
        return new HashSet<string>(list, StringComparer.OrdinalIgnoreCase);
    }

    public async Task SaveAsync(HashSet<string> set)
    {
        await FileUtil.SaveJsonAsync(_path, set.ToList());
    }

    public async Task ReplaceAllAsync(IEnumerable<string> ids)
    {
        var set = new HashSet<string>(ids, StringComparer.OrdinalIgnoreCase);
        await SaveAsync(set);
    }
}
