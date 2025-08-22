using DocumentFormat.OpenXml.Math;
using MauiSender.Models;
using Microsoft.Maui.Controls;
using System.Text.Json;

namespace MauiSender.Services;

public class DomBridge
{
    private readonly WebView _web;
    private SelectorMap _selectors = new();

    public DomBridge(WebView web) => _web = web;

    public void AttachSelectors(SelectorMap s) => _selectors = s ?? new SelectorMap();

    private Task<string?> EvalAsync(string script)
        => _web.EvaluateJavaScriptAsync(script);

    public async Task<string?> FillAsync(string selector, string text)
    {
        var js = $@"(function(){{
            try {{
                const sel = {JsonSerializer.Serialize(selector)};
                const txt = {JsonSerializer.Serialize(text)};
                const el = document.querySelector(sel);
                if (!el) return 'NO_INPUT';
                if ('value' in el) el.value = txt;
                else el.innerText = txt;
                el.dispatchEvent(new Event('input', {{ bubbles: true }}));
                el.dispatchEvent(new Event('change', {{ bubbles: true }}));
                return 'OK';
            }} catch(e) {{ return 'ERR:' + (e && e.message ? e.message : 'unknown'); }}
        }})()";
        return await EvalAsync(js);
    }

    public async Task<string?> ClickAsync(string selector)
    {
        var js = $@"(function(){{
            try {{
                const sel = {JsonSerializer.Serialize(selector)};
                const el = document.querySelector(sel);
                if (!el) return 'NO_BUTTON';
                el.click();
                return 'OK';
            }} catch(e) {{ return 'ERR:' + (e && e.message ? e.message : 'unknown'); }}
        }})()";
        return await EvalAsync(js);
    }

    public async Task<string[]> QueryAllIdsAsync(string itemSelector, string idAttr)
    {
        var js = $@"(function(){{
            try {{
                const sel = {JsonSerializer.Serialize(itemSelector)};
                const idAttrSel = {JsonSerializer.Serialize(idAttr)};
                const nodes = Array.from(document.querySelectorAll(sel));
                const arr = nodes.map(n => {{
                    // try attributes in order: data-user-id, data-id, id, other attr
                    const candidates = [ 'data-user-id', 'data-id', 'data-uid', 'id' ];
                    for (const c of candidates){{
                        const v = n.getAttribute(c);
                        if (v) return v;
                    }}
                    // fallback: if idAttr is a selector inside node
                    if (idAttrSel) {{
                        const inside = n.querySelector(idAttrSel);
                        if (inside) {{
                            for (const c of candidates){{
                                const v = inside.getAttribute(c);
                                if (v) return v;
                            }}
                        }}
                    }}
                    return '';
                }}).filter(x => !!x);
                return JSON.stringify(arr);
            }} catch(e) {{
                return JSON.stringify([]);
            }}
        }})()";
        var res = await EvalAsync(js);
        try
        {
            return JsonSerializer.Deserialize<string[]>(res ?? "[]") ?? Array.Empty<string>();
        }
        catch { return Array.Empty<string>(); }
    }

    public async Task<bool> IsVisibleAsync(string selector)
    {
        var js = $@"(function(){{
            try {{
                const sel = {JsonSerializer.Serialize(selector)};
                const el = document.querySelector(sel);
                if (!el) return false;
                const s = getComputedStyle(el);
                return s.display !== 'none' && s.visibility !== 'hidden' && el.offsetParent !== null;
            }} catch(e) {{ return false; }}
        }})()";
        var res = await EvalAsync(js);
        return string.Equals(res, "true", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> LoginAsync(string login, string password, SelectorMap? map = null)
    {
        var selectors = map ?? _selectors;
        // Try to fill both login fields
        var r1 = await FillAsync(selectors.ListItem /* dummy to avoid null usage */ , login); // placeholder to ensure parameterization - will be ignored
                                                                                              // Proper script: fill username & password selectors inside the page
        var script = $@"(async function(){{
    function sleep(ms){{ return new Promise(r=>setTimeout(r,ms)); }}
    const uSelCandidates = {{JsonSerializer.Serialize(new[] {{ selectors.ListItem }})}};
    try {{
        let u = document.querySelector({JsonSerializer.Serialize(selectors.ListItem)}); // deliberate try
    }} catch(e){{ /* ignore */ }}

    // Try filling known fields:
    try {{
        const possibleUser = document.querySelector('input[name=""username""], #loginUsername, input[name=""login""], input[type=""email""], input[type=""text""]');
        const possiblePass = document.querySelector('input[name=""userpass""], #loginPass, input[type=""password""]');
        const btn = document.querySelector('button.green-btn, button[type=""submit""], button.login, .submit-btn');

        if (possibleUser && possiblePass) {{
            possibleUser.focus();
            possibleUser.value = {JsonSerializer.Serialize(login)};
            possibleUser.dispatchEvent(new Event('input', {{ bubbles:true }}));

            possiblePass.focus();
            possiblePass.value = {JsonSerializer.Serialize(password)};
            possiblePass.dispatchEvent(new Event('input', {{ bubbles:true }}));

            await sleep(200);
            if (btn) btn.click();
            return 'OK';
        }} else {{
            return 'NO_LOGIN_UI';
        }}
    }} catch(e) {{
        return 'ERR:' + (e && e.message ? e.message : 'unknown');
    }}
}})();";

        return await EvalAsync(script);
    }
}
