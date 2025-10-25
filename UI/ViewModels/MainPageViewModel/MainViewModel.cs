using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Helpers;
using Core.Services;
using System.Windows.Input;
using UI.Services;
using UI.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace UI.ViewModels.MainPageViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly TreeBuilderService _treeBuilderService;
    private readonly IFolderPickerService folderPicker;

    public OptionsPanelViewModel OptionsPanel { get; }
    public GlobalIgnoreViewModel GlobalIgnore { get; }
    public TooltipViewModel Tooltip { get; }

    public MainViewModel(TreeBuilderService treeBuilderService, IFolderPickerService folderPicker)
    {
        _treeBuilderService = treeBuilderService;
        this.folderPicker = folderPicker;

        OptionsPanel = new OptionsPanelViewModel();
        GlobalIgnore = new GlobalIgnoreViewModel();
        Tooltip = new TooltipViewModel();

        leftPanelWidth = Math.Max(Preferences.Get(nameof(LeftPanelWidth), 320), 240);
    }

    // Layout Preferences
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

    // Core State
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
        set => SetProperty(ref exclusions, value);
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

    // Derived
    public bool HasSelectedFolder => !string.IsNullOrWhiteSpace(SelectedFolderPath);

    public ICommand ShowAboutCommand => new AsyncRelayCommand(ExecuteShowAbout);

    private async Task ExecuteShowAbout()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        var result = await folderPicker.PickFolderAsync();
        if (string.IsNullOrWhiteSpace(result)) return;

        SelectedFolderPath = result;

        var ignorePath = Path.Combine(result, ".treeglyphignore");
        if (File.Exists(ignorePath))
            Exclusions = await File.ReadAllTextAsync(ignorePath);

        GenerateTreeCommand?.Execute(null);

        if (OptionsPanel.AutoSaveIgnoreFile && !string.IsNullOrWhiteSpace(Exclusions))
            await SaveIgnoreFileAsync();
    }

    [RelayCommand]
    private void GenerateTree()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            return;

        var globalRules = GlobalIgnore.ShouldApplyGlobalIgnore && File.Exists(GlobalIgnore.IgnoreFilePath)
            ? File.ReadAllLines(GlobalIgnore.IgnoreFilePath)
            : Array.Empty<string>();

        var localRules = exclusions.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));

        var allRules = globalRules.Concat(localRules);
        _treeBuilderService.SetExclusions(allRules);

        var entry = _treeBuilderService.BuildTree(SelectedFolderPath);
        TreeOutput = entry?.ToAsciiTree(0, OptionsPanel.MaxDepth) ?? string.Empty;
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

            picker.FileTypeChoices.Add("Text File", new List<string> { ".txt" });
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
            await Toast.Make("⚠️ Unable to find an active window.", ToastDuration.Short).Show();
        }
#endif
    }

    [RelayCommand]
    private void ClearTree()
    {
        TreeOutput = string.Empty;
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