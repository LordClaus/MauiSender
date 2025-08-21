using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiSender.Services;

public class Navigator
{
    private readonly DomBridge _bridge;
    private readonly SelectorMap _sel;

    public Navigator(DomBridge bridge, SelectorMap sel)
    {
        _bridge = bridge;
        _sel = sel;
    }

    public async Task<string[]> GetRecipientsAsync()
        => await _bridge.QueryAllAsync(_sel["list.item"]);

    public async Task<bool> OpenChatAsync(string userId)
    {
        string script = @"(function(){
            const id = arguments[0];
            const items = Array.from(document.querySelectorAll(arguments[1]));
            const match = items.find(it => (it.getAttribute('data-user-id')||'')===id);
            if(!match) return false;
            match.click();
            return true;
        })()";
        var ok = await (_bridge as dynamic)._web.EvaluateJavaScriptAsync(script, userId, _sel["list.item"]);
        return ok == "true";
    }

    public async Task<bool> EnsureInputReadyAsync()
        => await _bridge.IsVisibleAsync(_sel["chat.input"]);
}
