using Microsoft.Extensions.Logging;
using UI;
using Core.Services;
using UI.Services;
using UI.ViewModels;
using CommunityToolkit.Maui;
using Platforms.Windows;


#if WINDOWS
using UI.Platforms.Windows;
#endif

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
             .ConfigureEffects(effects =>
             {
                 effects.Add<UI.Effects.HoverCursorEffect, WinUI.Effects.HoverCursorEffectHandler>();
             })
            .UseMauiCommunityToolkit(options =>
            {
#if WINDOWS
                options.SetShouldEnableSnackbarOnWindows(true);
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<TreeBuilderService>();
#if WINDOWS
        builder.Services.AddSingleton<IFileSaveService, FileSaveService>();
#endif


#if WINDOWS
        builder.Services.AddSingleton<IFolderPickerService, FolderPickerService>();
#endif
#if WINDOWS
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("ForceWindowsScroll", (handler, view) =>
        {
            if (handler.PlatformView is Microsoft.UI.Xaml.Controls.TextBox nativeEditor)
            {
                nativeEditor.TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap;
                nativeEditor.AcceptsReturn = true;
                nativeEditor.IsTabStop = true;
                // You can handle tab input manually if needed via keyboard events
            }
        });
#endif


        return builder.Build();
    }
}