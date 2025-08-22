// ViewModels/SenderViewModel.cs
using MauiSender.Models;
using MauiSender.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiSender.ViewModels;

public class SenderViewModel : INotifyPropertyChanged
{
    private readonly AccountsService _accounts;
    private readonly TemplatesRepo _templates;
    private readonly SelectorMapService _selectorService;
    private readonly SettingsService _settings;
    private readonly CampaignRunner _runner;
    private readonly LogsRepo _logs;

    private DomBridge? _dom;
    private SelectorMap? _selectors;

    public ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<Template> Templates { get; } = new();

    private int _sent;
    public int Sent { get => _sent; private set { _sent = value; Notify(); } }

    private int _failed;
    public int Failed { get => _failed; private set { _failed = value; Notify(); } }

    private int _waiting;
    public int Waiting { get => _waiting; private set { _waiting = value; Notify(); } }

    public SenderViewModel(
        AccountsService accounts,
        TemplatesRepo templates,
        SelectorMapService selectorService,
        SettingsService settings,
        CampaignRunner runner,
        LogsRepo logs)
    {
        _accounts = accounts;
        _templates = templates;
        _selectorService = selectorService;
        _settings = settings;
        _runner = runner;
        _logs = logs;
    }

    public async Task InitializeAsync()
    {
        await _accounts.EnsureAsync();
        Accounts.Clear();
        var accs = await _accounts.LoadAsync();
        foreach (var a in accs) Accounts.Add(a);

        await _templates.EnsureAsync();
        Templates.Clear();
        var tpls = await _templates.LoadAsync();
        foreach (var t in tpls)
        {
            if (t.Parts.Count == 0) t.ParseParts();
            Templates.Add(t);
        }

        await _selector_service_ensure_load();
        var settings = await _settings.LoadAsync();

        _runner.CountersChanged += (s, f, w) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Sent = s; Failed = f; Waiting = w;
            });
        };
    }

    private async Task _selector_service_ensure_load()
    {
        await _selectorService.EnsureAsync();
        _selectors = await _selectorService.LoadAsync();
    }

    // Attach DOM variants for different call sites
    public void AttachDom(WebView webView)
    {
        _dom = new DomBridge(webView);
        if (_selectors != null) _dom.AttachSelectors(_selectors);
        _runner.AttachDom(_dom, _selectors ?? new SelectorMap());
    }

    public void AttachDom(DomBridge dom, SelectorMap selectors)
    {
        _dom = dom;
        _selectors = selectors;
        _dom.AttachSelectors(selectors);
        _runner.AttachDom(dom, selectors);
    }

    public async Task StartAsync()
    {
        var cfg = await _settings.LoadAsync();
        await _runner.StartAsync(cfg);
    }

    public void Pause() => _runner.Pause();
    public void Resume() => _runner.Resume();
    public void Stop() => _runner.Stop();

    public async Task AddAccountAsync(string login, string password)
    {
        var acc = new Account { Login = login?.Trim() ?? string.Empty, Password = password ?? string.Empty };
        await _accounts.AddAsync(acc);
        Accounts.Add(acc);
    }

    public async Task RemoveAccountAsync(Account a)
    {
        if (a == null) return;
        await _accounts.RemoveByLoginAsync(a.Login);
        Accounts.Remove(a);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}
