namespace MauiSender.Models;

public class LogEntry
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public string TemplateId { get; set; } = "";
    public int PartIndex { get; set; }
    public DateTime Ts { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "sent";
    public string? Error { get; set; }
}
