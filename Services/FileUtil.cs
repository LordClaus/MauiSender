using System.Text;
using System.Text.Json;

namespace MauiSender.Services;

internal static class FileUtil
{
    public static string DataDir => FileSystem.AppDataDirectory;

    public static string PathInData(string fileName) =>
        Path.Combine(DataDir, fileName);

    public static void EnsureDir()
    {
        if (!Directory.Exists(DataDir))
            Directory.CreateDirectory(DataDir);
    }

    public static async Task EnsureJsonFileAsync<T>(string path, T defaultValue)
    {
        EnsureDir();
        if (!File.Exists(path))
        {
            var json = JsonSerializer.Serialize(defaultValue, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(path, json, Encoding.UTF8);
        }
    }

    public static async Task<T> LoadJsonAsync<T>(string path, T fallback)
    {
        EnsureDir();
        if (!File.Exists(path))
            return fallback;

        try
        {
            var json = await File.ReadAllTextAsync(path, Encoding.UTF8);
            var obj = JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return obj is null ? fallback : obj;
        }
        catch
        {
            // якщо файл пошкоджений — повернути fallback (не кидати)
            return fallback;
        }
    }

    public static async Task SaveJsonAsync<T>(string path, T obj)
    {
        EnsureDir();
        var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, json, Encoding.UTF8);
    }
}
