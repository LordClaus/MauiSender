using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using MauiSender.Services;
using MauiSender.ViewModels;
using MauiSender.Pages;

namespace MauiSender;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>();

        builder.Services.AddSingleton<FileSystemService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<AccountsService>();
        builder.Services.AddSingleton<TemplatesRepo>();
        builder.Services.AddSingleton<BlacklistRepo>();
        builder.Services.AddSingleton<LogsRepo>();
        builder.Services.AddSingleton<ExportService>();
        builder.Services.AddSingleton<SelectorMapService>();

        builder.Services.AddTransient<AccountsViewModel>();
        builder.Services.AddTransient<ChatViewModel>();

        builder.Services.AddTransient<AccountsPage>();
        builder.Services.AddTransient<ChatPage>();

        return builder.Build();
    }
}
