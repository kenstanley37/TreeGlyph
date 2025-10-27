
using Core.Services;
using Windows.Storage.Pickers; // For FolderPicker
using WinRT.Interop;           // For InitializeWithWindow

namespace TreeGlyph.UI.Services;

public class DefaultFolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
#if WINDOWS
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");

        var hwnd = WindowNative.GetWindowHandle(
            Microsoft.Maui.Controls.Application.Current?.Windows[0].Handler.PlatformView);
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
#else
        // MAUI doesn't support folder picking on macOS/Linux yet
        await Microsoft.Maui.Controls.Application.Current.MainPage.DisplayAlert(
            "Unsupported",
            "Folder picking is not yet supported on this platform.",
            "OK");
        return null;
#endif
    }

}

