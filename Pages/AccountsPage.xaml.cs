using System.Collections.ObjectModel;
using MauiSender.Models;
using MauiSender.Services;

namespace MauiSender.Pages;

public partial class AccountsPage : ContentPage
{
    private readonly AccountService _accountService = new();
    private readonly ObservableCollection<UserAccount> _accounts;
    private DomBridge? _dom;

    private const string ChatUrl = "https://goldenbride.net/chat#!CHAT;";

    public AccountsPage()
    {
        InitializeComponent();
        _accounts = new ObservableCollection<UserAccount>(_accountService.GetAll());
        AccountsPicker.ItemsSource = _accounts;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _dom ??= new DomBridge(ChatWeb);
    }

    // ƒодати акаунт
    private void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LoginEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            return;

        var acc = new UserAccount { Login = LoginEntry.Text.Trim(), Password = PasswordEntry.Text };
        _accountService.Add(acc);
        _accounts.Add(acc);

        LoginEntry.Text = string.Empty;
        PasswordEntry.Text = string.Empty;
    }

    // ¬идалити вибраний акаунт
    private void OnDeleteClicked(object sender, EventArgs e)
    {
        if (AccountsPicker.SelectedItem is UserAccount acc)
        {
            _accountService.Remove(acc);
            _accounts.Remove(acc);
        }
    }

    // як т≥льки чатова стор≥нка завантажилась Ч €кщо обрано акаунт, намагаЇмось автолог≥н
    private async void OnChatNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success) return;
        if (_dom is null) return;
        if (AccountsPicker.SelectedItem is not UserAccount acc) return;

        // ѕ≥сл€ кожного нав≥гац≥йного завершенн€ пробуЇмо залог≥нитись,
        // бо чат п≥двантажуЇ попап асинхронно.
        try
        {
            var ok = await TryLoginAsync(acc, TimeSpan.FromSeconds(20));
            if (ok)
            {
                // ”сп≥х Ч в≥дкриваЇмо стор≥нку з нашою УменюшкоюФ над WebView
                await Navigation.PushAsync(new SenderPage(acc));
            }
            // €кщо н≥ Ч юзер може зм≥нити акаунт або спробувати ще раз (просто ще раз вибрати акаунт)
        }
        catch
        {
            // ≥гноруЇмо Утих≥Ф помилки лог≥ну Ч UI не падаЇ
        }
    }

    // ƒодатково: €кщо користувач зм≥нив виб≥р акаунта Ч перевантажимо стор≥нку ≥ спробуЇмо лог≥н
    protected void OnCurrentSelectionChanged(object sender, EventArgs e)
    {
        // не обов'€зково, але зручно
        ChatWeb.Source = ChatWeb.Source;
    }

    // === јвтолог≥н у попап≥ чату ===
    private async Task<bool> TryLoginAsync(UserAccount acc, TimeSpan timeout)
    {
        if (_dom is null) return false;

        // ќч≥куЇмо по€ву ≥нпут≥в лог≥ну-парол€ (попап)
        var start = DateTime.UtcNow;
        while ((DateTime.UtcNow - start) < timeout)
        {
            bool userField = await _dom.IsVisibleAsync("input[name='username'], #loginUsername");
            bool passField = await _dom.IsVisibleAsync("input[name='userpass'], #loginPass");
            bool button = await _dom.IsVisibleAsync(".account-pop-up.log-in button.green-btn, button.green-btn");

            if (userField && passField && button)
                break;

            await Task.Delay(500);
        }

        // спроба заповнити й натиснути
        await _dom.FillAsync("input[name='username'], #loginUsername", acc.Login);
        await _dom.FillAsync("input[name='userpass'], #loginPass", acc.Password);
        await Task.Delay(150); // невелика УлюдськаФ пауза
        await _dom.ClickAsync(".account-pop-up.log-in button.green-btn, button.green-btn");

        // „екаЇмо зникненн€ попапа = усп≥шний вх≥д
        var waitOkStart = DateTime.UtcNow;
        while ((DateTime.UtcNow - waitOkStart) < TimeSpan.FromSeconds(15))
        {
            bool popupVisible = await _dom.IsVisibleAsync(".account-pop-up.log-in");
            if (!popupVisible)
                return true; // попап зник Ч вважаЇмо, що залог≥нились

            await Task.Delay(500);
        }

        return false;
    }
}
