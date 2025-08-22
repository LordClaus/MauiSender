// MauiSender/ViewModels/AccountsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiSender.Models;
using MauiSender.Pages;
using MauiSender.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiSender.ViewModels;

public partial class AccountsViewModel : INotifyPropertyChanged
{
    private readonly AccountsService _accountsService;

    public ObservableCollection<Account> Accounts { get; } = new();

    private string _newLogin = string.Empty;
    public string NewLogin
    {
        get => _newLogin;
        set { _newLogin = value; OnPropertyChanged(); }
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set { _newPassword = value; OnPropertyChanged(); }
    }

    private Account? _selectedAccount;
    public Account? SelectedAccount
    {
        get => _selectedAccount;
        set { _selectedAccount = value; OnPropertyChanged(); }
    }

    public AccountsViewModel(AccountsService accountsService)
    {
        _accountsService = accountsService ?? throw new ArgumentNullException(nameof(accountsService));
    }

    public async Task InitializeAsync()
    {
        await _accountsService.EnsureAsync();
        Accounts.Clear();
        var list = await _accountsService.LoadAsync();
        foreach (var a in list) Accounts.Add(a);
    }

    public async Task AddAsync()
    {
        var login = (NewLogin ?? string.Empty).Trim();
        var pass = NewPassword ?? string.Empty;

        if (string.IsNullOrWhiteSpace(login))
            return;

        var acc = new Account { Login = login, Password = pass };
        await _accountsService.AddAsync(acc);
        Accounts.Add(acc);
        NewLogin = string.Empty;
        NewPassword = string.Empty;
    }

    public async Task RemoveSelectedAsync()
    {
        if (SelectedAccount == null) return;
        await _accountsService.RemoveByLoginAsync(SelectedAccount.Login);
        Accounts.Remove(SelectedAccount);
        SelectedAccount = null;
    }

    public async Task SaveAllAsync()
    {
        var list = Accounts.ToList();
        await _accountsService.SaveAsync(list);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? n = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
}