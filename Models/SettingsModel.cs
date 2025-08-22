namespace MauiSender.Models
{
    public sealed class SettingsModel
    {
        public int MessageIntervalSec { get; set; } = 30;  // базова пауза між повідомленнями
        public int PartDelaySec { get; set; } = 8;    // пауза між частинами в одному шаблоні (---)
        public TemplateMode Mode { get; set; } = TemplateMode.Random;

        public string BaseUrl { get; set; } = "https://goldenbride.net";
        public string Login { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
