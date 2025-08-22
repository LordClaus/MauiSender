// MauiSender/Services/Navigator.cs
using MauiSender.Models;

namespace MauiSender.Services;

public class Navigator
{
    private readonly DomBridge _dom;
    private readonly SelectorMap _map;
    private readonly HashSet<string> _visited = new();
    private string[] _cache = Array.Empty<string>();
    private int _index = -1;

    public Navigator(DomBridge dom, SelectorMap map)
    {
        _dom = dom;
        _map = map;
    }

    public async Task<string?> NextAsync()
    {
        if (_cache.Length == 0 || _index >= _cache.Length - 1)
        {
            _cache = await _dom.QueryAllIdsAsync(_map.ListItem);
            _index = -1;
        }
        if (_cache.Length == 0) return null;

        for (int i = 0; i < _cache.Length; i++)
        {
            _index++;
            if (_index >= _cache.Length) return null;
            var id = _cache[_index];
            if (string.IsNullOrWhiteSpace(id)) continue;
            if (_visited.Contains(id)) continue;
            _visited.Add(id);
            return id;
        }
        return null;
    }

    public async Task<bool> OpenChatForAsync(string id)
    {
        var sel = $"{_map.ListItem}[data-user-id='{id}'], {_map.ListItem}[data-id='{id}'], {_map.ListItem}#{id}";
        var res = await _dom.ClickAsync(sel);
        await Task.Delay(400);
        return res == "OK";
    }
}
