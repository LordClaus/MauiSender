// MauiSender/ViewModels/AccountsViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSender.Models;
using MauiSender.Pages;
using MauiSender.Services;

namespace MauiSender.ViewModels;

public partial class AccountsViewModel : ObservableObject
{
    private readonly AccountsService _service;

    [ObservableProperty] private ObservableCollection<Account> accounts = new();
    [ObservableProperty] private string newLogin = "";
    [ObservableProperty] private string newPassword = "";
    [ObservableProperty] private Account? selectedAccount;

    public AccountsViewModel(AccountsService service)
    {
        _service = service;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        Accounts = await _service.LoadAsync();
    }

    [RelayCommand]
    private async Task AddAccount()
    {
        if (string.IsNullOrWhiteSpace(NewLogin) || string.IsNullOrWhiteSpace(NewPassword)) return;
        Accounts.Add(new Account { Login = NewLogin.Trim(), Password = NewPassword });
        NewLogin = "";
        NewPassword = "";
        await _service.SaveAsync(Accounts);
    }

    [RelayCommand]
    private async Task RemoveAccount(Account acc)
    {
        Accounts.Remove(acc);
        await _service.SaveAsync(Accounts);
    }

    [RelayCommand]
    private async Task Save()
    {
        await _service.SaveAsync(Accounts);
    }

    [RelayCommand]
    private async Task OpenChat(Account acc)
    {
        var page = Application.Current!.Services.GetRequiredService<ChatPage>();
        await Application.Current!.MainPage!.Navigation.PushAsync(page);
        var vm = (ChatViewModel)page.BindingContext;
        vm.SetCurrentAccount(acc);
        await vm.EnsureInitAsync();
    }
}
