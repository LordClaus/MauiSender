using MauiSender.Models;

namespace MauiSender.Services;

public class CampaignRunner
{
    private readonly DomBridge _dom;
    private readonly Navigator _nav;
    private readonly DelayPolicy _delay;
    private readonly TemplatesRepo _templatesRepo;
    private readonly BlacklistRepo _blacklistRepo;
    private readonly LogsRepo _logs;

    private CancellationTokenSource? _cts;
    private bool _paused;
    private int _sent;
    private int _failed;
    private int _waiting;

    public event Action<int, int, int>? CountersChanged;

    public bool IsRunning => _cts is not null && !_cts.IsCancellationRequested && !_paused;

    public CampaignRunner(
        DomBridge dom,
        Navigator nav,
        DelayPolicy delay,
        TemplatesRepo templatesRepo,
        BlacklistRepo blacklistRepo,
        LogsRepo logs)
    {
        _dom = dom;
        _nav = nav;
        _delay = delay;
        _templatesRepo = templatesRepo;
        _blacklistRepo = blacklistRepo;
        _logs = logs;
    }

    public void Pause() => _paused = true;

    public void Resume() => _paused = false;

    public void Stop()
    {
        _cts?.Cancel();
        _cts = null;
        _paused = false;
        UpdateCounters();
    }

    public async Task StartAsync(SettingsModel settings, CancellationToken? externalToken = null)
    {
        if (_cts is not null) return;

        _cts = CancellationTokenSource.CreateLinkedTokenSource(externalToken ?? CancellationToken.None);
        _paused = false;
        _sent = 0;
        _failed = 0;

        var templates = (await _templatesRepo.LoadAsync())
            .Where(t => t.Enabled)
            .ToList();
        templates.ForEach(t =>
        {
            if (t.Parts.Count == 0) t.ParseParts();
        });

        var blacklist = await _blacklistRepo.LoadAsync();

        _ = RunLoopAsync(settings, templates, blacklist, _cts.Token);
    }

    private async Task RunLoopAsync(SettingsModel settings, List<Template> templates, HashSet<string> blacklist, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            while (_paused && !token.IsCancellationRequested)
                await Task.Delay(100, token);

            var ids = await _nav.GetRecipientIdsAsync();
            var queue = ids.Where(id => !string.IsNullOrWhiteSpace(id) && !blacklist.Contains(id)).ToList();
            _waiting = queue.Count;
            UpdateCounters();

            foreach (var recipientId in queue)
            {
                if (token.IsCancellationRequested) break;
                while (_paused && !token.IsCancellationRequested)
                    await Task.Delay(100, token);

                var template = PickTemplate(templates, settings.TemplateMode);
                if (template is null) { await Task.Delay(1000, token); continue; }

                bool ok = await SendToRecipientAsync(recipientId, template, token);
                if (ok) _sent++; else _failed++;
                _waiting = Math.Max(0, _waiting - 1);
                UpdateCounters();

                await _delay.MessageDelayAsync();
            }
        }
    }

    private Template? PickTemplate(List<Template> list, TemplateMode mode)
    {
        if (list.Count == 0) return null;

        if (mode == TemplateMode.RoundRobin)
        {
            var t = list[0];
            list.RemoveAt(0);
            list.Add(t);
            return t;
        }

        // RandomWeighted
        int sum = list.Where(t => t.Enabled).Sum(t => Math.Max(1, t.Weight));
        if (sum <= 0) return list[Random.Shared.Next(list.Count)];

        int roll = Random.Shared.Next(1, sum + 1);
        int acc = 0;
        foreach (var t in list)
        {
            int w = Math.Max(1, t.Weight);
            acc += w;
            if (roll <= acc) return t;
        }

        return list[Random.Shared.Next(list.Count)];
    }

    private async Task<bool> SendToRecipientAsync(string recipientId, Template template, CancellationToken token)
    {
        try
        {
            // Тут ти можеш клікнути по картці/відкрити чат для конкретного recipientId,
            // але у нас немає гарантованого селектора елемента за id — це лишається на SelectorMap,
            // тож відправляємо у відкритий чат (MVP).

            foreach (var (part, index) in template.Parts.Select((p, i) => (p, i)))
            {
                if (token.IsCancellationRequested) break;

                await _delay.SmallHumanDelayAsync();

                var fillRes = await _dom.FillAsync(_dom.Selectors.ChatInput, part);
                if (fillRes is null || !fillRes.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    await _logs.AppendAsync(new LogEntry
                    {
                        UserId = recipientId,
                        TemplateId = template.Id,
                        PartIndex = index,
                        Status = "FAIL",
                        Error = fillRes ?? "Fill failed"
                    });
                    return false;
                }

                await _delay.SmallHumanDelayAsync();

                var clickRes = await _dom.ClickAsync(_dom.Selectors.ChatSend);
                if (clickRes is null || !clickRes.Equals("OK", StringComparison.OrdinalIgnoreCase))
                {
                    await _logs.AppendAsync(new LogEntry
                    {
                        UserId = recipientId,
                        TemplateId = template.Id,
                        PartIndex = index,
                        Status = "FAIL",
                        Error = clickRes ?? "Click failed"
                    });
                    return false;
                }

                await _logs.AppendAsync(new LogEntry
                {
                    UserId = recipientId,
                    TemplateId = template.Id,
                    PartIndex = index,
                    Status = "OK"
                });

                await _delay.PartDelayAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            await _logs.AppendAsync(new LogEntry
            {
                UserId = recipientId,
                TemplateId = template.Id,
                PartIndex = -1,
                Status = "FAIL",
                Error = ex.Message
            });
            return false;
        }
    }

    private void UpdateCounters() => CountersChanged?.Invoke(_sent, _failed, _waiting);
}
