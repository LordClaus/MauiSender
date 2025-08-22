using System.Text;
using System.Text.Json;

namespace MauiSender.Services
{
    public sealed class LogsRepo
    {
        private readonly string _path;
        private readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public LogsRepo(string path)
        {
            _path = path;
            if (!File.Exists(_path)) File.WriteAllText(_path, "[]");
        }

        public sealed class LogEntry
        {
            public DateTimeOffset Timestamp { get; set; }
            public string Message { get; set; } = "";
            public string Status { get; set; } = "";
            public string? RecipientId { get; set; }
            public string? Error { get; set; }
        }

        public async Task AppendAsync(LogEntry entry)
        {
            var list = await LoadAsync();
            list.Add(entry);
            await SaveAsync(list);
        }

        public async Task<List<LogEntry>> LoadAsync()
        {
            try
            {
                var raw = await File.ReadAllTextAsync(_path);
                return JsonSerializer.Deserialize<List<LogEntry>>(raw) ?? new();
            }
            catch { return new(); }
        }

        private Task SaveAsync(List<LogEntry> list)
            => File.WriteAllTextAsync(_path, JsonSerializer.Serialize(list, _opts));

        public async Task ExportCsvAsync(DateTime from, DateTime to, string filePath)
        {
            var all = await LoadAsync();
            var filtered = all.Where(x => x.Timestamp.UtcDateTime >= from.ToUniversalTime()
                                       && x.Timestamp.UtcDateTime <= to.ToUniversalTime());
            var sb = new StringBuilder();
            sb.AppendLine("Timestamp,RecipientId,Status,Message,Error");
            foreach (var x in filtered)
                sb.AppendLine($"{x.Timestamp:o},{x.RecipientId},{x.Status},\"{x.Message.Replace("\"", "\"\"")}\",{x.Error}");
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
