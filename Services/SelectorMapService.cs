using MauiSender.Models;

namespace MauiSender.Services;

public class SelectorMapService
{
    private readonly string _path = FileUtil.PathInData("selectors.json");

    public async Task EnsureAsync()
    {
        await FileUtil.EnsureJsonFileAsync(_path, new SelectorMap());
    }

    public async Task<SelectorMap> LoadAsync()
    {
        return await FileUtil.LoadJsonAsync(_path, new SelectorMap());
    }

    public async Task SaveAsync(SelectorMap map)
    {
        await FileUtil.SaveJsonAsync(_path, map);
    }
}
