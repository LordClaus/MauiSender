using MauiSender.Models;

namespace MauiSender.Services;

public class Navigator
{
    private readonly DomBridge _dom;

    public Navigator(DomBridge dom)
    {
        _dom = dom;
    }

    public async Task<string[]> GetRecipientIdsAsync()
    {
        var s = _dom.Selectors;
        return await _dom.QueryAllIdsAsync(s.ListItem, s.ListItemIdAttr);
    }
}
