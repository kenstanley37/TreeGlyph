namespace UI.Services;

public interface IFolderPickerService
{
    Task<string?> PickFolderAsync();
}