using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;

namespace UI.ViewModels.MainPageViewModel;

public partial class GlobalIgnoreViewModel : ObservableObject
{
    public MainViewModel? Parent { get; set; }

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

    private bool shouldApplyGlobalIgnore = Preferences.Get(nameof(ShouldApplyGlobalIgnore), false);
    public bool ShouldApplyGlobalIgnore
    {
        get => shouldApplyGlobalIgnore;
        set
        {
            if (SetProperty(ref shouldApplyGlobalIgnore, value))
            {
                Preferences.Set(nameof(ShouldApplyGlobalIgnore), value);
                LogService.Write("GLOBAL-IGNORE", $"Toggle persisted: {value}");
            }
        }
    }

    public string IgnoreFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TreeGlyph",
        "ignore-global.txt");

    private bool hasLoggedLoad = false;

    public GlobalIgnoreViewModel()
    {
        IsEditingGlobalIgnore = false; // 🔒 Start hidden
        LoadGlobalIgnoreFile();
    }

    private void LoadGlobalIgnoreFile()
    {
        if (File.Exists(IgnoreFilePath))
        {
            GlobalIgnoreExclusions = File.ReadAllText(IgnoreFilePath);

            if (!hasLoggedLoad)
            {
                LogService.Write("GLOBAL-IGNORE", $"Loaded global ignore file: {IgnoreFilePath}");
                hasLoggedLoad = true;
            }

            if (!string.IsNullOrWhiteSpace(GlobalIgnoreExclusions))
            {
                ShouldApplyGlobalIgnore = true;
                LogService.Write("GLOBAL-IGNORE", "Auto-applied global ignore rules.");
            }
        }
        else
        {
            LogService.Write("GLOBAL-IGNORE", "No global ignore file found on startup.");
        }
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditingGlobalIgnore = false;
        LogService.Write("GLOBAL-IGNORE", "Edit canceled.");
    }

    [RelayCommand]
    public async Task EditGlobalIgnoreAsync()
    {
        if (!File.Exists(IgnoreFilePath))
        {
            await Task.Run(() =>
            {
                File.WriteAllText(IgnoreFilePath, "# Add your global ignore rules here\n");
            });

            LogService.Write("GLOBAL-IGNORE", $"Created new ignore file: {IgnoreFilePath}");
        }

        await Task.Run(() => LoadGlobalIgnoreFile());

        IsEditingGlobalIgnore = true;
        LogService.Write("GLOBAL-IGNORE", "Edit session started.");
    }

    [RelayCommand]
    public async Task SaveGlobalIgnoreAsync()
    {
        await File.WriteAllTextAsync(IgnoreFilePath, GlobalIgnoreExclusions.Trim());
        IsEditingGlobalIgnore = false;
        LogService.Write("GLOBAL-IGNORE", $"Saved global ignore file: {IgnoreFilePath}");
    }

    [RelayCommand]
    public void ApplyGlobalIgnore()
    {
        ShouldApplyGlobalIgnore = true;
        LogService.Write("GLOBAL-IGNORE", "Global ignore applied.");
    }
}