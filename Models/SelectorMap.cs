namespace MauiSender.Models
{
    public sealed class SelectorMap
    {
        public string InputSelector { get; set; } = "#messageBox, textarea[name='message'], textarea";
        public string SendButtonSelector { get; set; } = "button.send, button[type=submit], .send-btn";
        public string RecipientsListSelector { get; set; } = ".users-list .user";

        public static SelectorMap LoadFromEmbedded(string? _ = null)
            => new SelectorMap(); // дефолтні селектори
    }
}
