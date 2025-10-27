using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class GlobalIgnoreViewModel : ObservableObject
{
    public MainViewModel? Parent { get; set; }

    private const string PreferenceKey = nameof(ShouldApplyGlobalIgnore);
    private bool isInitializing = true;

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

    public bool IsGlobalIgnoreEnabled
    {
        get
        {
            LogService.Write("GLOBAL-IGNORE", $"Switch state read: {ShouldApplyGlobalIgnore}");
            return ShouldApplyGlobalIgnore;
        }
        set
        {
            LogService.Write("GLOBAL-IGNORE", $"Switch state changed by user: {value}");
            ShouldApplyGlobalIgnore = value;
        }
    }

    private bool shouldApplyGlobalIgnore;
    public bool ShouldApplyGlobalIgnore
    {
        get => shouldApplyGlobalIgnore;
        set
        {
            if (SetProperty(ref shouldApplyGlobalIgnore, value))
            {
                if (!isInitializing)
                {
                    Preferences.Set(PreferenceKey, value);
                    LogService.Write("GLOBAL-IGNORE", $"Toggle persisted: {value}");
                }
                else
                {
                    LogService.Write("GLOBAL-IGNORE", $"Toggle loaded during init: {value}");
                }
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
        isInitializing = true;
        IsEditingGlobalIgnore = false;
        ShouldApplyGlobalIgnore = Preferences.Get(PreferenceKey, false);
        LogService.Write("GLOBAL-IGNORE", $"Toggle loaded: {ShouldApplyGlobalIgnore}");
        LoadGlobalIgnoreFile();
        isInitializing = false;
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
                LogService.Write("GLOBAL-IGNORE", $"Global ignore file has content ({GlobalIgnoreExclusions.Length} chars).");
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