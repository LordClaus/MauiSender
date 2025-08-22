// Services/CampaignRunner.cs
using MauiSender.Models;

namespace MauiSender.Services;

public class CampaignRunner
{
    public event Action<int, int, int>? CountersChanged;
    public event Action<int, int, int>? CountersUpdated; // alias for older call sites

    private int _sent;
    private int _failed;
    private int _waiting;

    private DomBridge? _dom;
    private SelectorMap? _selectors;
    private readonly TemplatesRepo _templatesRepo;
    private readonly BlacklistRepo _blacklistRepo;
    private readonly LogsRepo _logsRepo;
    private readonly DelayPolicy _delay;
    private readonly SettingsService _settings;

    private CancellationTokenSource? _cts;
    private bool _paused;

    public bool IsRunning => _cts != null && !_cts.IsCancellationRequested && !_paused;
    public bool IsPaused => _paused;

    // Mode property for backward compatibility
    public TemplateMode Mode { get; set; } = TemplateMode.RandomWeighted;

    // Full constructor (keeps backward-compatibility signature)
    public CampaignRunner(
        TemplatesRepo templatesRepo,
        BlacklistRepo blacklistRepo,
        LogsRepo logsRepo,
        DelayPolicy delay,
        SettingsService settings,
        object optionalA = null,
        object optionalB = null)
    {
        _templatesRepo = templatesRepo;
        _blacklistRepo = blacklistRepo;
        _logsRepo = logsRepo;
        _delay = delay;
        _settings = settings;
    }

    public void AttachDom(DomBridge dom) => _dom = dom;
    public void AttachDom(DomBridge dom, SelectorMap selectors) { _dom = dom; _selectors = selectors; }

    public void Pause() { _paused = true; RaiseCounters(); }
    public void Resume() { _paused = false; RaiseCounters(); }
    public void TogglePause() { _paused = !_paused; RaiseCounters(); }
    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
        _paused = false;
        RaiseCounters();
    }

    private void RaiseCounters()
    {
        CountersChanged?.Invoke(_sent, _failed, _waiting);
        CountersUpdated?.Invoke(_sent, _failed, _waiting);
    }

    // Start using current settings loaded from SettingsService
    public async Task StartAsync()
    {
        var settings = await _settings.LoadAsync();
        await StartAsync(settings, CancellationToken.None);
    }

    // Start with explicit settings (keeps existing signature)
    public async Task StartAsync(SettingsModel settings, CancellationToken? externalToken = null)
    {
        if (_dom == null) throw new InvalidOperationException("DomBridge not attached");
        if (_templatesRepo == null) throw new InvalidOperationException("TemplatesRepo not provided");

        if (_cts != null) return; // already running

        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken ?? CancellationToken.None);
        _paused = false;
        _sent = 0; _failed = 0; _waiting = 0;
        RaiseCounters();

        var templates = await _templatesRepo.LoadAsync();
        templates.ForEach(t => { if (t.Parts.Count == 0) t.ParseParts(); });

        var blacklist = await _blacklistRepo.LoadAsync();
        var selectors = _selectors ?? new SelectorMap();
        // background loop
        _ = Task.Run(async () =>
        {
            try
            {
                while (!_cts.IsCancellationRequested)
                {
                    while (_paused && !_cts.IsCancellationRequested)
                        await Task.Delay(200, _cts.Token);

                    var ids = await _dom.QueryAllIdsAsync(selectors.ListItem, selectors.ListItemIdAttr);
                    var queue = ids.Where(id => !string.IsNullOrWhiteSpace(id) && !blacklist.Contains(id)).ToList();
                    _waiting = queue.Count;
                    RaiseCounters();

                    if (queue.Count == 0)
                    {
                        await Task.Delay(1000, _cts.Token);
                        continue;
                    }

                    foreach (var uid in queue)
                    {
                        if (_cts.IsCancellationRequested) break;
                        while (_paused && !_cts.IsCancellationRequested) await Task.Delay(200, _cts.Token);

                        var tpl = PickTemplate(templates, settings.TemplateMode);
                        if (tpl == null) continue;

                        var ok = await SendToRecipientAsync(uid, tpl, selectors, _cts.Token);
                        if (ok) _sent++; else _failed++;
                        _waiting = Math.Max(0, _waiting - 1);
                        RaiseCounters();

                        await _delay.MessageDelayAsync();
                    }
                }
            }
            catch (OperationCanceledException) { /* cancelled */ }
            finally
            {
                _cts = null;
                _paused = false;
                RaiseCounters();
            }
        });
    }

    private Template? PickTemplate(List<Template> templates, TemplateMode mode)
    {
        var enabled = templates.Where(t => t.Enabled && t.Parts.Any()).ToList();
        if (!enabled.Any()) return null;

        if (mode == TemplateMode.RoundRobin)
        {
            var t = enabled[0];
            enabled.RemoveAt(0);
            enabled.Add(t);
            return t;
        }

        var sum = enabled.Sum(t => Math.Max(1, t.Weight));
        var r = Random.Shared.Next(0, Math.Max(1, sum));
        var acc = 0;
        foreach (var t in enabled)
        {
            acc += Math.Max(1, t.Weight);
            if (r < acc) return t;
        }
        return enabled.First();
    }

    private async Task<bool> SendToRecipientAsync(string uid, Template tpl, SelectorMap selectors, CancellationToken token)
    {
        try
        {
            for (int i = 0; i < tpl.Parts.Count; i++)
            {
                var part = tpl.Parts[i];
                await _delay.HumanSmallAsync();

                var fill = await _dom.FillAsync(selectors.ChatInput, part);
                if (!string.Equals(fill, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    await _logsRepo.AppendAsync(new LogEntry { UserId = uid, TemplateId = tpl.Id, PartIndex = i, Ts = DateTime.UtcNow, Status = "FAIL", Error = fill });
                    return false;
                }

                await _delay.HumanSmallAsync();

                var click = await _dom.ClickAsync(selectors.ChatSend);
                if (!string.Equals(click, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    await _logsRepo.AppendAsync(new LogEntry { UserId = uid, TemplateId = tpl.Id, PartIndex = i, Ts = DateTime.UtcNow, Status = "FAIL", Error = click });
                    return false;
                }

                await _logsRepo.AppendAsync(new LogEntry { UserId = uid, TemplateId = tpl.Id, PartIndex = i, Ts = DateTime.UtcNow, Status = "OK" });
                await _delay.PartDelayAsync();
            }
            return true;
        }
        catch (Exception ex)
        {
            await _logsRepo.AppendAsync(new LogEntry { UserId = uid, TemplateId = tpl.Id, PartIndex = -1, Ts = DateTime.UtcNow, Status = "FAIL", Error = ex.Message });
            return false;
        }
    }
}
