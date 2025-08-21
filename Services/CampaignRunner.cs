using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiSender.Models;

namespace MauiSender.Services;

public class CampaignCountersEventArgs : EventArgs
{
    public int Sent { get; init; }
    public int Failed { get; init; }
    public int Waiting { get; init; }
    public string? ActiveTemplateName { get; init; }
    public string? NextRecipientId { get; init; }
}

public class CampaignRunner
{
    private readonly DomBridge _bridge;
    private readonly SelectorMap _sel;
    private readonly TemplateRepo _templates;
    private readonly BlacklistRepo _blacklist;
    private readonly LogsRepo _logs;
    private readonly Navigator _nav;
    private readonly DelayPolicy _delay;

    private CancellationTokenSource? _cts;
    private int _sent, _failed;
    private string? _nextRecipient;

    public bool IsRunning => _cts != null;
    public bool IsPaused { get; private set; }
    public string Mode { get; set; } = "randomWeighted";

    public event EventHandler<CampaignCountersEventArgs>? CountersUpdated;

    public CampaignRunner(DomBridge bridge, SelectorMap sel, TemplateRepo templates, BlacklistRepo blacklist, LogsRepo logs, Navigator nav, DelayPolicy delay)
    {
        _bridge = bridge; _sel = sel; _templates = templates; _blacklist = blacklist; _logs = logs; _nav = nav; _delay = delay;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _ = Task.Run(() => LoopAsync(_cts.Token));
        OnCounters();
        await Task.CompletedTask;
    }

    public void TogglePause() => IsPaused = !IsPaused;
    public void Stop() { _cts?.Cancel(); _cts = null; }

    private async Task LoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (IsPaused) { await Task.Delay(300, ct); continue; }

            var recipients = await _nav.GetRecipientsAsync();
            foreach (var rid in recipients)
            {
                if (ct.IsCancellationRequested) break;
                if (_blacklist.Contains(rid)) continue;

                _nextRecipient = rid;
                OnCounters();

                var opened = await _nav.OpenChatAsync(rid);
                if (!opened || !await _nav.EnsureInputReadyAsync())
                {
                    _failed++;
                    _logs.Add(new LogEntry { UserId = rid, Status = "FAILED", Error = "NO_INPUT/OPEN" });
                    OnCounters();
                    await _delay.MessageDelayAsync(ct);
                    continue;
                }

                var template = _templates.Pick(Mode);
                for (int i = 0; i < template.Parts.Length; i++)
                {
                    var part = template.Parts[i];
                    await _delay.HumanizeAsync(ct);
                    var f = await _bridge.FillAsync(_sel["chat.input"], part);
                    if (f != "OK")
                    {
                        _failed++;
                        _logs.Add(new LogEntry { UserId = rid, TemplateId = template.Id, PartIndex = i, Status = "FAILED", Error = f });
                        OnCounters();
                        break;
                    }
                    await _delay.HumanizeAsync(ct);
                    var c = await _bridge.ClickAsync(_sel["chat.send"]);
                    if (c != "OK")
                    {
                        _failed++;
                        _logs.Add(new LogEntry { UserId = rid, TemplateId = template.Id, PartIndex = i, Status = "FAILED", Error = c });
                        OnCounters();
                        break;
                    }
                    _logs.Add(new LogEntry { UserId = rid, TemplateId = template.Id, PartIndex = i, Status = "SENT" });
                    OnCounters();
                    await _delay.PartDelayAsync(ct);
                }

                _sent++;
                OnCounters();
                await _delay.MessageDelayAsync(ct);
            }

            await Task.Delay(1000, ct); // refresh cycle
        }
    }

    private void OnCounters()
        => CountersUpdated?.Invoke(this, new CampaignCountersEventArgs { Sent = _sent, Failed = _failed, Waiting = Math.Max(0, 0), ActiveTemplateName = null, NextRecipientId = _nextRecipient });
}
