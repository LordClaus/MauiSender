namespace MauiSender.Services
{
    public sealed class Navigator
    {
        private readonly WebView _web;

        public Navigator(WebView web) => _web = web ?? throw new ArgumentNullException(nameof(web));

        public Task GoAsync(string url)
        {
            _web.Source = url;
            return Task.CompletedTask;
        }
    }
}
