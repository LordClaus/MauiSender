using MauiSender.ViewModels;

namespace MauiSender.Pages;

public partial class AccountsPage : ContentPage
{
    public AccountsPage(AccountsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
