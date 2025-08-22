namespace MauiSender.Services;

public class ExportService
{
    private readonly LogsRepo _logs;

    public ExportService(LogsRepo logs)
    {
        _logs = logs;
    }

    public async Task<string> ExportLogsCsvAsync(DateTime from, DateTime to, string? filePath = null)
    {
        var path = filePath ?? FileUtil.PathInData($"logs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        return await _logs.ExportCsvAsync(from, to, path);
    }
}
