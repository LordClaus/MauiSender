using MauiSender.Models;

namespace MauiSender.Services;

public class SettingsService
{
    private readonly string _path = FileUtil.PathInData("settings.json");

    public async Task EnsureAsync()
        => await FileUtil.EnsureJsonFileAsync(_path, new SettingsModel());

    public async Task<SettingsModel> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new SettingsModel());

    public async Task SaveAsync(SettingsModel cfg)
        => await FileUtil.SaveJsonAsync(_path, cfg);
}
