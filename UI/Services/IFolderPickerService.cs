namespace TreeGlyph.UI.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}