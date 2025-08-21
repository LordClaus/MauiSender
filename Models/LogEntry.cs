using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSender.Models;

public class LogEntry
{
    [SQLite.PrimaryKey, SQLite.AutoIncrement]
    public int Id { get; set; }

    public string UserId { get; set; } = "";
    public int? TemplateId { get; set; }
    public int PartIndex { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "SENT"; // SENT / FAILED
    public string? Error { get; set; }
}
