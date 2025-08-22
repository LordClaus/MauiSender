using MauiSender.Models;

namespace MauiSender.Services;

public class SettingsService
{
    private readonly string _path = FileUtil.PathInData("settings.json");

    public async Task EnsureAsync()
    {
        var def = new SettingsModel();
        await FileUtil.EnsureJsonFileAsync(_path, def);
    }

    public async Task<SettingsModel> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new SettingsModel());

    public async Task SaveAsync(SettingsModel settings)
        => await FileUtil.SaveJsonAsync(_path, settings);
}
