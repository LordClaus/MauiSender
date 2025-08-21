using MauiSender.Services;
using MauiSender.ViewModels;

namespace MauiSender;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void WebView_Navigated(object sender, WebNavigatedEventArgs e)
    {
        if (BindingContext is MainViewModel vm)
        {
            vm.AttachWebView(GoldenWeb);
        }
    }
}

