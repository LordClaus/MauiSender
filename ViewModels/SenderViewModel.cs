using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MauiSender.Models;
using MauiSender.Services;

namespace MauiSender.ViewModels;

public class SenderViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly SettingsService _settings;
    private readonly TemplatesRepo _templatesRepo;
    private readonly AccountsService _accounts;
    private readonly BlacklistRepo _blacklist;
    private readonly LogsRepo _logs;

    private DomBridge? _dom;
    private Navigator? _navigator;
    private CampaignRunner? _runner;
    private SettingsModel _cfg = new();

    public ObservableCollection<Account> Accounts { get; } = new();
    public ObservableCollection<Template> Templates { get; } = new();

    private int _sent;
    public int Sent { get => _sent; set { _sent = value; OnPropertyChanged(); } }

    private int _failed;
    public int Failed { get => _failed; set { _failed = value; OnPropertyChanged(); } }

    private int _waiting;
    public int Waiting { get => _waiting; set { _waiting = value; OnPropertyChanged(); } }

    public SenderViewModel(
        SettingsService settings,
        TemplatesRepo templatesRepo,
        AccountsService accounts,
        BlacklistRepo blacklist,
        LogsRepo logs)
    {
        _settings = settings;
        _templatesRepo = templatesRepo;
        _accounts = accounts;
        _blacklist = blacklist;
        _logs = logs;
    }

    public async Task LoadAsync()
    {
        _cfg = await _settings.LoadAsync();

        Accounts.Clear();
        foreach (var a in await _accounts.LoadAsync())
            Accounts.Add(a);

        Templates.Clear();
        foreach (var t in await _templatesRepo.LoadAsync())
        {
            if (t.Parts.Count == 0) t.ParseParts();
            Templates.Add(t);
        }
    }

    public void AttachDom(WebView webView, SelectorMap selectors, SettingsModel? effectiveSettings = null)
    {
        _dom = new DomBridge(webView);
        _dom.AttachSelectors(selectors);
        _navigator = new Navigator(_dom);

        var cfg = effectiveSettings ?? _cfg;
        var delay = new DelayPolicy(cfg.BaseIntervalSec, cfg.PartIntervalSec, cfg.JitterPct);

        _runner = new CampaignRunner(_dom, _navigator, delay, _templatesRepo, _blacklist, _logs);
        _runner.CountersChanged += (s, f, w) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Sent = s;
                Failed = f;
                Waiting = w;
            });
        };
    }

    public async Task StartAsync()
    {
        if (_runner is null) return;
        await _runner.StartAsync(_cfg);
    }

    public void Pause() => _runner?.Pause();

    public void Resume() => _runner?.Resume();

    public void Stop() => _runner?.Stop();

    public async Task AddAccountAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            return;

        // простенька валідація логіна
        if (!Regex.IsMatch(login, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") && login.Any(char.IsWhiteSpace))
            return;

        var a = new Account { Login = login.Trim(), Password = password };
        await _accounts.AddAsync(a);
        Accounts.Add(a);
    }

    public async Task RemoveAccountAsync(Account? a)
    {
        if (a is null) return;
        await _accounts.RemoveAsync(a.Login);
        Accounts.Remove(a);
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
