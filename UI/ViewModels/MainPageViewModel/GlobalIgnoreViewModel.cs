using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UI.ViewModels.MainPageViewModel;

public partial class GlobalIgnoreViewModel : ObservableObject
{
    private string globalIgnoreExclusions = string.Empty;
    public string GlobalIgnoreExclusions
    {
        get => globalIgnoreExclusions;
        set => SetProperty(ref globalIgnoreExclusions, value);
    }

    private bool isEditingGlobalIgnore;
    public bool IsEditingGlobalIgnore
    {
        get => isEditingGlobalIgnore;
        set => SetProperty(ref isEditingGlobalIgnore, value);
    }

    private bool shouldApplyGlobalIgnore;
    public bool ShouldApplyGlobalIgnore
    {
        get => shouldApplyGlobalIgnore;
        set => SetProperty(ref shouldApplyGlobalIgnore, value);
    }

    public string IgnoreFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TreeGlyph",
        "ignore-global.txt");

    [RelayCommand]
    public async Task LoadGlobalIgnoreAsync()
    {
        if (!File.Exists(IgnoreFilePath))
            File.WriteAllText(IgnoreFilePath, "# Add your global ignore rules here\n");

        GlobalIgnoreExclusions = await File.ReadAllTextAsync(IgnoreFilePath);
        IsEditingGlobalIgnore = true;
    }

    [RelayCommand]
    public async Task SaveGlobalIgnoreAsync()
    {
        await File.WriteAllTextAsync(IgnoreFilePath, GlobalIgnoreExclusions.Trim());
        IsEditingGlobalIgnore = false;
    }

    [RelayCommand]
    public void ApplyGlobalIgnore()
    {
        ShouldApplyGlobalIgnore = true;
    }
}