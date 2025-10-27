using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Core.Helpers;
using Core.Services;
using Infrastructure.Services;
using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Windows.Input;
using TreeGlyph.UI.Services;
using TreeGlyph.UI.Views;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class MainViewModel : ObservableObject
{
    private readonly TreeBuilderService _treeBuilderService;
    private readonly IFolderPickerService folderPicker;
    private CancellationTokenSource? cancellationTokenSource;
    private readonly DispatcherTimer timer;

    public OptionsPanelViewModel OptionsPanel { get; } = new();
    public GlobalIgnoreViewModel GlobalIgnore { get; } = new();
    public TooltipViewModel Tooltip { get; }
    public FolderPanelViewModel FolderPanel { get; }
    public ToolbarViewModel Toolbar { get; } = new();
    public TreePreviewViewModel TreePreview { get; } = new();

    public IAsyncRelayCommand GenerateTreeAsyncCommand { get; }


    public MainViewModel(TreeBuilderService treeBuilderService, IFolderPickerService folderPicker)
    {
        _treeBuilderService = treeBuilderService;
        this.folderPicker = folderPicker;

        // Instantiate child view models
        OptionsPanel = new OptionsPanelViewModel();
        GlobalIgnore = new GlobalIgnoreViewModel();
        Tooltip = new TooltipViewModel();
        FolderPanel = new FolderPanelViewModel(GlobalIgnore);
        Toolbar = new ToolbarViewModel();
        TreePreview = new TreePreviewViewModel();

        // ✅ Wire up parent references
        OptionsPanel.Parent = this;
        GlobalIgnore.Parent = this;
        Tooltip.Parent = this;
        FolderPanel.Parent = this;
        Toolbar.Parent = this;
        TreePreview.Parent = this;

        // Load persisted layout settings
        leftPanelWidth = Math.Max(Preferences.Get(nameof(LeftPanelWidth), 320), 240);

        // Setup timer
        timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        timer.Tick += OnTimerTick;

        // Setup commands
        GenerateTreeAsyncCommand = new AsyncRelayCommand(GenerateTreeAsync);
    }

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (SetProperty(ref isBusy, value))
            {
                CancelTreeBuildCommand.NotifyCanExecuteChanged();
            }
        }
    }

    private string? currentFolder;
    public string? CurrentFolder
    {
        get => currentFolder;
        set => SetProperty(ref currentFolder, value);
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
            {
                OnPropertyChanged(nameof(HasSelectedFolder));
                SaveIgnoreFileCommand.NotifyCanExecuteChanged();
            }
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
                SaveIgnoreFileCommand.NotifyCanExecuteChanged();
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

    private string skippedPathsLog = string.Empty;
    public string SkippedPathsLog
    {
        get => skippedPathsLog;
        set => SetProperty(ref skippedPathsLog, value);
    }

    private double progress;
    public double Progress
    {
        get => progress;
        set => SetProperty(ref progress, value);
    }

    public void LogSkippedPath(string path)
    {
        SkippedPathsLog += $"{path}{Environment.NewLine}";
    }

    public ICommand ViewLogsCommand => new RelayCommand(OnViewLogs);

    private void OnViewLogs()
    {
        Shell.Current.GoToAsync("//LogViewPage");
    }


    // Derived
    public bool HasSelectedFolder => !string.IsNullOrWhiteSpace(SelectedFolderPath);

    public ICommand ShowAboutCommand => new AsyncRelayCommand(ExecuteShowAbout);

    private async Task ExecuteShowAbout()
    {
        await Shell.Current.GoToAsync(nameof(AboutPage));
    }

    private Stopwatch stopwatch = new();

    public string ElapsedTime => stopwatch.Elapsed.ToString(@"mm\:ss\.ff");

    public void StartScan()
    {
        stopwatch.Restart();
        timer.Start(); // Use a DispatcherTimer to update UI every 100ms
    }

    private long peakMemory = 0;
    public string PeakMemoryUsage => $"{peakMemory / 1024 / 1024:N0} MB";

    private void OnTimerTick(object? sender, object e)
    {
        var current = GC.GetTotalMemory(false);
        if (current > peakMemory) peakMemory = current;

        OnPropertyChanged(nameof(ElapsedTime));
        OnPropertyChanged(nameof(MemoryUsage));
        OnPropertyChanged(nameof(PeakMemoryUsage));
    }


    public string MemoryUsage =>
    $"{GC.GetTotalMemory(true) / 1024 / 1024:N0} MB";

    private bool CanSaveTree() => !string.IsNullOrWhiteSpace(TreeOutput);
    private bool CanClearTree() => !string.IsNullOrWhiteSpace(TreeOutput);
    private bool CanCopyPreview() => !string.IsNullOrWhiteSpace(TreeOutput);
    private bool CanSaveIgnoreFile() =>
        !string.IsNullOrWhiteSpace(SelectedFolderPath) &&
        !string.IsNullOrWhiteSpace(Exclusions);


    [RelayCommand]
    private async Task BrowseFolderAsync()
    {
        var result = await folderPicker.PickFolderAsync();
        if (string.IsNullOrWhiteSpace(result))
        {
            LogService.Write("FOLDER", "Folder selection canceled or empty.");
            return;
        }

        SelectedFolderPath = result;
        Preferences.Default.Set("LastFolderPath", result);
        LogService.Write("FOLDER", $"Folder selected: {result}");

        // Load .treeglyphignore if present
        var ignorePath = Path.Combine(result, ".treeglyphignore");
        if (File.Exists(ignorePath))
        {
            Exclusions = await File.ReadAllTextAsync(ignorePath);
            LogService.Write("IGNORE-LOAD", $"Loaded .treeglyphignore from: {ignorePath}");
        }
        else
        {
            LogService.Write("IGNORE-LOAD", "No .treeglyphignore file found.");
        }

        // Refresh toolbar command states
        GenerateTreeCommand.NotifyCanExecuteChanged();
        SaveTreeCommand.NotifyCanExecuteChanged();
        SaveIgnoreFileCommand.NotifyCanExecuteChanged();
        CopyPreviewCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();
        LogService.Write("UI", "Toolbar commands refreshed after folder selection.");

        // Auto-generate tree
        LogService.Write("TREEGEN", "Auto-generating tree...");
        GenerateTreeCommand?.Execute(null);

        // Auto-save ignore file if enabled
        if (OptionsPanel.AutoSaveIgnoreFile && !string.IsNullOrWhiteSpace(Exclusions))
        {
            LogService.Write("IGNORE-SAVE", "Auto-save triggered.");
            await SaveIgnoreFileAsync();
        }
    }


    [RelayCommand]
    private async Task GenerateTreeAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath))
            return;

        IsBusy = true;
        StartScan();
        Progress = 0;
        SkippedPathsLog = string.Empty;
        cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        // 🔄 Start progress animation loop
        var animationTask = Task.Run(async () =>
        {
            while (IsBusy)
            {
                for (double p = 0; p <= 1.0; p += 0.02)
                {
                    Progress = p;
                    await Task.Delay(30);
                    if (!IsBusy) break;
                }
            }
        });

        try
        {
            TreeOutput = "⏳ Building tree...";
            LogService.Write("TREEGEN", $"Starting tree build for: {SelectedFolderPath}");

            // 🧾 Load global ignore rules
            var globalRules = Array.Empty<string>();

            if (GlobalIgnore.ShouldApplyGlobalIgnore)
            {
                globalRules = GlobalIgnore.GlobalIgnoreExclusions
                    .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(line => line.Trim())
                    .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                    .ToArray();
            }

            LogService.Write("TREEGEN", $"Global ignore enabled: {GlobalIgnore.ShouldApplyGlobalIgnore}");

            if (GlobalIgnore.ShouldApplyGlobalIgnore && globalRules.Any())
            {
                LogService.Write("EXCLUSION-SET", $"Parsed {globalRules.Count()} global globs:");
                foreach (var rule in globalRules)
                    LogService.Write("GLOBAL-RULE", $"• {rule}");
            }
            else
            {
                LogService.Write("EXCLUSION-SET", "No global rules applied.");
            }

            // 🧾 Load local ignore rules from .treeglyphignore
            var localRules = Exclusions
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"));

            if (localRules.Any())
            {
                LogService.Write("EXCLUSION-SET", $"Parsed {localRules.Count()} local globs:");
                foreach (var rule in localRules)
                    LogService.Write("LOCAL-RULE", $"• {rule}");
            }
            else
            {
                LogService.Write("EXCLUSION-SET", "No local rules found.");
            }

            // 🧩 Combine and apply exclusions
            //var allRules = globalRules.Concat(localRules).ToList();
            var allRules = globalRules
                .Concat(localRules)
                .Select(rule => rule.Trim())
                .Where(rule => !string.IsNullOrWhiteSpace(rule))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            LogService.Write("EXCLUSION-SET", $"Total exclusions applied after deduplication: {allRules.Count}");

            foreach (var rule in allRules
                .Select(r => r.Trim())
                .Where(r => !string.IsNullOrWhiteSpace(r)))
            {
                LogService.Write("EXCLUSION-SET", $"   • {rule}");
            }


            _treeBuilderService.SetExclusions(allRules);

            // 📋 Inject real-time skipped path logger
            _treeBuilderService.SetSkippedPathLogger(path =>
            {
                SkippedPathsLog += $"⛔ {path}{Environment.NewLine}";
            });

            // 🌲 Build tree
            var entry = await Task.Run(() => _treeBuilderService.BuildTree(SelectedFolderPath, token), token);
            TreeOutput = entry?.ToAsciiTree(0, OptionsPanel.MaxDepth) ?? string.Empty;

            if (string.IsNullOrWhiteSpace(SkippedPathsLog))
                SkippedPathsLog = "✅ No skipped folders.";

            LogService.Write("TREEGEN", "Tree build completed.");
        }
        catch (OperationCanceledException)
        {
            TreeOutput = "⛔ Tree building canceled.";
            LogService.Write("TREEGEN", "Tree build canceled by user.");
        }
        finally
        {
            IsBusy = false;
            Progress = 0;
            cancellationTokenSource = null;
            timer.Stop();
            await animationTask;

            // 🔄 Refresh toolbar command states
            SaveTreeCommand.NotifyCanExecuteChanged();
            SaveIgnoreFileCommand.NotifyCanExecuteChanged();
            CopyPreviewCommand.NotifyCanExecuteChanged();
            ClearTreeCommand.NotifyCanExecuteChanged();
        }
    }

    [RelayCommand(CanExecute = nameof(IsBusy))]
    private async Task CancelTreeBuild()
    {
        if (!IsBusy)
            return;

        LogService.Write("TREEGEN", "Cancel requested by user.");
        cancellationTokenSource?.Cancel();
        IsBusy = false;

        LogService.Write("TREEGEN", "Cancellation token triggered.");
        LogService.Write("UI", "Refreshing toolbar command states...");

        // 🔄 Refresh toolbar command states
        GenerateTreeCommand.NotifyCanExecuteChanged();
        SaveTreeCommand.NotifyCanExecuteChanged();
        SaveIgnoreFileCommand.NotifyCanExecuteChanged();
        CopyPreviewCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();

        LogService.Write("UI", "Toolbar commands refreshed.");
        await Toast.Make("⛔ Tree building canceled.").Show();
    }

    [RelayCommand(CanExecute = nameof(CanSaveTree))]
    private async Task SaveTreeAsync()
    {
        if (string.IsNullOrWhiteSpace(TreeOutput))
        {
            LogService.Write("[SAVE]", "TreeOutput is empty. Save aborted.");
            return;
        }

        LogService.Write("[SAVE]", "SaveTreeAsync invoked.");

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
                LogService.Write("[SAVE]", $"Tree saved to: {file.Path}");
                await Toast.Make("✅ Tree saved successfully.").Show();
            }
            else
            {
                LogService.Write("[SAVE]", "Save operation canceled by user.");
            }
        }
        else
        {
            LogService.Write("[SAVE]", "Unable to find an active window. Save aborted.");
            await Toast.Make("⚠️ Unable to find an active window.", ToastDuration.Short).Show();
        }
#endif

        LogService.Write("UI", "Refreshing toolbar command states after save.");
        SaveTreeCommand.NotifyCanExecuteChanged();
        CopyPreviewCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanClearTree))]
    private void ClearTree()
    {
        TreeOutput = string.Empty;
        Exclusions = string.Empty;
        SavePath = string.Empty;
        TestPath = string.Empty;
        ExclusionTestResult = string.Empty;
        SkippedPathsLog = string.Empty;

        LogService.Write("[CLEAR]", "TreeOutput, Exclusions, SavePath, TestPath, ExclusionTestResult, and SkippedPathsLog cleared.");
        LogService.Write("[EXCLUSION-RESET]", "Local exclusions cleared.");

        // 🔄 Refresh toolbar command states
        GenerateTreeCommand.NotifyCanExecuteChanged();
        SaveTreeCommand.NotifyCanExecuteChanged();
        SaveIgnoreFileCommand.NotifyCanExecuteChanged();
        CopyPreviewCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();

        LogService.Write("UI", "Toolbar commands refreshed after ClearTree.");
    }

    [RelayCommand(CanExecute = nameof(CanSaveIgnoreFile))]
    private async Task SaveIgnoreFileAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedFolderPath) || string.IsNullOrWhiteSpace(Exclusions))
        {
            LogService.Write("IGNORE-SAVE", "Save skipped: missing folder path or exclusions.");
            return;
        }

        var ignorePath = Path.Combine(SelectedFolderPath, ".treeglyphignore");
        LogService.Write("IGNORE-SAVE", $"Saving ignore file to: {ignorePath}");

        await File.WriteAllTextAsync(ignorePath, Exclusions.Trim());
        LogService.Write("IGNORE-SAVE", "Ignore file saved successfully.");

        await Toast.Make("✅ Ignore file saved.").Show();

        // 🔄 Refresh toolbar command states
        SaveIgnoreFileCommand.NotifyCanExecuteChanged();
        GenerateTreeCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();

        LogService.Write("UI", "Toolbar commands refreshed after ignore file save.");
    }

    [RelayCommand(CanExecute = nameof(CanCopyPreview))]
    private async Task CopyPreviewAsync()
    {
        if (string.IsNullOrWhiteSpace(TreeOutput))
        {
            LogService.Write("[COPY]", "TreeOutput is empty. Copy aborted.");
            return;
        }

        LogService.Write("[COPY]", "CopyPreviewAsync invoked.");
        await Clipboard.SetTextAsync(TreeOutput);
        LogService.Write("[COPY]", "TreeOutput copied to clipboard.");

        await Shell.Current.DisplayAlert("Copied", "Preview copied to clipboard.", "OK");

        // 🔄 Refresh toolbar command states
        CopyPreviewCommand.NotifyCanExecuteChanged();
        ClearTreeCommand.NotifyCanExecuteChanged();

        LogService.Write("UI", "Toolbar commands refreshed after copy.");
    }
}