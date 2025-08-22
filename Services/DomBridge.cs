namespace MauiSender.Services
{
    public sealed class DomBridge
    {
        private readonly WebView _web;

        public DomBridge(WebView web) => _web = web ?? throw new ArgumentNullException(nameof(web));

        public Task<string?> EvaluateAsync(string js)
            => _web.EvaluateJavaScriptAsync(js);

        public async Task InjectHelpersAsync()
        {
            const string helpers = @"
                window.__sender = window.__sender || {};
                window.__sender.setValue = (sel, val) => {
                  const el = document.querySelector(sel);
                  if(!el) return 'NO_INPUT';
                  el.value = val;
                  el.dispatchEvent(new Event('input', {bubbles:true}));
                  return 'OK';
                };
                window.__sender.click = (sel) => {
                  const el = document.querySelector(sel);
                  if(!el) return 'NO_BTN';
                  el.click();
                  return 'OK';
                };
            ";
            await EvaluateAsync(helpers);
        }

        public Task<string?> FillAsync(string selector, string text)
            => EvaluateAsync($"window.__sender ? window.__sender.setValue({Q(selector)},{Q(text)}) : 'NO_HELPERS'");

        public Task<string?> ClickAsync(string selector)
            => EvaluateAsync($"window.__sender ? window.__sender.click({Q(selector)}) : 'NO_HELPERS'");

        public async Task AutoLoginAsync(string loginSelector, string passSelector, string buttonSelector,
                                         string login, string password)
        {
            await InjectHelpersAsync();
            await FillAsync(loginSelector, login);
            await FillAsync(passSelector, password);
            await ClickAsync(buttonSelector);
        }

        private static string Q(string s) => $"\"{s.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
    }
}
