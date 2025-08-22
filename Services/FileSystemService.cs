// MauiSender/Services/FileSystemService.cs
namespace MauiSender.Services;

public class FileSystemService
{
    public string AppDir =>
        FileSystem.Current.AppDataDirectory;

    public string PathFor(string filename) =>
        System.IO.Path.Combine(AppDir, filename);

    public async Task EnsureExistsAsync(string filename, string defaultContent = "[]")
    {
        var path = PathFor(filename);
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(AppDir);
            await File.WriteAllTextAsync(path, defaultContent);
        }
    }
}
