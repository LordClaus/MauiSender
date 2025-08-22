using MauiSender.Models;

namespace MauiSender.Services;

public class TemplatesRepo
{
    private readonly string _path = FileUtil.PathInData("templates.json");

    public async Task EnsureAsync()
    {
        var def = new List<Template>
        {
            new Template
            {
                Name = "Привітання",
                RawText = "Привіт! --- Як справи?",
                Weight = 1
            }
        };
        def.ForEach(t => t.ParseParts());
        await FileUtil.EnsureJsonFileAsync(_path, def);
    }

    public async Task<List<Template>> LoadAsync()
        => await FileUtil.LoadJsonAsync(_path, new List<Template>());

    public async Task SaveAsync(List<Template> list)
        => await FileUtil.SaveJsonAsync(_path, list);
}
