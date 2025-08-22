namespace MauiSender.Services;

public class DelayPolicy
{
    private readonly int _baseIntervalMs;
    private readonly int _partIntervalMs;
    private readonly int _jitterPct;
    private readonly Random _rnd = new();

    public DelayPolicy(int baseIntervalSec, int partIntervalSec, int jitterPct)
    {
        _baseIntervalMs = Math.Max(0, baseIntervalSec) * 1000;
        _partIntervalMs = Math.Max(0, partIntervalSec) * 1000;
        _jitterPct = Math.Clamp(jitterPct, 0, 30);
    }

    private int WithJitter(int ms)
    {
        if (_jitterPct <= 0) return ms;
        var delta = (int)(ms * (_jitterPct / 100.0));
        var offset = _rnd.Next(-delta, delta + 1);
        return Math.Max(0, ms + offset);
    }

    public Task SmallHumanDelayAsync()
    {
        var ms = _rnd.Next(200, 801);
        return Task.Delay(ms);
    }

    public Task MessageDelayAsync()
        => Task.Delay(WithJitter(_baseIntervalMs));

    public Task PartDelayAsync()
        => Task.Delay(WithJitter(_partIntervalMs));
}
