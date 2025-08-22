using MauiSender.Models;

namespace MauiSender.Services
{
    public sealed class CampaignRunner
    {
        private readonly DomBridge _dom;
        private readonly TemplatesRepo _templates;
        private readonly BlacklistRepo _blacklist;
        private readonly LogsRepo _logs;
        private readonly DelayPolicy _delay;
        private readonly Navigator _nav;
        private readonly SettingsService _settings;

        public TemplateMode Mode { get; set; } = TemplateMode.Random;

        // Селектори можна тримати тут або читати з SelectorMapService.Current.Map
        public string InputSelector { get; set; } = "#messageBox, textarea";
        public string SendButtonSelector { get; set; } = "button.send, button[type=submit]";

        public CampaignRunner(
            DomBridge dom,
            TemplatesRepo templates,
            BlacklistRepo blacklist,
            LogsRepo logs,
            DelayPolicy delay,
            Navigator nav,
            SettingsService settings)
        {
            _dom = dom;
            _templates = templates;
            _blacklist = blacklist;
            _logs = logs;
            _delay = delay;
            _nav = nav;
            _settings = settings;
        }

        public async Task RunAsync(CancellationToken token)
        {
            await _dom.InjectHelpersAsync();

            var list = _templates.All.ToList();
            if (list.Count == 0) return;

            var i = 0;
            while (!token.IsCancellationRequested)
            {
                var tpl = Mode == TemplateMode.RoundRobin
                    ? list[i++ % list.Count]
                    : list[Random.Shared.Next(list.Count)];

                // Розбити шаблон на частини за ---/---N---
                var parts = SplitTemplate(tpl, _delay.GetPartDelay());

                foreach (var p in parts)
                {
                    token.ThrowIfCancellationRequested();

                    // Вставити текст і (за потреби) натиснути кнопку
                    await _dom.FillAsync(InputSelector, p.Text);
                    // Якщо хочеш — клікай Send кожну частину або лише після останньої
                    // await _dom.ClickAsync(SendButtonSelector);

                    await Task.Delay(TimeSpan.FromSeconds(p.DelayAfterSec), token);
                }

                await _logs.AppendAsync(new LogsRepo.LogEntry
                {
                    Timestamp = DateTimeOffset.UtcNow,
                    Message = tpl,
                    Status = "OK",
                });

                await Task.Delay(TimeSpan.FromSeconds(_delay.GetMessageInterval()), token);
            }
        }

        private readonly record struct Part(string Text, int DelayAfterSec);

        private static List<Part> SplitTemplate(string raw, int defaultDelaySec)
        {
            // Підтримка: "Hello --- world", "Hello ---5--- world"
            var parts = new List<Part>();
            var sb = new System.Text.StringBuilder();
            int i = 0;
            while (i < raw.Length)
            {
                if (raw[i] == '-' && i + 2 < raw.Length && raw[i + 1] == '-' && raw[i + 2] == '-')
                {
                    // знайдено --- або ---N---
                    // закриваємо попередній текст як частину
                    var chunk = sb.ToString().Trim();
                    if (chunk.Length > 0) parts.Add(new Part(chunk, defaultDelaySec));
                    sb.Clear();

                    // перевіримо на число між двома блоками ---
                    i += 3;
                    // якщо далі йде число і потім знову --- (типу ---5---)
                    int j = i;
                    while (j < raw.Length && char.IsDigit(raw[j])) j++;
                    bool hasNumber = j > i && j + 2 < raw.Length && raw[j] == '-' && raw[j + 1] == '-' && raw[j + 2] == '-';

                    if (hasNumber)
                    {
                        var numStr = raw.Substring(i, j - i);
                        if (int.TryParse(numStr, out var n) && n > 0)
                            defaultDelaySec = n; // локально оновлюємо для наступного шматка

                        i = j + 3; // перескочили другі ---
                    }
                    continue;
                }
                sb.Append(raw[i]);
                i++;
            }
            var last = sb.ToString().Trim();
            if (last.Length > 0) parts.Add(new Part(last, defaultDelaySec));
            return parts;
        }
    }
}
