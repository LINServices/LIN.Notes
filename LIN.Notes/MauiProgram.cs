#if ANDROID
using Android.Views;

#endif
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace LIN.Notes;

public static class MauiProgram
{

    /// <summary>
    /// Nueva app Maui.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {

        // Constructor.
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Servicio blazor.
        builder.Services.AddMauiBlazorWebView();
        
        // Debug mode.
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Establecer app.
        LIN.Access.Auth.Build.SetAuth("DEFAULT");

        // Servicios de acceso.
        LIN.Access.Auth.Build.Init();
        LIN.Access.Notes.Build.Init();

        return builder.Build();
    }



    public static void Aa()
    {
#if ANDROID
        var currentActivity = Platform.CurrentActivity;

        if (currentActivity == null || currentActivity.Window == null)
            return;

        var currentTheme = AppInfo.RequestedTheme;

        if (currentTheme == AppTheme.Light)
        {
            currentActivity.Window.SetStatusBarColor(new(255, 255, 255));
            currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }
        else
        {
            currentActivity.Window.SetStatusBarColor(new(0, 0, 0));
            currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }


        //currentActivity.Window.SetTitleColor(new(0, 0, 0));
#endif
    }


}