using MauiSender.Models;
using MauiSender.Services;

namespace MauiSender.Pages;

public partial class SenderPage : ContentPage
{
    private readonly UserAccount _account;
    private DomBridge? _dom;

    public SenderPage(UserAccount account)
    {
        InitializeComponent();
        _account = account;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dom ??= new DomBridge(ChatWeb);
        // Тут можеш підвантажити шаблони в TemplatePicker та ін.
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        // Тут викликається CampaignRunner, який використовує DomBridge для відправки
        // Приклад: await _dom.FillAsync("#messageBox, textarea[name='message']", "Привіт!");
        //          await _dom.ClickAsync("button.send, .send-btn");
    }
}
