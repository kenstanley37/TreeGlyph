namespace TreeGlyph.UI.Services;
public interface IFileSaveService
{
    Task<string?> PickSavePathAsync(string suggestedFileName);
}