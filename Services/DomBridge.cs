using System.Text.Json;
using MauiSender.Models;

namespace MauiSender.Services;

public class DomBridge
{
    private readonly WebView _web;
    private SelectorMap _selectors = new();

    public DomBridge(WebView web) => _web = web;

    public void AttachSelectors(SelectorMap map) => _selectors = map ?? new();

    private async Task<string?> EvalAsync(string script)
        => await _web.EvaluateJavaScriptAsync(script);

    public async Task<string?> FillAsync(string selector, string text)
    {
        string script = $@"(function() {{
            const el = document.querySelector({JsonSerializer.Serialize(selector)});
            if(!el) return 'NO_INPUT';
            el.value = {JsonSerializer.Serialize(text)};
            el.dispatchEvent(new Event('input', {{bubbles:true}}));
            return 'OK';
        }})()";
        return await EvalAsync(script);
    }

    public async Task<string?> ClickAsync(string selector)
    {
        string script = $@"(function() {{
            const el = document.querySelector({JsonSerializer.Serialize(selector)});
            if(!el) return 'NO_BUTTON';
            el.click();
            return 'OK';
        }})()";
        return await EvalAsync(script);
    }

    public async Task<string[]> QueryAllIdsAsync(string listSelector, string idAttrSelector)
    {
        string script = $@"(function(){{
            const nodes = Array.from(document.querySelectorAll({JsonSerializer.Serialize(listSelector)}));
            const res = nodes.map(n => {{
                let id = n.getAttribute('data-user-id') || n.id || n.getAttribute('data-id');
                if(!id && {(!string.IsNullOrWhiteSpace(idAttrSelector)).ToString().ToLower()}) {{
                    const idNode = n.querySelector({JsonSerializer.Serialize(idAttrSelector)});
                    if(idNode) {{
                        id = idNode.getAttribute('data-user-id') || idNode.id || idNode.getAttribute('data-id');
                    }}
                }}
                return id || '';
            }}).filter(Boolean);
            return JSON.stringify(res);
        }})()";
        var json = await EvalAsync(script);
        try
        {
            return JsonSerializer.Deserialize<string[]>(json ?? "[]") ?? Array.Empty<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public SelectorMap Selectors => _selectors;
}
