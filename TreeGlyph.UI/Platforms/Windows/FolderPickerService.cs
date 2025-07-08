// Platforms/Windows/FolderPickerService.cs
#if WINDOWS
using TreeGlyph.Platforms.Windows;
using TreeGlyph.UI.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;
[assembly: Dependency(typeof(FolderPickerService))]
namespace TreeGlyph.Platforms.Windows;


public class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        var hwnd = WindowNative.GetWindowHandle(
            (Microsoft.Maui.Controls.Application.Current?.Windows[0].Handler.PlatformView));
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
#endif