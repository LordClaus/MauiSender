using System.Text;
using System.Text.Json;

namespace MauiSender.Services;

internal static class FileUtil
{
    public static string DataDir => FileSystem.AppDataDirectory;

    public static string PathInData(string fileName)
        => System.IO.Path.Combine(DataDir, fileName);

    public static async Task EnsureDirAsync()
    {
        if (!Directory.Exists(DataDir))
            Directory.CreateDirectory(DataDir);
        await Task.CompletedTask;
    }

    public static async Task EnsureJsonFileAsync<T>(string path, T defaultValue)
    {
        await EnsureDirAsync();
        if (!File.Exists(path))
        {
            var json = JsonSerializer.Serialize(defaultValue, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        }
    }

    public static async Task<T> LoadJsonAsync<T>(string path, T fallback)
    {
        await EnsureDirAsync();
        if (!File.Exists(path))
            return fallback;

        var json = await File.ReadAllTextAsync(path, Encoding.UTF8);
        try
        {
            var obj = JsonSerializer.Deserialize<T>(json);
            return obj is null ? fallback : obj;
        }
        catch
        {
            return fallback;
        }
    }

    public static async Task SaveJsonAsync<T>(string path, T value)
    {
        await EnsureDirAsync();
        var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
    }
}
