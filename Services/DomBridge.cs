// MauiSender/Services/DomBridge.cs
using System.Text.Json;

namespace MauiSender.Services;

public class DomBridge
{
    private readonly WebView _webView;

    public DomBridge(WebView webView)
    {
        _webView = webView;
    }

    public async Task FillAsync(string selector, string text)
    {
        string script = $"document.querySelector('{selector}').value = '{text}';";
        await _webView.EvaluateJavaScriptAsync(script);
    }

    public async Task ClickAsync(string selector)
    {
        string script = $"document.querySelector('{selector}').click();";
        await _webView.EvaluateJavaScriptAsync(script);
    }

    public async Task LoginAsync(string login, string password)
    {
        await FillAsync("input[name='login'], #login", login);
        await FillAsync("input[name='password'], #password", password);
        await ClickAsync("button[type='submit'], .submit-btn");
    }

    private static string JsEscape(string s)
        => s.Replace(@"\", @"\\").Replace("'", @"\'").Replace("\r", "").Replace("\n", "\\n");

    private async Task<string?> EvalAsync(string script)
    {
        return await MainThread.InvokeOnMainThreadAsync(() => _web.EvaluateJavaScriptAsync(script));
    }

    public async Task<string?> FillAsync(string selector, string text)
    {
        var sel = JsEscape(selector);
        var txt = JsEscape(text);
        string script = $@"(function(){{
            const el = document.querySelector('{sel}');
            if(!el) return 'NO_INPUT';
            if('value' in el) el.value='{txt}';
            el.dispatchEvent(new Event('input',{{bubbles:true}}));
            el.dispatchEvent(new Event('change',{{bubbles:true}}));
            return 'OK';
        }})()";
        return await EvalAsync(script);
    }

    public async Task<string?> ClickAsync(string selector)
    {
        var sel = JsEscape(selector);
        string script = $@"(function(){{
            const el = document.querySelector('{sel}');
            if(!el) return 'NO_BUTTON';
            el.click();
            return 'OK';
        }})()";
        return await EvalAsync(script);
    }

    public async Task<string[]> QueryAllIdsAsync(string itemSelector)
    {
        var sel = JsEscape(itemSelector);
        string script = $@"(function(){{
            const nodes = Array.from(document.querySelectorAll('{sel}'));
            return JSON.stringify(nodes.map(n=> n.getAttribute('data-user-id') || n.id || n.getAttribute('data-id') || ''));
        }})()";
        var json = await EvalAsync(script);
        try
        {
            var arr = JsonSerializer.Deserialize<string[]>(json ?? "[]") ?? Array.Empty<string>();
            return arr.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToArray();
        }
        catch { return Array.Empty<string>(); }
    }

    public async Task<bool> IsVisibleAsync(string selector)
    {
        var sel = JsEscape(selector);
        string script = $@"(function(){{
            const el = document.querySelector('{sel}');
            if(!el) return false;
            const s = getComputedStyle(el);
            return s.display!=='none' && s.visibility!=='hidden' && el.offsetParent !== null;
        }})()";
        var res = await EvalAsync(script);
        return res == "true";
    }



    public async Task AutoLoginAsync(string login, string password)
    {
        var ls = JsEscape(login);
        var ps = JsEscape(password);

        string script = $@"(async function(){{
            function findInput(name){{
                let el = document.querySelector('input[name=""'+name+'""], #' + name + ', input#'+name);
                if(el) return el;
                const labels = Array.from(document.querySelectorAll('label'));
                for(const l of labels){{
                    const forId = l.getAttribute('for');
                    if(forId) {{
                        const e2 = document.getElementById(forId);
                        if(e2) return e2;
                    }}
                }}
                return null;
            }}

            function sleep(ms){{ return new Promise(r=>setTimeout(r,ms)); }}

            for(let i=0;i<40;i++){{ // ~20s
                const u = document.querySelector('input[name=""username""]') || document.querySelector('#loginUsername') || findInput('username');
                const p = document.querySelector('input[name=""userpass""]') || document.querySelector('#loginPass') || findInput('userpass');
                const btn = document.querySelector('button.green-btn') || document.querySelector('button[type=""submit""]');
                if(u && p && btn){{
                    u.focus(); u.value='{ls}'; u.dispatchEvent(new Event('input',{{bubbles:true}}));
                    p.focus(); p.value='{ps}'; p.dispatchEvent(new Event('input',{{bubbles:true}}));
                    await sleep(300);
                    btn.click();
                    return 'OK';
                }}
                await sleep(500);
            }}
            return 'NO_LOGIN_UI';
        }})()";

        await EvalAsync(script);
    }
}
