using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSender.Services;

public class DelayPolicy
{
    private readonly Func<int> _msg;
    private readonly Func<int> _part;
    private readonly Func<int> _jitter;

    public DelayPolicy(Func<int> messageIntervalSec, Func<int> partIntervalSec, Func<int> jitterPct)
    {
        _msg = messageIntervalSec;
        _part = partIntervalSec;
        _jitter = jitterPct;
    }

    private int ApplyJitter(int baseMs)
    {
        var pct = Math.Clamp(_jitter(), 0, 30);
        var span = (int)(baseMs * (pct / 100.0));
        var delta = Random.Shared.Next(-span, span + 1);
        return Math.Max(0, baseMs + delta);
    }

    public async Task MessageDelayAsync(CancellationToken ct)
    {
        var ms = ApplyJitter(_msg() * 1000);
        await Task.Delay(ms, ct);
    }

    public async Task PartDelayAsync(CancellationToken ct)
    {
        var baseSec = _part() > 0 ? _part() : _msg();
        var ms = ApplyJitter(baseSec * 1000);
        await Task.Delay(ms, ct);
    }

    public async Task HumanizeAsync(CancellationToken ct)
    {
        // small delay before clicks/typing
        var ms = Random.Shared.Next(200, 800);
        await Task.Delay(ms, ct);
    }
}
