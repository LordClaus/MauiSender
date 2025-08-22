// MauiSender/Services/DelayPolicy.cs
namespace MauiSender.Services;

public class DelayPolicy
{
    public int BaseMessageSec { get; set; }
    public int PartSec { get; set; }
    public int JitterPct { get; set; }

    public DelayPolicy(int baseMessageSec, int partSec, int jitterPct)
    {
        BaseMessageSec = Math.Max(1, baseMessageSec);
        PartSec = Math.Max(1, partSec);
        JitterPct = Math.Clamp(jitterPct, 0, 30);
    }

    private int ApplyJitter(int ms)
    {
        var pct = JitterPct / 100.0;
        var delta = (int)(ms * pct);
        var jitter = Random.Shared.Next(-delta, delta + 1);
        return Math.Max(0, ms + jitter);
    }

    public Task PartDelayAsync() =>
        Task.Delay(ApplyJitter(PartSec * 1000));

    public Task MessageDelayAsync() =>
        Task.Delay(ApplyJitter(BaseMessageSec * 1000));

    public static async Task HumanDelayAsync(int minMs = 200, int maxMs = 800) =>
        await Task.Delay(Random.Shared.Next(minMs, maxMs + 1));
}
