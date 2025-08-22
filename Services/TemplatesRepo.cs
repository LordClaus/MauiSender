using MauiSender.Models;

namespace MauiSender.Services;

public class TemplatesRepo
{
    private readonly string _path = FileUtil.PathInData("templates.json");

    public async Task EnsureAsync()
    {
        var defaultList = new List<Template>
        {
            new Template { Name = "Привітання", RawText = "Привіт! --- Як справи?", Weight = 1, Enabled = true }
        };
        defaultList.ForEach(t => t.ParseParts());
        await FileUtil.EnsureJsonFileAsync(_path, defaultList);
    }

    public async Task<List<Template>> LoadAsync()
    {
        var list = await FileUtil.LoadJsonAsync(_path, new List<Template>());
        foreach (var t in list) if (t.Parts.Count == 0) t.ParseParts();
        return list;
    }

    public async Task SaveAsync(List<Template> templates)
    {
        templates.ForEach(t => t.ParseParts());
        await FileUtil.SaveJsonAsync(_path, templates);
    }
}
