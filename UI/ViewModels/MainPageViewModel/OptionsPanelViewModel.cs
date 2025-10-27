using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;
using System.ComponentModel;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class OptionsPanelViewModel : ObservableObject
{
    private MainViewModel? parent;
    public MainViewModel? Parent
    {
        get => parent;
        set
        {
            if (SetProperty(ref parent, value))
            {
                if (parent is not null)
                {
                    parent.PropertyChanged += OnParentPropertyChanged;
                    LogService.Write("OPTIONS", "Parent ViewModel assigned.");
                }

                // Initial sync
                OnPropertyChanged(nameof(HasSelectedFolder));
                LogService.Write("OPTIONS", "HasSelectedFolder synced from parent.");
            }
        }
    }

    private void OnParentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(MainViewModel.HasSelectedFolder):
                OnPropertyChanged(nameof(HasSelectedFolder));
                LogService.Write("OPTIONS", "HasSelectedFolder updated from parent.");
                break;
        }
    }

    public bool HasSelectedFolder => Parent?.HasSelectedFolder ?? false;

    private int maxDepth = Preferences.Get(nameof(MaxDepth), 99);
    public int MaxDepth
    {
        get => maxDepth;
        set
        {
            if (SetProperty(ref maxDepth, value))
            {
                Preferences.Set(nameof(MaxDepth), value);
                LogService.Write("OPTIONS", $"MaxDepth set to: {value}");
            }
        }
    }

    private bool autoSaveIgnoreFile = Preferences.Get(nameof(AutoSaveIgnoreFile), false);
    public bool AutoSaveIgnoreFile
    {
        get => autoSaveIgnoreFile;
        set
        {
            if (SetProperty(ref autoSaveIgnoreFile, value))
            {
                Preferences.Set(nameof(AutoSaveIgnoreFile), value);
                LogService.Write("OPTIONS", $"AutoSaveIgnoreFile set to: {value}");
            }
        }
    }

    [RelayCommand]
    private async Task ShowMaxDepthInfo()
    {
        LogService.Write("OPTIONS", "MaxDepth info dialog shown.");
        await Shell.Current.CurrentPage.DisplayAlert(
            "Max Depth",
            "This setting controls how many levels deep TreeGlyph will scan and display folders. A lower value shows a simpler tree.",
            "OK"
        );
    }

    [RelayCommand]
    private async Task ShowExclusionInfo()
    {
        LogService.Write("OPTIONS", "Exclusion info dialog shown.");
        await Shell.Current.CurrentPage.DisplayAlert(
            "Exclude Patterns",
            "These patterns follow .gitignore rules. Use them to exclude folders or files from the tree view. Examples include:\n\n- node_modules/\n- *.log\n- bin/\n- obj/",
            "OK"
        );
    }

    [RelayCommand]
    private async Task ShowExclusionExamples()
    {
        LogService.Write("OPTIONS", "Exclusion examples dialog shown.");
        await Shell.Current.CurrentPage.DisplayAlert(
            "Common Exclusion Patterns",
            "• node_modules/\n• *.log\n• bin/\n• obj/\n• .vs/\n• *.tmp\n• *.user\n• .DS_Store",
            "Got it"
        );
    }

    [RelayCommand]
    private async Task ShowAutoSaveInfo()
    {
        LogService.Write("OPTIONS", "AutoSaveIgnoreFile toast shown.");
        await Toast.Make("Automatically saves .treeglyphignore when exclusions change.").Show();
    }
}