using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Infrastructure.Services;

namespace UI.ViewModels.MainPageViewModel;

public partial class FolderPanelViewModel : ObservableObject
{
    private MainViewModel? parent;
    public MainViewModel? Parent
    {
        get => parent;
        set
        {
            if (SetProperty(ref parent, value))
            {
                OnPropertyChanged(nameof(SelectedFolderPath));
                OnPropertyChanged(nameof(Exclusions));

                if (parent is not null)
                {
                    // Initial sync
                    ExclusionsLocal = parent.Exclusions;

                    parent.PropertyChanged += (_, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(MainViewModel.SelectedFolderPath):
                                OnPropertyChanged(nameof(SelectedFolderPath));
                                break;

                            case nameof(MainViewModel.Exclusions):
                                ExclusionsLocal = parent.Exclusions;
                                LogService.Write("EXCLUSION", $"📝 Synced from Parent.Exclusions ({parent.Exclusions.Length} chars)");
                                break;
                        }
                    };
                }
            }
        }
    }

    public string SelectedFolderPath => Parent?.SelectedFolderPath ?? string.Empty;
    public string Exclusions => Parent?.Exclusions ?? string.Empty;

    private string exclusions = string.Empty;
    public string ExclusionsLocal
    {
        get => exclusions;
        set
        {
            if (SetProperty(ref exclusions, value))
            {
                if (Parent is not null)
                {
                    Parent.Exclusions = value;
                    LogService.Write("EXCLUSION", $"📝 Synced to Parent.Exclusions ({value.Length} chars)");
                }
            }
        }
    }

    private bool hasSelectedFolder;
    public bool HasSelectedFolder
    {
        get => hasSelectedFolder;
        set => SetProperty(ref hasSelectedFolder, value);
    }

    public GlobalIgnoreViewModel GlobalIgnore { get; } = new();

    [RelayCommand]
    private async Task ShowExclusionInfo()
    {
        await Shell.Current.CurrentPage.DisplayAlert(
            "Exclude Patterns",
            "These patterns follow .gitignore rules. Use them to exclude folders or files from the tree view. Examples include:\n\n- node_modules/\n- *.log\n- bin/\n- obj/",
            "OK"
        );
    }

    [RelayCommand]
    private async Task ShowExclusionExamples()
    {
        await Shell.Current.CurrentPage.DisplayAlert(
            "Common Exclusion Patterns",
            "• node_modules/\n• *.log\n• bin/\n• obj/\n• .vs/\n• *.tmp\n• *.user\n• .DS_Store",
            "Got it"
        );
    }

    public string ExclusionPlaceholder =>
        "node_modules/\n*.log\nbin/\nobj/";
}