using MauiSender.Services;
using MauiSender.ViewModels;

namespace MauiSender.Pages;

public partial class ChatPage : ContentPage
{
    private DomBridge? _dom;

    public ChatPage(ChatViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        vm.AttachWebView(ChatWeb);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dom ??= new DomBridge(ChatWeb);
    }

    private async void OnChatNavigated(object sender, WebNavigatedEventArgs e)
    {
        var vm = (ChatViewModel)BindingContext;
        await vm.OnChatNavigatedAsync();
    }
}
