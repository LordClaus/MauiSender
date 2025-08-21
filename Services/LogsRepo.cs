using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiSender.Models;

namespace MauiSender.Services;

public class LogsRepo
{
    private readonly SQLiteConnection _db;
    public LogsRepo()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gbsender.db3");
        _db = new SQLiteConnection(dbPath);
        _db.CreateTable<LogEntry>();
    }

    public void Add(LogEntry e) => _db.Insert(e);

    public async Task<string> ExportCsvAsync(string? fileName = null)
    {
        fileName ??= $"logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserId,TemplateId,PartIndex,Timestamp,Status,Error");
        foreach (var row in _db.Table<LogEntry>().OrderBy(x => x.Id))
        {
            sb.AppendLine($"{row.Id},{row.UserId},{row.TemplateId},{row.PartIndex},{row.Timestamp:o},{row.Status},\"{row.Error?.Replace('\"', '\'')}\"");
        }
        await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8);
        return path;
    }
}
