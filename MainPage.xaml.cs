using MauiSender.Models;
using MauiSender.Services;
using System.Diagnostics;

namespace MauiSender;

public partial class MainPage : ContentPage
{
    // сервіси / стани
    private DomBridge _dom = default!;
    private TemplatesRepo _templates = default!;
    private BlacklistRepo _blacklist = default!;
    private LogsRepo _logs = default!;
    private SettingsService _settings = default!;
    private DelayPolicy _delay = default!;
    private Navigator _nav = default!;
    private CampaignRunner _runner = default!;
    private CancellationTokenSource? _cts;

    // шляхи до JSON
    private readonly string _templatesPath = Path.Combine(FileSystem.AppDataDirectory, "templates.json");
    private readonly string _blacklistPath = Path.Combine(FileSystem.AppDataDirectory, "blacklist.json");
    private readonly string _logsPath = Path.Combine(FileSystem.AppDataDirectory, "logs.json");
    private readonly string _settingsPath = Path.Combine(FileSystem.AppDataDirectory, "settings.json");

    public MainPage()
    {
        InitializeComponent();

        // гарантуємо наявність AppDataDirectory
        Directory.CreateDirectory(FileSystem.AppDataDirectory);

        // 1) репозиторії (вони самі створять файли, якщо нема)
        _templates = new TemplatesRepo(_templatesPath);
        _blacklist = new BlacklistRepo(_blacklistPath);
        _logs = new LogsRepo(_logsPath);
        _settings = new SettingsService(_settingsPath);

        // 2) Web/DOM
        _dom = new DomBridge(MyWebView);
        _nav = new Navigator(MyWebView);

        // 3) Політика затримок
        _delay = new DelayPolicy
        {
            MessageIntervalSec = _settings.Current.MessageIntervalSec,
            PartDelaySec = _settings.Current.PartDelaySec
        };

        // 4) Runner
        _runner = new CampaignRunner(_dom, _templates, _blacklist, _logs, _delay, _nav, _settings)
        {
            InputSelector = "#messageBox, textarea[name='message'], textarea",
            SendButtonSelector = "button.send, button[type=submit], .send-btn",
            Mode = TemplateMode.Random
        };

        // Picker -> enum
        PickerMode.SelectedIndex = 0; // Random
        EntryBaseInterval.Text = _settings.Current.MessageIntervalSec.ToString();
        EntryPartDelay.Text = _settings.Current.PartDelaySec.ToString();
    }

    private void OnReloadClicked(object sender, EventArgs e)
    {
        // якщо ти редагував templates.json поза додатком — перезавантаж
        _templates = new TemplatesRepo(_templatesPath);
        Debug.WriteLine("Templates reloaded");
    }

    private void Web_Navigating(object sender, WebNavigatingEventArgs e)
    {
        Debug.WriteLine($"Navigating: {e.Url}");
        // приклад: блок зовнішніх доменів
        // if (!e.Url.Contains("goldenbride")) e.Cancel = true;
    }

    private void Web_Navigated(object sender, WebNavigatedEventArgs e)
    {
        Debug.WriteLine($"Navigated: {e.Url}");
    }

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        if (_cts != null) return; // уже працює

        try
        {
            // зчитати UI-настройки
            _settings.Current.MessageIntervalSec = ParseInt(EntryBaseInterval.Text, 30);
            _settings.Current.PartDelaySec = ParseInt(EntryPartDelay.Text, _settings.Current.MessageIntervalSec);

            var modeStr = (PickerMode.SelectedItem as string) ?? "Random";
            _settings.Current.Mode = Enum.TryParse<TemplateMode>(modeStr, out var m) ? m : TemplateMode.Random;
            _settings.Save();

            _delay.MessageIntervalSec = _settings.Current.MessageIntervalSec;
            _delay.PartDelaySec = _settings.Current.PartDelaySec;

            _runner.Mode = _settings.Current.Mode;

            // UI
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;

            _cts = new CancellationTokenSource();
            await _runner.RunAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {
            // нормальне зупинення
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }
    }

    private void OnStopClicked(object? sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    // CRUD по шаблонах
    private async void OnAddTemplateClicked(object? sender, EventArgs e)
    {
        var text = await DisplayPromptAsync("Add template", "Enter message text (use --- or ---N--- for delays):", "Save", "Cancel", maxLength: 4000);
        if (!string.IsNullOrWhiteSpace(text))
            _templates.Add(text.Trim());
    }

    private async void OnEditTemplateClicked(object? sender, EventArgs e)
    {
        if (_templates.All.Count == 0)
        {
            await DisplayAlert("Templates", "No templates yet.", "OK");
            return;
        }

        var items = _templates.All.Select((t, i) => $"{i + 1}. {Short(t)}").ToArray();
        var choice = await DisplayActionSheet("Pick a template to edit", "Cancel", null, items);
        if (choice == "Cancel" || string.IsNullOrEmpty(choice)) return;

        var idx = Array.IndexOf(items, choice);
        var current = _templates.All[idx];

        var edited = await DisplayPromptAsync("Edit template", "Update text:", "Save", "Cancel", current, maxLength: 4000);
        if (edited is not null)
            _templates.UpdateAt(idx, edited);
    }

    private async void OnDeleteTemplateClicked(object? sender, EventArgs e)
    {
        if (_templates.All.Count == 0)
        {
            await DisplayAlert("Templates", "No templates yet.", "OK");
            return;
        }

        var items = _templates.All.Select((t, i) => $"{i + 1}. {Short(t)}").ToArray();
        var choice = await DisplayActionSheet("Pick a template to delete", "Cancel", null, items);
        if (choice == "Cancel" || string.IsNullOrEmpty(choice)) return;

        var idx = Array.IndexOf(items, choice);
        var confirm = await DisplayAlert("Confirm", $"Delete template #{idx + 1}?", "Yes", "No");
        if (confirm) _templates.RemoveAt(idx);
    }

    private static int ParseInt(string? s, int def)
        => int.TryParse(s, out var v) && v > 0 ? v : def;

    private static string Short(string s)
    {
        s = s.Replace("\r", " ").Replace("\n", " ").Trim();
        return s.Length > 60 ? s.Substring(0, 60) + "…" : s;
    }
}
