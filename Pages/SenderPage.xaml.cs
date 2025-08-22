// File: Pages/SenderPage.xaml.cs
using MauiSender.Services;
using MauiSender.ViewModels;

namespace MauiSender.Pages;

public partial class SenderPage : ContentPage
{
    private DomBridge? _dom;

    public SenderPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dom ??= new DomBridge(ChatWeb);
        if (BindingContext is SenderViewModel vm)
        {
            vm.AttachDom(_dom, () => Dispatcher);
        }
    }

    private void OnChatNavigated(object? sender, WebNavigatedEventArgs e)
    {
        // no-op
    }
}
