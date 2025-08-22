// MauiSender/Services/LogsRepo.cs
using SQLite;
using MauiSender.Models;

namespace MauiSender.Services;

public class LogsRepo
{
    private readonly SQLiteAsyncConnection _db;

    public LogsRepo(FileSystemService fs)
    {
        var path = fs.PathFor("logs.db3");
        _db = new SQLiteAsyncConnection(path);
        _db.CreateTableAsync<LogEntry>().Wait();
    }

    public Task<int> AddAsync(LogEntry entry) => _db.InsertAsync(entry);

    public Task<List<LogEntry>> GetByDateAsync(DateTime from, DateTime to) =>
        _db.Table<LogEntry>()
           .Where(l => l.Ts >= from && l.Ts <= to)
           .ToListAsync();
}
