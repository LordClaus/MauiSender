using MauiSender.Models;

namespace MauiSender.Services;

public class LogsRepo
{
    private readonly string _path = FileUtil.PathInData("logs.jsonl"); // JSON Lines

    public async Task EnsureAsync()
    {
        await FileUtil.EnsureDirAsync();
        if (!File.Exists(_path))
        {
            using var _ = File.Create(_path);
        }
        await Task.CompletedTask;
    }

    public async Task AppendAsync(LogEntry entry)
    {
        await EnsureAsync();
        var line = System.Text.Json.JsonSerializer.Serialize(entry);
        await File.AppendAllTextAsync(_path, line + Environment.NewLine);
    }
}
