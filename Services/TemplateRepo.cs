using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiSender.Models;

namespace MauiSender.Services;

public class TemplateRepo
{
    private readonly SQLiteConnection _db;
    public TemplateRepo()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "gbsender.db3");
        _db = new SQLiteConnection(dbPath);
        _db.CreateTable<Template>();
    }

    public IEnumerable<Template> GetAllEnabled() => _db.Table<Template>().Where(t => t.Enabled);

    public void ReplaceAll(IEnumerable<Template> items)
    {
        _db.DeleteAll<Template>();
        _db.InsertAll(items);
    }

    public Template? GetByName(string name) => _db.Table<Template>().FirstOrDefault(t => t.Name == name);
    public Template? GetById(int id) => _db.Table<Template>().FirstOrDefault(t => t.Id == id);

    public Template Pick(string mode)
    {
        var all = _db.Table<Template>().Where(t => t.Enabled).ToList();
        if (all.Count == 0) return new Template { Name = "Порожньо", RawText = "Привіт!" };

        if (mode == "roundRobin")
        {
            var idx = Preferences.Default.Get("rr.idx", 0);
            var t = all[idx % all.Count];
            Preferences.Default.Set("rr.idx", (idx + 1) % all.Count);
            return t;
        }
        else
        {
            // weighted random
            var weighted = all.SelectMany(t => Enumerable.Repeat(t, Math.Max(1, t.Weight))).ToList();
            var rnd = Random.Shared.Next(weighted.Count);
            return weighted[rnd];
        }
    }
}
