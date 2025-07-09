using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using System.Windows.Input;
using TreeGlyph.UI.Helpers;
using TreeGlyph.UI.Services;
using TreeGlyph.UI.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TreeGlyph.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFolderPickerService folderPicker;
    private readonly TreeBuilderService _treeBuilderService;

    public MainViewModel(TreeBuilderService treeBuilderService, IFolderPickerService folderPicker)
    {
        _treeBuilderService = treeBuilderService;
        this.folderPicker = folderPicker;

        autoSaveIgnoreFile = Preferences.Get(nameof(AutoSaveIgnoreFile), false);
        isAdvancedExpanded = Preferences.Get(nameof(IsAdvancedExpanded), false);
        leftPanelWidth = Math.Max(Preferences.Get(nameof(LeftPanelWidth), 320), 240);

    }

    // Layout preferences
    private double leftPanelWidth;
    public double LeftPanelWidth
    {
        get => leftPanelWidth;
        set
        {
            if (SetProperty(ref leftPanelWidth, value))
                Preferences.Set(nameof(LeftPanelWidth), value);
        }
    }

    private bool isAdvancedExpanded;
    public bool IsAdvancedExpanded
    {
        get => isAdvancedExpanded;
        set
        {
            if (SetProperty(ref isAdvancedExpanded, value))
                Preferences.Set(nameof(IsAdvancedExpanded), value);
        }
    }

    // Core state
    private string selectedFolderPath = string.Empty;
    public string SelectedFolderPath
    {
        get => selectedFolderPath;
        set
        {
            if (SetProperty(ref selectedFolderPath, value))
                OnPropertyChanged(nameof(HasSelectedFolder));
        }
    }

    private string exclusions = string.Empty;
    public string Exclusions
    {
        get => exclusions;
        set
        {
            if (SetProperty(ref exclusions, value))
            {
                var lines = value?
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .Where(p => !string.IsNullOrWhiteSpace(p) && !p.StartsWith("#"))
                    .ToArray() ?? [];

                _treeBuilderService.SetExclusions(lines);
            }
        }
    }

    private string savePath = string.Empty;
    public string SavePath
    {
        get => savePath;
        set => SetProperty(ref savePath, value);
    }

    private string treeOutput = string.Empty;
    public string TreeOutput
    {
        get => treeOutput;
        set => SetProperty(ref treeOutput, value);
    }

    private string testPath = string.Empty;
    public string TestPath
    {
        get => testPath;
        set
        {
            if (SetProperty(ref testPath, value))
            {
                var normalized = value?.Replace('\\', '/');
                ExclusionTestResult = string.IsNullOrWhiteSpace(normalized)
                    ? string.Empty
                    : _treeBuilderService.IsExcluded(normalized)
                        ? "❌ Excluded"
                        : "✅ Included";
            }
        }
    }

    private string exclusionTestResult = string.Empty;
    public string ExclusionTestResult
    {
        get => exclusionTestResult;
        set => SetProperty(ref exclusionTestResult, value);
    }

    private bool isTooltipOpen;
    public bool IsTooltipOpen
    {
        get => isTooltipOpen;
        set => SetProperty(ref isTooltipOpen, value);
    }

    private string tooltipMessage = string.Empty;
    public string TooltipMessage
    {
        get => tooltipMessage;
        set => SetProperty(ref tooltipMessage, value);
    }

    private bool autoSaveIgnoreFile;
    public bool AutoSaveIgnoreFile
    {
        get => autoSaveIgnoreFile;
        set
        {
            if (SetProperty(ref autoSaveIgnoreFile, value))
                Preferences.Set(nameof(AutoSaveIgnoreFile), value);
        }
    }
    public ICommand ShowAboutCommand => new AsyncRelayCommand(ExecuteShowAbout);

    private async Task ExecuteShowAbout()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage)); // or PushAsync depending on your nav setup
    }


    // Derived property
    public bool HasSelectedFolder => !string.IsNullOrWhiteSpace(SelectedFolderPath);

    // 📂 Commands

    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        Debug.WriteLine("📂 Browse command fired!");
        var result = await folderPicker.PickFolderAsync();

        if (!string.IsNullOrWhiteSpace(result))
        {
            SelectedFolderPath = result;

            var ignorePath = Path.Combine(result, ".treeglyphignore");
            if (File.Exists(ignorePath))
            {
                Exclusions = await File.ReadAllTextAsync(ignorePath);
                Debug.WriteLine("📄 Loaded .treeglyphignore");
            }

            GenerateTreeCommand?.Execute(null);

            if (AutoSaveIgnoreFile && !string.IsNullOrWhiteSpace(Exclusions))
                SaveIgnoreFileCommand?.Execute(null);
        }
    }

    [RelayCommand]
    private void GenerateTree()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            return;

        var entry = _treeBuilderService.BuildTree(SelectedFolderPath);
        TreeOutput = entry?.ToAsciiTree() ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveTreeAsync()
    {
#if WINDOWS
        if (App.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView is MauiWinUIWindow winUIWindow)
        {
            var hwnd = winUIWindow.WindowHandle;

            var picker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeChoices.Add("Text File", new List<string>() { ".txt" });
            picker.SuggestedFileName = "tree";

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteTextAsync(file, TreeOutput);
                await Toast.Make("✅ Tree saved successfully.").Show();
            }
        }
        else
        {
            await Toast.Make("⚠️ Unable to find an active window. Try again after the UI loads.", ToastDuration.Short).Show();
        }
#endif
    }

    [RelayCommand]
    private void ClearTree()
    {
        TreeOutput = string.Empty;
        //TreeOutput = string.Join("\n", Enumerable.Range(1, 200).Select(i => $"Line {i}"));
        Exclusions = string.Empty;
        SavePath = string.Empty;
        TestPath = string.Empty;
        ExclusionTestResult = string.Empty;
    }

    [RelayCommand]
    private async Task SaveIgnoreFileAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath) || string.IsNullOrWhiteSpace(Exclusions))
            return;

        var ignorePath = Path.Combine(SelectedFolderPath, ".treeglyphignore");
        await File.WriteAllTextAsync(ignorePath, Exclusions.Trim());
        await Toast.Make("Ignore File Saved").Show();
    }

    [RelayCommand]
    private async Task CopyPreviewAsync()
    {
        if (!string.IsNullOrWhiteSpace(TreeOutput))
        {
            await Clipboard.SetTextAsync(TreeOutput);
            await Shell.Current.DisplayAlert("Copied", "Preview copied to clipboard.", "OK");
        }
    }
}