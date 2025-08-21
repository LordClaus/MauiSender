using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSender.Services;

public class BlacklistRepo
{
    private readonly string _path;
    public BlacklistRepo()
    {
        _path = Path.Combine(FileSystem.AppDataDirectory, "blacklist.txt");
        if (!File.Exists(_path)) File.WriteAllText(_path, "");
    }

    public bool Contains(string id) => Load().Contains(id);

    public void ReplaceAll(IEnumerable<string> ids)
        => File.WriteAllLines(_path, ids.Distinct().OrderBy(x => x));

    private HashSet<string> Load()
        => File.ReadAllLines(_path).ToHashSet(StringComparer.OrdinalIgnoreCase);
}
