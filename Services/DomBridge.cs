using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiSender.Services;

public class DomBridge
{
    private readonly WebView _web;
    public DomBridge(WebView web) => _web = web;

    // Виконання JS з аргументами
    private async Task<string?> EvalAsync(string script, params object[] args)
    {
        var serializedArgs = args.Select(a => JsonSerializer.Serialize(a));
        var argsJs = string.Join(",", serializedArgs);

        // загортаємо у функцію
        var finalScript = $"({script})({argsJs});";
        return await _web.EvaluateJavaScriptAsync(finalScript);
    }

    // Заповнити інпут текстом
    public async Task<string?> FillAsync(string selector, string text)
    {
        string script = @"
            function(sel, txt){
                const el = document.querySelector(sel);
                if(!el) return 'NO_INPUT';
                el.value = txt;
                el.dispatchEvent(new Event('input',{bubbles:true}));
                return 'OK';
            }";
        return await EvalAsync(script, selector, text);
    }

    // Натиснути кнопку
    public async Task<string?> ClickAsync(string selector)
    {
        string script = @"
            function(sel){
                const el = document.querySelector(sel);
                if(!el) return 'NO_BUTTON';
                el.click();
                return 'OK';
            }";
        return await EvalAsync(script, selector);
    }

    // Знайти всі елементи
    public async Task<string[]> QueryAllAsync(string selector)
    {
        string script = @"
            function(sel){
                const nodes = Array.from(document.querySelectorAll(sel));
                return JSON.stringify(nodes.map(n=>({ 
                    outerHtml: n.outerHTML, 
                    id: n.getAttribute('data-user-id') || n.id || n.getAttribute('data-id') || null 
                })));
            }";
        var json = await EvalAsync(script, selector);
        try
        {
            var items = JsonSerializer.Deserialize<List<DomNode>>(json ?? "[]") ?? new();
            return items.Select(i => i.id ?? "")
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();
        }
        catch { return Array.Empty<string>(); }
    }

    // Перевірити видимість елемента
    public async Task<bool> IsVisibleAsync(string selector)
    {
        string script = @"
            function(sel){
                const el = document.querySelector(sel);
                if(!el) return false;
                const s = getComputedStyle(el);
                return s.display!=='none' && s.visibility!=='hidden' && el.offsetParent !== null;
            }";
        var res = await EvalAsync(script, selector);
        return res == "true";
    }

    private record DomNode(string? id, string? outerHtml);
}