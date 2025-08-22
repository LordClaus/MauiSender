using MauiSender.Models;

namespace MauiSender.Services
{
    public class CampaignRunner
    {
        private DomBridge _dom;

        public void AttachDom(DomBridge dom)
        {
            _dom = dom;
        }

        public async Task StartCampaignAsync(List<Account> accounts, string template)
        {
            if (_dom == null) return;

            foreach (var acc in accounts)
            {
                await _dom.LoginAsync(acc.Username, acc.Password);
                await _dom.FillAsync("textarea[name='message'], #messageBox", template);
                await _dom.ClickAsync("button.send, .send-btn");
                await Task.Delay(2000);
            }
        }
    }
}
