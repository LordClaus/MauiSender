// MauiSender/ViewModels/ChatViewModel.cs
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSender.Models;
using MauiSender.Services;

namespace MauiSender.ViewModels;

public partial class ChatViewModel : ObservableObject
{
    private readonly SettingsService _settings;
    private readonly TemplatesRepo _templates;
    private readonly BlacklistRepo _blacklist;
    private readonly LogsRepo _logs;
    private readonly SelectorMapService _selectors;

    private WebView? _web;
    private DomBridge? _dom;
    private Navigator? _nav;
    private CampaignRunner? _runner;

    [ObservableProperty] private ObservableCollection<string> templateNames = new();
    [ObservableProperty] private string? selectedTemplateName;
    [ObservableProperty] private ObservableCollection<string> rotationModes = new(new[] { "randomWeighted", "roundRobin" });
    [ObservableProperty] private string selectedRotationMode = "randomWeighted";

    [ObservableProperty] private int messageIntervalSec = 60;
    [ObservableProperty] private int partIntervalSec = 60;
    [ObservableProperty] private int jitterPct = 10;

    [ObservableProperty] private string countersText = "Sent: 0 | Failed: 0 | Waiting: 0";
    [ObservableProperty] private string activeTemplateText = "Шаблон: —";
    [ObservableProperty] private string nextRecipientText = "Наступний: —";
    [ObservableProperty] private string currentAccountText = "Акаунт: —";

    private Account? _account;

    public ChatViewModel(SettingsService settings, TemplatesRepo templates, BlacklistRepo blacklist, LogsRepo logs, SelectorMapService selectors)
    {
        _settings = settings;
        _templates = templates;
        _blacklist = blacklist;
        _logs = logs;
        _selectors = selectors;

        _ = EnsureInitAsync();
    }

    public void AttachWebView(WebView web)
    {
        _web = web;
        _dom = new DomBridge(_web);
    }

    public void SetCurrentAccount(Account acc)
    {
        _account = acc;
        CurrentAccountText = $"Акаунт: {acc.Login}";
    }

    public async Task EnsureInitAsync()
    {
        var s = await _settings.LoadAsync();
        MessageIntervalSec = s.MessageIntervalSec;
        PartIntervalSec = s.PartIntervalSec;
        JitterPct = s.JitterPct;
        SelectedRotationMode = s.TemplateMode;

        await _blacklist.LoadAsync();
        var tpls = await _templates.LoadAsync();
        TemplateNames = new ObservableCollection<string>(tpls.Select(t => t.Name));
        SelectedTemplateName = TemplateNames.FirstOrDefault();

        await _selectors.LoadAsync();
    }

    [RelayCommand]
    private async Task Start()
    {
        if (_web is null || _dom is null) return;
        var sel = _selectors.Current;

        var delay = new DelayPolicy(MessageIntervalSec, PartIntervalSec, JitterPct);
        _nav = new Navigator(_dom, sel);
        _runner = new CampaignRunner(_dom, _templates, _blacklist, _logs, _nav, delay, _settings);
        _runner.CountersChanged += (snt, fail, wait) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CountersText = $"Sent: {snt} | Failed: {fail} | Waiting: {wait}";
            });
        };

        if (_account is not null)
        {
            await _dom.AutoLoginAsync(_account.Login, _account.Password);
        }

        _ = _runner.StartAsync(SelectedRotationMode);
    }

    [RelayCommand]
    private void Stop()
    {
        _runner?.Stop();
    }

    [RelayCommand]
    private void PauseResume()
    {
        if (_runner?.IsRunning == true) _runner.Pause();
        else _ = Start();
    }

    [RelayCommand]
    private async Task ManageTemplates()
    {
        var list = _templates.All;
        if (list.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert("Шаблони", "Немає шаблонів у templates.json", "OK");
            return;
        }
        var names = string.Join("\n", list.Select(t => $"{t.Name} ({t.Parts.Count} parts, w={t.Weight})"));
        await Application.Current!.MainPage!.DisplayAlert("Шаблони", names, "OK");
    }

    [RelayCommand]
    private async Task ManageBlacklist()
    {
        await Application.Current!.MainPage!.DisplayAlert("BlackList", "Редагуйте файл blacklist.json вручну у директорії AppData", "OK");
    }

    public async Task OnChatNavigatedAsync()
    {
        if (_account is not null && _dom is not null)
        {
            await _dom.AutoLoginAsync(_account.Login, _account.Password);
        }
    }
}
