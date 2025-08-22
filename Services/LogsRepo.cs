using MauiSender.Models;
using System.Text;
using System.Globalization;

namespace MauiSender.Services;

public class LogsRepo
{
    private readonly string _path = FileUtil.PathInData("logs.json");

    public async Task EnsureAsync()
        => await FileUtil.EnsureJsonFileAsync(_path, new List<LogEntry>());

    public async Task<List<LogEntry>> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new List<LogEntry>());

    public async Task AppendAsync(LogEntry e)
    {
        var list = await LoadAsync();
        list.Add(e);
        await FileUtil.SaveJsonAsync(_path, list);
    }

    public async Task<string> ExportCsvAsync(DateTime from, DateTime to, string filePath)
    {
        var list = await LoadAsync();
        var filtered = list.Where(l => l.Ts >= from && l.Ts <= to).ToList();
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserId,TemplateId,PartIndex,Ts,Status,Error");
        foreach (var l in filtered)
        {
            var err = l.Error?.Replace("\"", "\"\"") ?? "";
            sb.AppendLine($"\"{l.Id}\",\"{l.UserId}\",\"{l.TemplateId}\",{l.PartIndex},\"{l.Ts:o}\",\"{l.Status}\",\"{err}\"");
        }
        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }
}
