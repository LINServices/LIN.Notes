﻿#if ANDROID
using Android.Views;

#endif
using Microsoft.Extensions.Logging;
using LIN.Access.Auth;

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
        builder.Services.AddAuthenticationService();

        // Debug mode.
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddNotesService();

        LIN.Notes.Services.Realtime.Build();

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
            currentActivity.Window.SetNavigationBarColor(new(255, 255, 255));
            currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }
        else
        {
            currentActivity.Window.SetStatusBarColor(new(0, 0, 0));
            currentActivity.Window.SetNavigationBarColor(new(0, 0, 0));
           currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.Visible;
        }

#endif
    }


    public static void Set(int[] light, int[] dark)
    {
#if ANDROID

        var currentActivity = Platform.CurrentActivity;

        if (currentActivity == null || currentActivity.Window == null)
            return;

        var currentTheme = AppInfo.RequestedTheme;

        if (currentTheme == AppTheme.Light)
        {
            currentActivity.Window.SetStatusBarColor(new(light[0], light[1], light[2]));
            currentActivity.Window.SetNavigationBarColor(new(light[0], light[1], light[2]));
            currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.LightStatusBar;
        }
        else
        {
            currentActivity.Window.SetStatusBarColor(new(dark[0], dark[1], dark[2]));
            currentActivity.Window.SetNavigationBarColor(new(dark[0], dark[1], dark[2]));
            currentActivity.Window.DecorView.SystemUiVisibility = (StatusBarVisibility)SystemUiFlags.Visible;
        }


        //currentActivity.Window.SetTitleColor(new(0, 0, 0));
#endif
    }


}