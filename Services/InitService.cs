namespace MauiSender.Services;

public class InitService
{
    private readonly AccountsService _accounts;
    private readonly TemplatesRepo _templates;
    private readonly BlacklistRepo _blacklist;
    private readonly LogsRepo _logs;
    private readonly SelectorMapService _selectors;
    private readonly SettingsService _settings;

    public InitService(
        AccountsService accounts,
        TemplatesRepo templates,
        BlacklistRepo blacklist,
        LogsRepo logs,
        SelectorMapService selectors,
        SettingsService settings)
    {
        _accounts = accounts;
        _templates = templates;
        _blacklist = blacklist;
        _logs = logs;
        _selectors = selectors;
        _settings = settings;
    }

    public async Task EnsureAllAsync()
    {
        // Впевнитися, що всі необхідні файли існують (створити, якщо нема)
        var tasks = new List<Task>
        {
            _settings.EnsureAsync(),
            _accounts.EnsureAsync(),
            _templates.EnsureAsync(),
            _blacklist.EnsureAsync(),
            _logs.EnsureAsync(),
            _selectors.EnsureAsync()
        };
        await Task.WhenAll(tasks);
    }

    // Синхронний варіант для виклику під час старту (без async/await в App ctor)
    public void EnsureAllSync() => EnsureAllAsync().GetAwaiter().GetResult();
}
