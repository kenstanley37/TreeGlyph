#if WINDOWS
using TreeGlyph.UI.Services;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TreeGlyph.UI.Platforms.Windows;

public class FileSaveService : IFileSaveService
{
    public async Task<string?> PickSavePathAsync(string suggestedFileName)
    {
        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.Desktop,
            SuggestedFileName = suggestedFileName
        };

        picker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });

        var hwnd = ((MauiWinUIWindow)Microsoft.Maui.Controls.Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }
}
#endif