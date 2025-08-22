using MauiSender.Models;

namespace MauiSender.Services;

public class InitService
{
    private readonly SettingsService _settingsService;
    private readonly AccountsService _accounts;
    private readonly TemplatesRepo _templates;
    private readonly BlacklistRepo _blacklist;
    private readonly SelectorMapService _selectors;
    private readonly LogsRepo _logs;

    public InitService(
        SettingsService settingsService,
        AccountsService accounts,
        TemplatesRepo templates,
        BlacklistRepo blacklist,
        SelectorMapService selectors,
        LogsRepo logs)
    {
        _settingsService = settingsService;
        _accounts = accounts;
        _templates = templates;
        _blacklist = blacklist;
        _selectors = selectors;
        _logs = logs;
    }

    public async Task InitAsync()
    {
        await _settingsService.EnsureAsync();
        await _accounts.EnsureAsync();
        await _templates.EnsureAsync();
        await _blacklist.EnsureAsync();
        await _selectors.EnsureAsync();
        await _logs.EnsureAsync();
    }
}
