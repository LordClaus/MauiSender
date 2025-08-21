using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiSender.Services;
using MauiSender.Models;
using CommunityToolkit.Mvvm.Input;
using System.Text.RegularExpressions;

namespace MauiSender.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

    public ObservableCollection<string> TemplateNames { get; } = new();
    public ObservableCollection<string> Modes { get; } = new() { "randomWeighted", "roundRobin" };

    private string _selectedTemplateName = "Ротація";
    public string SelectedTemplateName { get => _selectedTemplateName; set { _selectedTemplateName = value; OnPropertyChanged(); } }

    private string _selectedMode = "randomWeighted";
    public string SelectedMode { get => _selectedMode; set { _selectedMode = value; OnPropertyChanged(); } }

    private int _baseIntervalSec = 45;
    public int BaseIntervalSec { get => _baseIntervalSec; set { _baseIntervalSec = value; OnPropertyChanged(); } }

    private int _partIntervalSec = 45;
    public int PartIntervalSec { get => _partIntervalSec; set { _partIntervalSec = value; OnPropertyChanged(); } }

    private int _jitterPct = 15;
    public int JitterPct { get => _jitterPct; set { _jitterPct = value; OnPropertyChanged(); } }

    public string PauseResumeText => _runner?.IsPaused == true ? "Resume" : "Pause";

    private string _countersText = "Sent 0 | Failed 0 | Waiting 0";
    public string CountersText { get => _countersText; set { _countersText = value; OnPropertyChanged(); } }

    private string _activeTemplateText = "Шаблон: —";
    public string ActiveTemplateText { get => _activeTemplateText; set { _activeTemplateText = value; OnPropertyChanged(); } }

    private string _nextRecipientText = "Наступний: —";
    public string NextRecipientText { get => _nextRecipientText; set { _nextRecipientText = value; OnPropertyChanged(); } }

    private string _baseUrl = "https://goldenbride.com/";
    public string BaseUrl { get => _baseUrl; set { _baseUrl = value; OnPropertyChanged(); } }

    private WebView? _web;
    private DomBridge? _bridge;
    private SelectorMap? _selectors;
    private TemplateRepo? _templates;
    private BlacklistRepo? _blacklist;
    private LogsRepo? _logs;
    private SettingsService? _settings;
    private CampaignRunner? _runner;
    private Navigator? _navigator;
    private DelayPolicy? _delay;

    public ICommand StartCommand => new AsyncRelayCommand(StartAsync);
    public ICommand PauseResumeCommand => new RelayCommand(TogglePause);
    public ICommand StopCommand => new RelayCommand(Stop);
    public ICommand ManageTemplatesCommand => new AsyncRelayCommand(ManageTemplatesAsync);
    public ICommand ManageBlacklistCommand => new AsyncRelayCommand(ManageBlacklistAsync);
    public ICommand ExportLogsCommand => new AsyncRelayCommand(ExportLogsAsync);

    public MainViewModel()
    {
        // ensure DB init and load templates list later when webview attaches
    }

    public void AttachWebView(WebView wv)
    {
        _web = wv;
        _bridge = new DomBridge(_web);
        _selectors = SelectorMap.LoadFromEmbedded();
        _templates = new TemplateRepo();
        _blacklist = new BlacklistRepo();
        _logs = new LogsRepo();
        _settings = new SettingsService();
        _delay = new DelayPolicy(() => BaseIntervalSec, () => PartIntervalSec, () => JitterPct);
        _navigator = new Navigator(_bridge, _selectors);

        RefreshTemplateNames();
    }

    private void RefreshTemplateNames()
    {
        TemplateNames.Clear();
        TemplateNames.Add("Ротація");
        if (_templates == null) return;
        foreach (var t in _templates.GetAllEnabled())
            TemplateNames.Add(t.Name);
    }

    private async Task StartAsync()
    {
        if (_runner != null && _runner.IsRunning) return;
        if (_bridge == null || _selectors == null || _templates == null || _blacklist == null || _logs == null || _delay == null || _navigator == null)
            return;

        _runner = new CampaignRunner(_bridge, _selectors, _templates, _blacklist, _logs, _navigator, _delay)
        {
            Mode = SelectedMode
        };

        _runner.CountersUpdated += (s, e) =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                CountersText = $"Sent {e.Sent} | Failed {e.Failed} | Waiting {e.Waiting}";
                ActiveTemplateText = $"Шаблон: {e.ActiveTemplateName ?? "—"}";
                NextRecipientText = $"Наступний: {e.NextRecipientId ?? "—"}";
                OnPropertyChanged(nameof(PauseResumeText));
            });
        };

        await _runner.StartAsync();
    }

    private void TogglePause()
    {
        _runner?.TogglePause();
        OnPropertyChanged(nameof(PauseResumeText));
    }

    private void Stop()
    {
        _runner?.Stop();
        OnPropertyChanged(nameof(PauseResumeText));
    }

    private async Task ManageTemplatesAsync()
    {
        if (_templates == null) return;
        // very simple UX: prompt for JSON and save
        string? json = await Application.Current!.MainPage!.DisplayPromptAsync("Templates JSON",
            "Встав JSON масив шаблонів або залиш порожнім для прикладу.\nЗбереження перезапише поточні.",
            accept: "Зберегти", cancel: "Скасувати", initialValue: "");

        if (json is null) return;

        if (string.IsNullOrWhiteSpace(json))
        {
            var example = new[]
            {
                new Template { Name="Привітання", Enabled=true, Weight=1, RawText="Привіт! --- Як справи?" },
                new Template { Name="Легкий старт", Enabled=true, Weight=2, RawText="Вітаю! Я побачила твій профіль і вирішила написати :)" }
            };
            _templates.ReplaceAll(example);
        }
        else
        {
            try
            {
                var items = JsonSerializer.Deserialize<List<Template>>(json);
                if (items != null) _templates.ReplaceAll(items);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Помилка", ex.Message, "OK");
            }
        }

        RefreshTemplateNames();
    }

    private async Task ManageBlacklistAsync()
    {
        if (_blacklist == null) return;
        string? csv = await Application.Current!.MainPage!.DisplayPromptAsync("Blacklist CSV",
            "Встав список userId через кому або пробіл.\nПорожньо — очистити.",
            accept: "ОК", cancel: "Скасувати", initialValue: "");

        if (csv is null) return;
        if (string.IsNullOrWhiteSpace(csv))
        {
            _blacklist.ReplaceAll(Array.Empty<string>());
        }
        else
        {
            var ids = Regex.Split(csv, @"[,\s]+").Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
            _blacklist.ReplaceAll(ids);
        }
    }

    private async Task ExportLogsAsync()
    {
        if (_logs == null) return;
        var path = await _logs.ExportCsvAsync();
        await Application.Current!.MainPage!.DisplayAlert("Експорт", $"CSV збережено: {path}", "OK");
    }
}
