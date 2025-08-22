// MauiSender/Services/ExportService.cs
using CsvHelper;
using System.Globalization;
using MauiSender.Models;

namespace MauiSender.Services;

public class ExportService
{
    private readonly LogsRepo _logs;

    public ExportService(LogsRepo logs) => _logs = logs;

    public async Task<string> ExportCsvAsync(DateTime from, DateTime to, string filePath)
    {
        var list = await _logs.GetByDateAsync(from, to);
        using var writer = new StreamWriter(filePath, false);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(list);
        return filePath;
    }

    public async Task<string> ExportXlsxAsync(DateTime from, DateTime to, string filePath)
    {
        var list = await _logs.GetByDateAsync(from, to);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Logs");
        ws.Cell(1, 1).Value = "Id";
        ws.Cell(1, 2).Value = "UserId";
        ws.Cell(1, 3).Value = "TemplateId";
        ws.Cell(1, 4).Value = "PartIndex";
        ws.Cell(1, 5).Value = "Ts";
        ws.Cell(1, 6).Value = "Status";
        ws.Cell(1, 7).Value = "Error";
        int r = 2;
        foreach (var l in list)
        {
            ws.Cell(r, 1).Value = l.Id;
            ws.Cell(r, 2).Value = l.UserId;
            ws.Cell(r, 3).Value = l.TemplateId;
            ws.Cell(r, 4).Value = l.PartIndex;
            ws.Cell(r, 5).Value = l.Ts;
            ws.Cell(r, 6).Value = l.Status;
            ws.Cell(r, 7).Value = l.Error ?? "";
            r++;
        }
        wb.SaveAs(filePath);
        await Task.CompletedTask;
        return filePath;
    }
}
