using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace TreeGlyph.UI.ViewModels.MainPageViewModel;

public partial class ToolbarViewModel : ObservableObject
{
    private MainViewModel? parent;

    public MainViewModel? Parent
    {
        get => parent;
        set
        {
            if (SetProperty(ref parent, value))
            {
                OnPropertyChanged(nameof(IsBusy));
                OnPropertyChanged(nameof(CurrentFolder));
                OnPropertyChanged(nameof(HasSelectedFolder));
                OnPropertyChanged(nameof(SelectedFolderPath));
                OnPropertyChanged(nameof(Exclusions));

                if (parent is not null)
                {
                    parent.PropertyChanged += (_, e) =>
                    {
                        switch (e.PropertyName)
                        {
                            case nameof(MainViewModel.IsBusy):
                                OnPropertyChanged(nameof(IsBusy));
                                break;
                            case nameof(MainViewModel.CurrentFolder):
                                OnPropertyChanged(nameof(CurrentFolder));
                                break;
                            case nameof(MainViewModel.HasSelectedFolder):
                                OnPropertyChanged(nameof(HasSelectedFolder));
                                break;
                            case nameof(MainViewModel.SelectedFolderPath):
                                OnPropertyChanged(nameof(SelectedFolderPath));
                                break;
                            case nameof(MainViewModel.Exclusions):
                                OnPropertyChanged(nameof(Exclusions));
                                break;
                        }
                    };
                }
            }
        }
    }

    public bool IsBusy => Parent?.IsBusy ?? false;
    public string? CurrentFolder => Parent?.CurrentFolder;
    public bool HasSelectedFolder => Parent?.HasSelectedFolder ?? false;
    public string SelectedFolderPath => Parent?.SelectedFolderPath ?? string.Empty;
    public string Exclusions => Parent?.Exclusions ?? string.Empty;

    public ICommand BrowseFolderCommand => Parent?.BrowseFolderCommand!;
    public ICommand GenerateTreeAsyncCommand => Parent?.GenerateTreeAsyncCommand!;
    public ICommand SaveTreeCommand => Parent?.SaveTreeCommand!;
    public ICommand ClearTreeCommand => Parent?.ClearTreeCommand!;
    public ICommand SaveIgnoreFileCommand => Parent?.SaveIgnoreFileCommand!;
    public ICommand ShowAboutCommand => Parent?.ShowAboutCommand!;
    public ICommand ViewLogsCommand => Parent?.ViewLogsCommand!;

}