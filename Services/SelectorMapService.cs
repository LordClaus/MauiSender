using MauiSender.Models;

namespace MauiSender.Services;

public class SelectorMapService
{
    private readonly string _path;

    public SelectorMapService(SettingsService settingsService)
    {
        // шлях зчитуємо з settings, але якщо settings ще не створений – fallback
        var defPath = FileUtil.PathInData("selectors.json");
        _path = defPath;
    }

    public async Task EnsureAsync()
    {
        var def = new SelectorMap();
        await FileUtil.EnsureJsonFileAsync(_path, def);
    }

    public async Task<SelectorMap> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new SelectorMap());

    public async Task SaveAsync(SelectorMap map)
        => await FileUtil.SaveJsonAsync(_path, map);
}
