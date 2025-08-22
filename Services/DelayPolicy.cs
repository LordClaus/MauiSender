namespace MauiSender.Services;

public class DelayPolicy
{
    private readonly Random _rnd = new();
    public int BaseMessageSec { get; private set; }
    public int PartSec { get; private set; }
    public int JitterPct { get; private set; }

    public DelayPolicy(int baseMessageSec = 45, int partSec = 45, int jitterPct = 10)
    {
        BaseMessageSec = Math.Max(1, baseMessageSec);
        PartSec = Math.Max(1, partSec);
        JitterPct = Math.Clamp(jitterPct, 0, 30);
    }

    private int ApplyJitterMs(int ms)
    {
        if (JitterPct <= 0) return ms;
        var delta = (int)(ms * (JitterPct / 100.0));
        return Math.Max(0, ms + _rnd.Next(-delta, delta + 1));
    }

    public Task PartDelayAsync() => Task.Delay(ApplyJitterMs(PartSec * 1000));
    public Task MessageDelayAsync() => Task.Delay(ApplyJitterMs(BaseMessageSec * 1000));
    public Task HumanSmallAsync() => Task.Delay(_rnd.Next(200, 801));
}
