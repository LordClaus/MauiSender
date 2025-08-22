namespace MauiSender.Services
{
    public sealed class DelayPolicy
    {
        public int MessageIntervalSec { get; set; } = 30;
        public int PartDelaySec { get; set; } = 8;

        // За бажанням — динамічні провайдери:
        public Func<int>? MessageIntervalProvider { get; set; }
        public Func<int>? PartDelayProvider { get; set; }

        public int GetMessageInterval() => MessageIntervalProvider?.Invoke() ?? MessageIntervalSec;
        public int GetPartDelay() => PartDelayProvider?.Invoke() ?? PartDelaySec;
    }
}
