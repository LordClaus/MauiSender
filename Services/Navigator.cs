using MauiSender.Models;

namespace MauiSender.Services;

public class Navigator
{
    private readonly DomBridge _dom;
    private readonly SelectorMap _map;

    public Navigator(DomBridge dom, SelectorMap map)
    {
        _dom = dom ?? throw new ArgumentNullException(nameof(dom));
        _map = map ?? throw new ArgumentNullException(nameof(map));
    }

    public async Task<string[]> GetRecipientIdsAsync()
    {
        return await _dom.QueryAllIdsAsync(_map.ListItem, _map.ListItemIdAttr);
    }

    public async Task<bool> OpenChatForAsync(string id)
    {
        // try to click an element that contains data-user-id == id
        var sel = $"{_map.ListItem}[data-user-id='{id}'], {_map.ListItem}[data-id='{id}'], {_map.ListItem}#{id}";
        var res = await _dom.ClickAsync(sel);
        return string.Equals(res, "OK", StringComparison.OrdinalIgnoreCase);
    }
}
