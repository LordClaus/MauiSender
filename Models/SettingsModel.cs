namespace MauiSender.Models;

public class SettingsModel
{
    public int MessageIntervalSec { get; set; } = 60;
    public int PartIntervalSec { get; set; } = 60;
    public int JitterPct { get; set; } = 10;
    public string TemplateMode { get; set; } = "randomWeighted";
    public int MaxFailuresBeforePause { get; set; } = 3;
    public bool StopOnCaptcha { get; set; } = true;
    public string Language { get; set; } = "UA";
}
